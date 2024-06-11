using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using System.Linq;
using Grpc.Core;
using MagicOnion.Client.Internal.Threading;
using MagicOnion.Client.Internal.Threading.Tasks;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Internal.Buffers;

namespace MagicOnion.Client
{
    public class StreamingHubClientOptions
    {
        public string? Host { get; }
        public CallOptions CallOptions { get; }
        public IMagicOnionSerializerProvider SerializerProvider { get; }
        public IMagicOnionClientLogger Logger { get; }

        public TimeSpan? HeartbeatInterval { get; }
        public Action<ReadOnlyMemory<byte>>? HeartbeatReceivedFromServer { get; }

        public StreamingHubClientOptions(string? host, CallOptions callOptions, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger)
            : this(host, callOptions, serializerProvider, logger, default, default)
        {
        }

        public StreamingHubClientOptions(string? host, CallOptions callOptions, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger, TimeSpan? heartbeatInterval, Action<ReadOnlyMemory<byte>>? heartbeatReceivedFromServer)
        {
            Host = host;
            CallOptions = callOptions;
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            HeartbeatInterval = heartbeatInterval;
            HeartbeatReceivedFromServer = heartbeatReceivedFromServer;
        }

        public static StreamingHubClientOptions CreateWithDefault(string? host = default, CallOptions callOptions = default, IMagicOnionSerializerProvider? serializerProvider = default, IMagicOnionClientLogger? logger = default)
            => new(host, callOptions, serializerProvider ?? MagicOnionSerializerProvider.Default, logger ?? NullMagicOnionClientLogger.Instance);

        public StreamingHubClientOptions WithHost(string? host)
            => new(host, CallOptions, SerializerProvider, Logger, HeartbeatInterval, HeartbeatReceivedFromServer);
        public StreamingHubClientOptions WithCallOptions(CallOptions callOptions)
            => new(Host, callOptions, SerializerProvider, Logger, HeartbeatInterval, HeartbeatReceivedFromServer);
        public StreamingHubClientOptions WithSerializerProvider(IMagicOnionSerializerProvider serializerProvider)
            => new(Host, CallOptions, serializerProvider, Logger, HeartbeatInterval, HeartbeatReceivedFromServer);
        public StreamingHubClientOptions WithLogger(IMagicOnionClientLogger logger)
            => new(Host, CallOptions, SerializerProvider, logger, HeartbeatInterval, HeartbeatReceivedFromServer);
        public StreamingHubClientOptions WithHeartbeatInterval(TimeSpan? interval)
            => new(Host, CallOptions, SerializerProvider, Logger, interval, HeartbeatReceivedFromServer);
        public StreamingHubClientOptions WithHeartbeatReceived(Action<ReadOnlyMemory<byte>>? onHeartbeatReceived)
            => new(Host, CallOptions, SerializerProvider, Logger, HeartbeatInterval, onHeartbeatReceived);
    }

    public abstract class StreamingHubClientBase<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
#pragma warning disable IDE1006 // Naming Styles
        const string StreamingHubVersionHeaderKey = "x-magiconion-streaminghub-version";
        const string StreamingHubVersionHeaderValue = "2";
#pragma warning restore IDE1006 // Naming Styles

        readonly CallInvoker callInvoker;
        readonly StreamingHubClientOptions options;
        readonly IMagicOnionClientLogger logger;
        readonly IMagicOnionSerializer messageSerializer;
        readonly AsyncLock asyncLock = new();
        readonly Method<StreamingHubPayload, StreamingHubPayload> duplexStreamingConnectMethod;
        // {messageId, TaskCompletionSource}
        readonly Dictionary<int, ITaskCompletion> responseFutures = new();
        readonly TaskCompletionSource<bool> waitForDisconnect = new();
        readonly CancellationTokenSource cancellationTokenSource = new();

        int messageIdSequence = 0;
        bool disposed;

        Task? heartbeatTask;
        DateTimeOffset lastHeartbeatSentAt;

        IClientStreamWriter<StreamingHubPayload> writer = default!;
        IAsyncStreamReader<StreamingHubPayload> reader = default!;

        Task subscription = default!;

        protected readonly TReceiver receiver;

