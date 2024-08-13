using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using Grpc.Core;
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

        public TimeSpan? ClientHeartbeatInterval { get; }
        public TimeSpan? ClientHeartbeatTimeout { get; }
        public Action<ServerHeartbeatEvent>? OnServerHeartbeatReceived { get; }
        public Action<ClientHeartbeatEvent>? OnClientHeartbeatResponseReceived { get; }
#if NET8_0_OR_GREATER
        public TimeProvider? TimeProvider { get; }
#endif

        public StreamingHubClientOptions(string? host, CallOptions callOptions, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger)
#if NET8_0_OR_GREATER
            : this(host, callOptions, serializerProvider, logger, default, default, default, default, default)
#else
            : this(host, callOptions, serializerProvider, logger, default, default, default, default)
#endif
        {
        }

#if NET8_0_OR_GREATER
        public StreamingHubClientOptions(string? host, CallOptions callOptions, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger, TimeSpan? clientHeartbeatInterval, TimeSpan? clientHeartbeatTimeout, Action<ServerHeartbeatEvent>? onServerHeartbeatReceived, Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived,TimeProvider? timeProvider)
#else
        public StreamingHubClientOptions(string? host, CallOptions callOptions, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger, TimeSpan? clientHeartbeatInterval, TimeSpan? clientHeartbeatTimeout, Action<ServerHeartbeatEvent>? onServerHeartbeatReceived, Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived)
#endif
        {
            Host = host;
            CallOptions = callOptions;
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ClientHeartbeatInterval = clientHeartbeatInterval;
            ClientHeartbeatTimeout = clientHeartbeatTimeout;
            OnServerHeartbeatReceived = onServerHeartbeatReceived;
            OnClientHeartbeatResponseReceived = onClientHeartbeatResponseReceived;
#if NET8_0_OR_GREATER
            TimeProvider = timeProvider;
#endif
        }

        public static StreamingHubClientOptions CreateWithDefault(string? host = default, CallOptions callOptions = default, IMagicOnionSerializerProvider? serializerProvider = default, IMagicOnionClientLogger? logger = default)
            => new(host, callOptions, serializerProvider ?? MagicOnionSerializerProvider.Default, logger ?? NullMagicOnionClientLogger.Instance);

        public StreamingHubClientOptions WithHost(string? host)
            => new(host, CallOptions, SerializerProvider, Logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );
        public StreamingHubClientOptions WithCallOptions(CallOptions callOptions)
            => new(Host, callOptions, SerializerProvider, Logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );
        public StreamingHubClientOptions WithSerializerProvider(IMagicOnionSerializerProvider serializerProvider)
            => new(
                Host, CallOptions, serializerProvider, Logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );
        public StreamingHubClientOptions WithLogger(IMagicOnionClientLogger logger)
            => new(Host, CallOptions, SerializerProvider, logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );

        /// <summary>
        /// Sets a heartbeat interval. If a value is <see keyword="null"/>, the heartbeat from the client is disabled.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public StreamingHubClientOptions WithClientHeartbeatInterval(TimeSpan? interval)
            => new(Host, CallOptions, SerializerProvider, Logger
                , interval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );

        /// <summary>
        /// Sets a heartbeat timeout period. If a value is <see keyword="null"/>, the client does not time out.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public StreamingHubClientOptions WithClientHeartbeatTimeout(TimeSpan? timeout)
            => new(Host, CallOptions, SerializerProvider, Logger
                , ClientHeartbeatInterval, timeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );

        /// <summary>
        /// Sets a heartbeat callback. If additional metadata is provided by the server in the heartbeat message, this metadata is provided as an argument.
        /// </summary>
        /// <param name="onServerHeartbeatReceived"></param>
        /// <returns></returns>
        public StreamingHubClientOptions WithServerHeartbeatReceived(Action<ServerHeartbeatEvent>? onServerHeartbeatReceived)
            => new(Host, CallOptions, SerializerProvider, Logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, onServerHeartbeatReceived, OnClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );

        /// <summary>
        /// Sets a client heartbeat response callback.
        /// </summary>
        /// <param name="onClientHeartbeatResponseReceived"></param>
        /// <returns></returns>
        public StreamingHubClientOptions WithClientHeartbeatResponseReceived(Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived)
            => new(Host, CallOptions, SerializerProvider, Logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, onClientHeartbeatResponseReceived
#if NET8_0_OR_GREATER
                , TimeProvider
#endif
            );

#if NET8_0_OR_GREATER
        /// <summary>
        /// Sets a <see cref="TimeProvider"/>
        /// </summary>
        /// <param name="timeProvider"></param>
        /// <returns></returns>
        public StreamingHubClientOptions WithTimeProvider(TimeProvider timeProvider)
            => new(Host, CallOptions, SerializerProvider, Logger
                , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
                , timeProvider
            );
