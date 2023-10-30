using System;
using System.Collections.Concurrent;
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
using MessagePack;

namespace MagicOnion.Client
{
    public abstract class StreamingHubClientBase<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
#pragma warning disable IDE1006 // Naming Styles
        const string StreamingHubVersionHeaderKey = "x-magiconion-streaminghub-version";
        const string StreamingHubVersionHeaderValue = "2";
#pragma warning restore IDE1006 // Naming Styles

        readonly string? host;
        readonly CallOptions option;
        readonly CallInvoker callInvoker;
        readonly IMagicOnionClientLogger logger;
        readonly IMagicOnionSerializer messageSerializer;
        readonly AsyncLock asyncLock = new AsyncLock();
        readonly Method<byte[], byte[]> duplexStreamingConnectMethod;

        IClientStreamWriter<byte[]> writer = default!;
        IAsyncStreamReader<byte[]> reader = default!;

        protected TReceiver receiver = default!;
        Task subscription = default!;

        TaskCompletionSource<bool> waitForDisconnect = new TaskCompletionSource<bool>();

        // {messageId, TaskCompletionSource}
        ConcurrentDictionary<int, ITaskCompletion> responseFutures = new ConcurrentDictionary<int, ITaskCompletion>();
        protected CancellationTokenSource cts = new CancellationTokenSource();
        int messageId = 0;
        bool disposed;

        protected StreamingHubClientBase(string serviceName, CallInvoker callInvoker, string? host, CallOptions option, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger)
        {
            this.duplexStreamingConnectMethod = CreateConnectMethod(serviceName);
            this.callInvoker = callInvoker ?? throw new ArgumentNullException(nameof(callInvoker));
            this.host = host;
            this.option = option;
            this.messageSerializer = serializerProvider?.Create(MethodType.DuplexStreaming, null) ?? throw new ArgumentNullException(nameof(serializerProvider));
            this.logger = logger ?? NullMagicOnionClientLogger.Instance;
        }

        // call immediately after create.
        public async Task __ConnectAndSubscribeAsync(TReceiver receiver, CancellationToken cancellationToken)
        {
            var syncContext = SynchronizationContext.Current; // capture SynchronizationContext.
            var callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(duplexStreamingConnectMethod, host, option);

            this.writer = callResult.RequestStream;
            this.reader = callResult.ResponseStream;
            this.receiver = receiver;

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

            var firstMoveNextTask = reader.MoveNext(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token);
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
        protected void SetResultForResponse<TResponse>(object taskCompletionSource, ArraySegment<byte> data)
            => ((TaskCompletionSource<TResponse>)taskCompletionSource).TrySetResult(Deserialize<TResponse>(data));
        protected void Serialize<T>(IBufferWriter<byte> writer, in T value)
            => messageSerializer.Serialize<T>(writer, value);
        protected T Deserialize<T>(ArraySegment<byte> bytes)
            => messageSerializer.Deserialize<T>(new ReadOnlySequence<byte>(bytes));

        protected abstract void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data);
        protected abstract void OnBroadcastEvent(int methodId, ArraySegment<byte> data);

        static Method<byte[], byte[]> CreateConnectMethod(string serviceName)
            => new Method<byte[], byte[]>(MethodType.DuplexStreaming, serviceName, "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        async Task StartSubscribe(SynchronizationContext? syncContext, Task<bool> firstMoveNext)
        {
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

                    moveNext = reader.MoveNext(cts.Token);
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
        void ConsumeData(SynchronizationContext? syncContext, byte[] data)
        {
            var messagePackReader = new MessagePackReader(data);
            var arrayLength = messagePackReader.ReadArrayHeader();
            if (arrayLength == 3)
            {
                var messageId = messagePackReader.ReadInt32();
                if (responseFutures.TryRemove(messageId, out var future))
                {
                    var methodId = messagePackReader.ReadInt32();
                    try
                    {
                        var offset = (int)messagePackReader.Consumed;
                        var rest = new ArraySegment<byte>(data, offset, data.Length - offset);
                        OnResponseEvent(methodId, future, rest);
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
            else if (arrayLength == 4)
            {
                var messageId = messagePackReader.ReadInt32();
                if (responseFutures.TryRemove(messageId, out var future))
                {
                    var statusCode = messagePackReader.ReadInt32();
                    var detail = messagePackReader.ReadString();
                    var offset = (int)messagePackReader.Consumed;
                    var error = messagePackReader.ReadString();
                    var ex = default(RpcException);
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        ex = new RpcException(new Status((StatusCode)statusCode, detail ?? string.Empty));
                    }
                    else
                    {
                        ex = new RpcException(new Status((StatusCode)statusCode, detail ?? string.Empty), detail + Environment.NewLine + error);
                    }

                    future.TrySetException(ex);
                }
            }
            else
            {
                var methodId = messagePackReader.ReadInt32();
                var offset = (int)messagePackReader.Consumed;
                if (syncContext != null)
                {
                    var tuple = Tuple.Create(methodId, data, offset, data.Length - offset);
                    syncContext.Post(state =>
                    {
                        var t = (Tuple<int, byte[], int, int>)state!;
                        OnBroadcastEvent(t.Item1, new ArraySegment<byte>(t.Item2, t.Item3, t.Item4));
                    }, tuple);
                }
                else
                {
                    OnBroadcastEvent(methodId, new ArraySegment<byte>(data, offset, data.Length - offset));
                }
            }
        }

        protected async Task<TResponse> WriteMessageFireAndForgetAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);
                    writer.WriteArrayHeader(2);
                    writer.Write(methodId);
                    writer.Flush();
                    Serialize(buffer, message);
                    return buffer.WrittenSpan.ToArray();
                }
            }

            var v = BuildMessage();
            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }

            return default!;
        }

        protected async Task<TResponse> WriteMessageWithResponseAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var mid = Interlocked.Increment(ref messageId);
            // NOTE: The continuations (user code) should be executed asynchronously. (Except: Unity WebGL)
            //       This is because the continuation may block the thread, for example, Console.ReadLine().
            //       If the thread is blocked, it will no longer return to the message consuming loop.
            var tcs = new TaskCompletionSourceEx<TResponse>(
#if !UNITY_WEBGL
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
            );
            responseFutures[mid] = tcs;

            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);
                    writer.WriteArrayHeader(3);
                    writer.Write(mid);
                    writer.Write(methodId);
                    writer.Flush();
                    Serialize(buffer, message);
                    return buffer.WrittenSpan.ToArray();
                }
            }

            var v = BuildMessage();
            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await writer.WriteAsync(v).ConfigureAwait(false);
            }

            return await tcs.Task.ConfigureAwait(false); // wait until server return response(or error). if connection was closed, throws cancellation from DisposeAsyncCore.
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
                cts.Cancel();
                cts.Dispose();
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
    }
}