        protected StreamingHubClientBase(string serviceName, TReceiver receiver, CallInvoker callInvoker, StreamingHubClientOptions options)
        {
            this.callInvoker = callInvoker;
            this.receiver = receiver;
            this.options = options;
            this.logger = options.Logger;
            this.duplexStreamingConnectMethod = CreateConnectMethod(serviceName);
            this.messageSerializer = options.SerializerProvider.Create(MethodType.DuplexStreaming, null);
        }

        // call immediately after create.
        public async Task __ConnectAndSubscribeAsync(CancellationToken cancellationToken)
        {
            var syncContext = SynchronizationContext.Current; // capture SynchronizationContext.
            var callResult = callInvoker.AsyncDuplexStreamingCall(duplexStreamingConnectMethod, options.Host, options.CallOptions);

            this.writer = callResult.RequestStream;
            this.reader = callResult.ResponseStream;

            // Establish StreamingHub connection between the client and the server.
            Metadata.Entry? messageVersion;
            try
            {
                // The client can read the response headers before any StreamingHub's message.
                // MagicOnion.Server v4.0.x or before doesn't send any response headers. The client is incompatible with that versions.
                // NOTE: Grpc.Net:
                //           If the channel can not be connected, ResponseHeadersAsync will throw an exception.
                //       C-core:
                //           If the channel can not be connected, ResponseHeadersAsync will **return** an empty metadata.
                var headers = await callResult.ResponseHeadersAsync.ConfigureAwait(false);
                messageVersion = headers.FirstOrDefault(x => x.Key == StreamingHubVersionHeaderKey);

                cancellationToken.ThrowIfCancellationRequested();

                // Check message version of StreamingHub.
                if (messageVersion != null && messageVersion.Value != StreamingHubVersionHeaderValue)
                {
                    throw new RpcException(new Status(StatusCode.Internal, $"The message version of StreamingHub mismatch between the client and the server. (ServerVersion={messageVersion?.Value}; Expected={StreamingHubVersionHeaderValue})"));
                }
            }
            catch (RpcException e)
            {
                throw new RpcException(e.Status, $"Failed to connect to StreamingHub '{duplexStreamingConnectMethod.ServiceName}'. ({e.Status})");
            }

            var firstMoveNextTask = reader.MoveNext(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token).Token);
            if (firstMoveNextTask.IsFaulted || messageVersion == null)
            {
                // NOTE: Grpc.Net:
                //           If an error is returned from `StreamingHub.Connect` method on a server-side,
                //           ResponseStream.MoveNext synchronously returns a task that is `IsFaulted = true`.
                //           `ConnectAsync` method should throw an exception here immediately.
                //       C-core:
                //           `firstMoveNextTask` is incomplete task (`IsFaulted = false`) whether ResponseHeadersAsync is failed or not.
                //           If the channel is disconnected or the server returns an error (StatusCode != OK), awaiting the Task will throw an exception.
                await firstMoveNextTask.ConfigureAwait(false);

                // NOTE: C-core: If the execution reaches here, Connect method returns without any error (StatusCode = OK). but MessageVersion isn't provided from the server.
                throw new RpcException(new Status(StatusCode.Internal, $"The request started successfully (StatusCode = OK), but the StreamingHub client has failed to negotiate with the server."));
            }