#endif
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
        readonly Method<StreamingHubPayload, StreamingHubPayload> duplexStreamingConnectMethod;
        // {messageId, TaskCompletionSource}
        readonly Dictionary<int, IStreamingHubResponseTaskSource> responseFutures = new();
        readonly TaskCompletionSource<bool> waitForDisconnect = new();
        readonly CancellationTokenSource cancellationTokenSource = new();
        readonly Dictionary<int, SendOrPostCallback> postCallbackCache = new();


        int messageIdSequence = 0;
        bool disposed;

        readonly Channel<StreamingHubPayload> writerQueue = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = false });
        Task? writerTask;
        IClientStreamWriter<StreamingHubPayload> writer = default!;
        IAsyncStreamReader<StreamingHubPayload> reader = default!;

        StreamingHubClientHeartbeatManager heartbeatManager = default!;
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
        internal async Task __ConnectAndSubscribeAsync(CancellationToken cancellationToken)
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
        protected void SetResultForResponse<TResponse>(object taskSource, ReadOnlyMemory<byte> data)
            => ((StreamingHubResponseTaskSource<TResponse>)taskSource).SetResult(Deserialize<TResponse>(data));
        protected void Serialize<T>(IBufferWriter<byte> writer, in T value)
            => messageSerializer.Serialize<T>(writer, value);
        protected T Deserialize<T>(ReadOnlyMemory<byte> data)
            => messageSerializer.Deserialize<T>(new ReadOnlySequence<byte>(data));

        protected abstract void OnClientResultEvent(int methodId, Guid messageId, ReadOnlyMemory<byte> data);
        protected abstract void OnResponseEvent(int methodId, object taskSource, ReadOnlyMemory<byte> data);
        protected abstract void OnBroadcastEvent(int methodId, ReadOnlyMemory<byte> data);

        static Method<StreamingHubPayload, StreamingHubPayload> CreateConnectMethod(string serviceName)
            => new (MethodType.DuplexStreaming, serviceName, "Connect", MagicOnionMarshallers.StreamingHubMarshaller, MagicOnionMarshallers.StreamingHubMarshaller);

        async Task StartSubscribe(SynchronizationContext? syncContext, Task<bool> firstMoveNext)
        {
            var cancellationToken = cancellationTokenSource.Token;

            heartbeatManager = new StreamingHubClientHeartbeatManager(
                writerQueue.Writer,
                options.ClientHeartbeatInterval ?? TimeSpan.Zero /* Disable */,
                options.ClientHeartbeatTimeout ?? Timeout.InfiniteTimeSpan,
                options.OnServerHeartbeatReceived,
                options.OnClientHeartbeatResponseReceived,
                syncContext,
                cancellationTokenSource.Token
#if NET8_0_OR_GREATER
                , options.TimeProvider ?? TimeProvider.System
#endif
            );

            // Activate the Heartbeat Manager if enabled in the options.
            if (options.ClientHeartbeatInterval is {} heartbeatInterval && heartbeatInterval > TimeSpan.Zero)
            {
                heartbeatManager.StartClientHeartbeatLoop();
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(heartbeatManager.TimeoutToken, cancellationTokenSource.Token).Token;
            }

            writerTask = RunWriterLoopAsync(cancellationToken);

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

                    moveNext = reader.MoveNext(cancellationToken);
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
                heartbeatManager.Dispose();

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

        void ConsumeData(SynchronizationContext? syncContext, StreamingHubPayload payload)
        {
            var messageReader = new StreamingHubClientMessageReader(payload.Memory);
            switch (messageReader.ReadMessageType())
            {
                case StreamingHubMessageType.Broadcast:
                    ProcessBroadcast(syncContext, payload, ref messageReader);
                    break;
                case StreamingHubMessageType.Response:
                    ProcessResponse(syncContext, payload, ref messageReader);
                    break;
                case StreamingHubMessageType.ResponseWithError:
                    ProcessResponseWithError(syncContext, payload, ref messageReader);
                    break;
                case StreamingHubMessageType.ClientResultRequest:
                    ProcessClientResultRequest(syncContext, payload, ref messageReader);
                    break;
                case StreamingHubMessageType.ServerHeartbeat:
                    heartbeatManager.ProcessServerHeartbeat(payload);
                    break;
                case StreamingHubMessageType.ClientHeartbeatResponse:
                    heartbeatManager.ProcessClientHeartbeatResponse(payload);
                    break;
            }
        }

        void ProcessBroadcast(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
        {
            if (syncContext is null)
            {
                var message = messageReader.ReadBroadcastMessage();
                OnBroadcastEvent(message.MethodId, message.Body);
                StreamingHubPayloadPool.Shared.Return(payload);
            }
            else
            {
                var (methodId, consumed) = messageReader.ReadBroadcastMessageMethodId();
                if (!postCallbackCache.TryGetValue(methodId, out var postCallback))
                {
                    // Create and cache a callback delegate capturing `this` and the header size.
                    postCallback = postCallbackCache[methodId] = CreateBroadcastCallback(methodId, consumed);
                }
                syncContext.Post(postCallback, payload);
            }
        }

        SendOrPostCallback CreateBroadcastCallback(int methodId, int consumed)
        {
            return (state) =>
            {
                var p = (StreamingHubPayload)state!;
                this.OnBroadcastEvent(methodId, p.Memory.Slice(consumed));
                StreamingHubPayloadPool.Shared.Return(p);
            };
        }

        void ProcessResponse(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
        {
            var message = messageReader.ReadResponseMessage();

            IStreamingHubResponseTaskSource? future;
            lock (responseFutures)
            {
                if (!responseFutures.Remove(message.MessageId, out future))
                {
                    return;
                }
            }

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

        void ProcessResponseWithError(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
        {
            var message = messageReader.ReadResponseWithErrorMessage();

            IStreamingHubResponseTaskSource? future;
            lock (responseFutures)
            {
                if (!responseFutures.Remove(message.MessageId, out future))
                {
                    return;
                }
            }

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
            StreamingHubPayloadPool.Shared.Return(payload);
        }

        void ProcessClientResultRequest(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
        {
            var message = messageReader.ReadClientResultRequestMessage();
            if (syncContext is null)
            {
                OnClientResultEvent(message.MethodId, message.ClientResultRequestMessageId, message.Body);
                StreamingHubPayloadPool.Shared.Return(payload);
            }
            else
            {
                var tuple = Tuple.Create(this, message.MethodId, message.ClientResultRequestMessageId, message.Body, payload);
                syncContext.Post(static state =>
                {
                    var t = (Tuple<StreamingHubClientBase<TStreamingHub, TReceiver>, int, Guid, ReadOnlyMemory<byte>, StreamingHubPayload>)state!;
                    t.Item1.OnClientResultEvent(t.Item2, t.Item3, t.Item4);
                    StreamingHubPayloadPool.Shared.Return(t.Item5);
                }, tuple);
            }
        }

        async Task RunWriterLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await writerQueue.Reader.WaitToReadAsync(default).ConfigureAwait(false))
                {
                    while (writerQueue.Reader.TryRead(out var payload))
                    {
                        await writer.WriteAsync(payload).ConfigureAwait(false);
                    }
                }
            }
        }

        protected Task<TResponse> WriteMessageFireAndForgetTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
            => WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(methodId, message).AsTask();

        protected ValueTask<TResponse> WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var v = BuildRequestMessage(methodId, message);
            _ = writerQueue.Writer.TryWrite(v);

            return default;
        }

        protected ValueTask WriteMessageFireAndForgetValueTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(methodId, message);
            return default;
        }

        protected Task<TResponse> WriteMessageWithResponseTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
            => WriteMessageWithResponseValueTaskOfTAsync<TRequest, TResponse>(methodId, message).AsTask();

        protected ValueTask<TResponse> WriteMessageWithResponseValueTaskOfTAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var mid = Interlocked.Increment(ref messageIdSequence);

            var taskSource = StreamingHubResponseTaskSourcePool<TResponse>.Shared.RentOrCreate();
            lock (responseFutures)
            {
                responseFutures[mid] = taskSource;
            }

            var v = BuildRequestMessage(methodId, mid, message);
            _ = writerQueue.Writer.TryWrite(v);

            return new ValueTask<TResponse>(taskSource, taskSource.Version); // wait until server return response(or error). if connection was closed, throws cancellation from DisposeAsyncCore.
        }

        protected ValueTask WriteMessageWithResponseValueTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var mid = Interlocked.Increment(ref messageIdSequence);

            var taskSource = StreamingHubResponseTaskSourcePool<TResponse>.Shared.RentOrCreate();
            lock (responseFutures)
            {
                responseFutures[mid] = taskSource;
            }

            var v = BuildRequestMessage(methodId, mid, message);
            _ = writerQueue.Writer.TryWrite(v);

            return new ValueTask(taskSource, taskSource.Version); // wait until server return response(or error). if connection was closed, throws cancellation from DisposeAsyncCore.
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

        protected Task WriteClientResultResponseMessageAsync<T>(int methodId, Guid clientResultMessageId, T result)
        {
            var v = BuildClientResultResponseMessage(methodId, clientResultMessageId, result);
            _ = writerQueue.Writer.TryWrite(v);
            return Task.CompletedTask;
        }

        protected Task WriteClientResultResponseMessageForErrorAsync(int methodId, Guid clientResultMessageId, Exception ex)
        {
            var statusCode = ex is RpcException rpcException
                ? rpcException.StatusCode
                : StatusCode.Internal;

            var v = BuildClientResultResponseMessageForError(methodId, clientResultMessageId, (int)statusCode, ex.Message, ex);
            _ = writerQueue.Writer.TryWrite(v);

            return Task.CompletedTask;
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
                writerQueue.Writer.Complete();
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
                            (item.Value as IStreamingHubResponseTaskSource).TrySetCanceled();
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
    }
}