            this.subscription = StartSubscribe(syncContext, firstMoveNextTask);
        }

        // Helper methods to make building clients easy.
        protected void SetResultForResponse<TResponse>(object taskCompletionSource, ReadOnlyMemory<byte> data)
            => ((TaskCompletionSource<TResponse>)taskCompletionSource).TrySetResult(Deserialize<TResponse>(data));
        protected void Serialize<T>(IBufferWriter<byte> writer, in T value)
            => messageSerializer.Serialize<T>(writer, value);
        protected T Deserialize<T>(ReadOnlyMemory<byte> data)
            => messageSerializer.Deserialize<T>(new ReadOnlySequence<byte>(data));

        protected abstract void OnClientResultEvent(int methodId, Guid messageId, ReadOnlyMemory<byte> data);
        protected abstract void OnResponseEvent(int methodId, object taskCompletionSource, ReadOnlyMemory<byte> data);
        protected abstract void OnBroadcastEvent(int methodId, ReadOnlyMemory<byte> data);

        static Method<StreamingHubPayload, StreamingHubPayload> CreateConnectMethod(string serviceName)
            => new (MethodType.DuplexStreaming, serviceName, "Connect", MagicOnionMarshallers.StreamingHubMarshaller, MagicOnionMarshallers.StreamingHubMarshaller);

        async Task StartSubscribe(SynchronizationContext? syncContext, Task<bool> firstMoveNext)
        {
            if (options.HeartbeatInterval is { } heartbeatInterval)
            {
                heartbeatTask = RunHeartbeatLoopAsync(heartbeatInterval, cancellationTokenSource.Token);
            }

            var reader = this.reader;
            try
            {
                var moveNext = firstMoveNext;
                while (await moveNext.ConfigureAwait(false)) // avoid Post to SyncContext(it losts one-frame per operation)
                {
                    try
                    {
                        ConsumeData(syncContext, reader.Current);
                    }
                    catch (Exception ex)
                    {
                        const string msg = "An error occurred when consuming a received message, but the subscription is still alive.";
                        // log post on main thread.
                        if (syncContext != null)
                        {
                            syncContext.Post(state => logger.Error((Exception)state!, msg), ex);
                        }
                        else
                        {
                            logger.Error(ex, msg);
                        }
                    }

                    moveNext = reader.MoveNext(cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    return;
                }
                const string msg = "An error occurred while subscribing to messages.";
                // log post on main thread.
                if (syncContext != null)
                {
                    syncContext.Post(state => logger.Error((Exception)state!, msg), ex);
                }
                else
                {
                    logger.Error(ex, msg);
                }
            }
            finally
            {
                try
                {
#if !UNITY_WEBGL
                    // set syncContext before await
                    // NOTE: If restore SynchronizationContext in WebGL environment, a continuation will not be executed inline and will be stuck.
                    if (syncContext != null && SynchronizationContext.Current == null)
                    {
                        SynchronizationContext.SetSynchronizationContext(syncContext);
                    }
#endif
                    await DisposeAsyncCore(false).ConfigureAwait(false);
                }
                finally
                {
                    waitForDisconnect.TrySetResult(true);
                }
            }
        }

        // MessageFormat:
        // broadcast: [methodId, [argument]]
        // response:  [messageId, methodId, response]
        // error-response: [messageId, statusCode, detail, StringMessage]
        void ConsumeData(SynchronizationContext? syncContext, StreamingHubPayload payload)
        {
            var messageReader = new StreamingHubClientMessageReader(payload.Memory);
            switch (messageReader.ReadMessageType())
            {
                case StreamingHubMessageType.Broadcast:
                    {
                        var message = messageReader.ReadBroadcastMessage();
                        if (syncContext != null)
                        {
                            var tuple = Tuple.Create(this, message.MethodId, payload, message.Body);
                            syncContext.Post(static state =>
                            {
                                var t = (Tuple<StreamingHubClientBase<TStreamingHub, TReceiver>, int, StreamingHubPayload, ReadOnlyMemory<byte>>)state!;
                                t.Item1.OnBroadcastEvent(t.Item2, t.Item4);
                                StreamingHubPayloadPool.Shared.Return(t.Item3);
                            }, tuple);
                        }
                        else
                        {
                            OnBroadcastEvent(message.MethodId, message.Body);
                            StreamingHubPayloadPool.Shared.Return(payload);
                        }

                    }
                    break;
                case StreamingHubMessageType.Response:
                    {
                        var message = messageReader.ReadResponseMessage();
                        lock (responseFutures)
                        {
                            if (responseFutures.TryGetValue(message.MessageId, out var future))
                            {
                                responseFutures.Remove(message.MessageId);
                                try
                                {
                                    OnResponseEvent(message.MethodId, future, message.Body);
                                    StreamingHubPayloadPool.Shared.Return(payload);
                                }
                                catch (Exception ex)
                                {
                                    if (!future.TrySetException(ex))
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case StreamingHubMessageType.ResponseWithError:
                    {
                        var message = messageReader.ReadResponseWithErrorMessage();
                        lock (responseFutures)
                        {
                            if (responseFutures.TryGetValue(message.MessageId, out var future))
                            {
                                responseFutures.Remove(message.MessageId);

                                RpcException ex;
                                if (string.IsNullOrWhiteSpace(message.Error))
                                {
                                    ex = new RpcException(new Status((StatusCode)message.StatusCode, message.Detail ?? string.Empty));
                                }
                                else
                                {
                                    ex = new RpcException(new Status((StatusCode)message.StatusCode, message.Detail ?? string.Empty), message.Detail + Environment.NewLine + message.Error);
                                }

                                future.TrySetException(ex);
                            }
                        }
                    }
                    break;
                case StreamingHubMessageType.ClientResultRequest:
                    {
                        var message = messageReader.ReadClientResultRequestMessage();

                        if (syncContext != null)
                        {
                            var tuple = Tuple.Create(this, message.MethodId, message.ClientResultRequestMessageId, message.Body, payload);
                            syncContext.Post(static state =>
                            {
                                var t = (Tuple<StreamingHubClientBase<TStreamingHub, TReceiver>, int, Guid, ReadOnlyMemory<byte>, StreamingHubPayload>)state!;
                                t.Item1.OnClientResultEvent(t.Item2, t.Item3, t.Item4);
                                StreamingHubPayloadPool.Shared.Return(t.Item5);
                            }, tuple);
                        }
                        else
                        {
                            OnClientResultEvent(message.MethodId, message.ClientResultRequestMessageId, message.Body);
                            StreamingHubPayloadPool.Shared.Return(payload);
                        }
                    }
                    break;
                case StreamingHubMessageType.Heartbeat:
                    {
                        var metadata = messageReader.ReadHeartbeat();
                        if (this.options.HeartbeatReceivedFromServer is { } heartbeatReceived)
                        {
                            if (syncContext != null)
                            {
                                var tuple = Tuple.Create(heartbeatReceived, metadata, payload);
                                syncContext.Post(static state =>
                                {
                                    var t = (Tuple<Action<ReadOnlyMemory<byte>>, ReadOnlyMemory<byte>, StreamingHubPayload>)state!;
                                    t.Item1(t.Item2);
                                    StreamingHubPayloadPool.Shared.Return(t.Item3);
                                }, tuple);
                            }
                            else
                            {
                                heartbeatReceived(metadata);
                                StreamingHubPayloadPool.Shared.Return(payload);
                            }
                        }
                        _ = WriteHeartbeatAsync();
                    }
                    break;
            }
        }

        async Task RunHeartbeatLoopAsync(TimeSpan heartbeatInterval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(heartbeatInterval, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                if ((DateTimeOffset.UtcNow - lastHeartbeatSentAt) > heartbeatInterval)
                {
                    await WriteHeartbeatAsync().ConfigureAwait(false);
                }
            }
        }

        async Task WriteHeartbeatAsync()
        {
            if (disposed) return;
            var v = BuildHeartbeatMessage();

            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }

            lastHeartbeatSentAt = DateTimeOffset.UtcNow;
        }

        protected async Task<TResponse> WriteMessageFireAndForgetAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var v = BuildRequestMessage(methodId, message);

            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }

            return default!;
        }

        protected async Task<TResponse> WriteMessageWithResponseAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var mid = Interlocked.Increment(ref messageIdSequence);
            // NOTE: The continuations (user code) should be executed asynchronously. (Except: Unity WebGL)
            //       This is because the continuation may block the thread, for example, Console.ReadLine().
            //       If the thread is blocked, it will no longer return to the message consuming loop.
            var tcs = new TaskCompletionSourceEx<TResponse>(
#if !UNITY_WEBGL
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
            );
            responseFutures[mid] = tcs;

            var v = BuildRequestMessage(methodId, mid, message);

            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }

            return await tcs.Task.ConfigureAwait(false); // wait until server return response(or error). if connection was closed, throws cancellation from DisposeAsyncCore.

        }

        protected void AwaitAndWriteClientResultResponseMessage(int methodId, Guid clientResultMessageId, Task task)
            => AwaitAndWriteClientResultResponseMessage(methodId, clientResultMessageId, new ValueTask(task));

        protected async void AwaitAndWriteClientResultResponseMessage(int methodId, Guid clientResultMessageId, ValueTask task)
        {
            try
            {
                await task.ConfigureAwait(false);
                await WriteClientResultResponseMessageAsync(methodId, clientResultMessageId, MessagePack.Nil.Default).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await WriteClientResultResponseMessageForErrorAsync(methodId, clientResultMessageId, e).ConfigureAwait(false);
            }
        }

        protected void AwaitAndWriteClientResultResponseMessage<T>(int methodId, Guid clientResultMessageId, Task<T> task)
            => AwaitAndWriteClientResultResponseMessage(methodId, clientResultMessageId, new ValueTask<T>(task));

        protected async void AwaitAndWriteClientResultResponseMessage<T>(int methodId, Guid clientResultMessageId, ValueTask<T> task)
        {
            try
            {
                var result = await task.ConfigureAwait(false);
                await WriteClientResultResponseMessageAsync(methodId, clientResultMessageId, result).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await WriteClientResultResponseMessageForErrorAsync(methodId, clientResultMessageId, e).ConfigureAwait(false);
            }
        }

        protected async void WriteClientResultResponseMessageForError(int methodId, Guid clientResultMessageId, Exception ex)
        {
            try
            {
                await WriteClientResultResponseMessageForErrorAsync(methodId, clientResultMessageId, ex).ConfigureAwait(false);
            }
            catch
            {
                // Ignore Exception
            }
        }

        protected async Task WriteClientResultResponseMessageAsync<T>(int methodId, Guid clientResultMessageId, T result)
        {
            var v = BuildClientResultResponseMessage(methodId, clientResultMessageId, result);
            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }
        }

        protected async Task WriteClientResultResponseMessageForErrorAsync(int methodId, Guid clientResultMessageId, Exception ex)
        {
            var statusCode = ex is RpcException rpcException
                ? rpcException.StatusCode
                : StatusCode.Internal;

            var v = BuildClientResultResponseMessageForError(methodId, clientResultMessageId, (int)statusCode, ex.Message, ex);
            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }
        }

        void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("StreamingHubClient", $"The StreamingHub has already been disconnected from the server.");
            }
        }

        public Task WaitForDisconnect()
        {
            return waitForDisconnect.Task;
        }

        public Task DisposeAsync()
        {
            return DisposeAsyncCore(true);
        }

        async Task DisposeAsyncCore(bool waitSubscription)
        {
            if (disposed) return;
            if (writer == null) return;

            disposed = true;

            try
            {
                await writer.CompleteAsync().ConfigureAwait(false);
            }
            catch { } // ignore error?
            finally
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                try
                {
                    if (waitSubscription)
                    {
                        if (subscription != null)
                        {
                            await subscription.ConfigureAwait(false);
                        }
                    }

                    // cleanup completion
                    List<Exception>? aggregateException = null;
                    foreach (var item in responseFutures)
                    {
                        try
                        {
                            (item.Value as ITaskCompletion).TrySetCanceled();
                        }
                        catch (Exception ex)
                        {
                            if (!(ex is OperationCanceledException))
                            {
                                aggregateException ??= new List<Exception>();
                                aggregateException.Add(ex);
                            }
                        }
                    }
                    if (aggregateException != null)
                    {
                        throw new AggregateException(aggregateException);
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is OperationCanceledException))
                    {
                        throw;
                    }
                }
            }
        }

        StreamingHubPayload BuildRequestMessage<T>(int methodId, T message)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteRequestMessageVoid(buffer, methodId, message, messageSerializer);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildRequestMessage<T>(int methodId, int messageId, T message)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteRequestMessage(buffer, methodId, messageId, message, messageSerializer);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildClientResultResponseMessage<T>(int methodId, Guid messageId, T response)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteClientResultResponseMessage(buffer, methodId, messageId, response, messageSerializer);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildClientResultResponseMessageForError(int methodId, Guid messageId, int statusCode, string detail, Exception? ex)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteClientResultResponseMessageForError(buffer, methodId, messageId, statusCode, detail, ex, messageSerializer);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildHeartbeatMessage()
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteHeartbeatMessageForClientToServer(buffer);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }
    }
}
