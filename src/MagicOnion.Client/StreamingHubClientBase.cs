using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MagicOnion.Utils;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion.Server;
using System.Buffers;
using System.Linq;

namespace MagicOnion.Client
{
    public abstract class StreamingHubClientBase<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        const string StreamingHubVersionHeaderKey = "x-magiconion-streaminghub-version";
        const string StreamingHubVersionHeaderValue = "2";

        readonly string host;
        readonly CallOptions option;
        readonly CallInvoker callInvoker;
        readonly IMagicOnionClientLogger logger;

        protected readonly MessagePackSerializerOptions serializerOptions;
        readonly AsyncLock asyncLock = new AsyncLock();

        DuplexStreamingResult<byte[], byte[]> connection;
        protected TReceiver receiver;
        Task subscription;
        TaskCompletionSource<object> waitForDisconnect = new TaskCompletionSource<object>();

        // {messageId, TaskCompletionSource}
        ConcurrentDictionary<int, object> responseFutures = new ConcurrentDictionary<int, object>();
        protected CancellationTokenSource cts = new CancellationTokenSource();
        int messageId = 0;
        bool disposed;

        protected StreamingHubClientBase(CallInvoker callInvoker, string host, CallOptions option, MessagePackSerializerOptions serializerOptions, IMagicOnionClientLogger logger)
        {
            this.callInvoker = callInvoker;
            this.host = host;
            this.option = option;
            this.serializerOptions = serializerOptions;
            this.logger = logger ?? NullMagicOnionClientLogger.Instance;
        }

        protected abstract Method<byte[], byte[]> DuplexStreamingAsyncMethod { get; }

        // call immediately after create.
        public async Task __ConnectAndSubscribeAsync(TReceiver receiver, CancellationToken cancellationToken)
        {
            var syncContext = SynchronizationContext.Current; // capture SynchronizationContext.
            var callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, host, option);
            var streamingResult = new DuplexStreamingResult<byte[], byte[]>(
                callResult,
                new MarshallingClientStreamWriter<byte[]>(callResult.RequestStream, serializerOptions),
                new MarshallingAsyncStreamReader<byte[]>(callResult.ResponseStream, serializerOptions),
                serializerOptions
            );

            this.connection = streamingResult;
            this.receiver = receiver;

            // Establish StreamingHub connection between the client and the server.
            Metadata.Entry messageVersion = default;
            try
            {
                // The client can read the response headers before any StreamingHub's message.
                // MagicOnion.Server v4.0.x or before doesn't send any response headers. The client is incompatible with that versions.
                // NOTE: Grpc.Net:
                //           If the channel can not be connected, ResponseHeadersAsync will throw an exception.
                //       C-core:
                //           If the channel can not be connected, ResponseHeadersAsync will **return** an empty metadata.
                var headers = await streamingResult.ResponseHeadersAsync.ConfigureAwait(false);
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
                throw new RpcException(e.Status, $"Failed to connect to StreamingHub '{DuplexStreamingAsyncMethod.ServiceName}'. ({e.Status})");
            }

            var firstMoveNextTask = connection.RawStreamingCall.ResponseStream.MoveNext(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token);
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

        protected abstract void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data);
        protected abstract void OnBroadcastEvent(int methodId, ArraySegment<byte> data);

        async Task StartSubscribe(SynchronizationContext syncContext, Task<bool> firstMoveNext)
        {
            var reader = connection.RawStreamingCall.ResponseStream;
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
                            syncContext.Post(state => logger.Error((Exception)state, msg), ex);
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
                    syncContext.Post(state => logger.Error((Exception)state, msg), ex);
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
                    waitForDisconnect.TrySetResult(null);
                }
            }
        }

        // MessageFormat:
        // broadcast: [methodId, [argument]]
        // response:  [messageId, methodId, response]
        // error-response: [messageId, statusCode, detail, StringMessage]
        void ConsumeData(SynchronizationContext syncContext, byte[] data)
        {
            var messagePackReader = new MessagePackReader(data);
            var arrayLength = messagePackReader.ReadArrayHeader();
            if (arrayLength == 3)
            {
                var messageId = messagePackReader.ReadInt32();
                object future;
                if (responseFutures.TryRemove(messageId, out future))
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
                        if (!(future as ITaskCompletion).TrySetException(ex))
                        {
                            throw;
                        }
                    }
                }
            }
            else if (arrayLength == 4)
            {
                var messageId = messagePackReader.ReadInt32();
                object future;
                if (responseFutures.TryRemove(messageId, out future))
                {
                    var statusCode = messagePackReader.ReadInt32();
                    var detail = messagePackReader.ReadString();
                    var offset = (int)messagePackReader.Consumed;
                    var rest = new ArraySegment<byte>(data, offset, data.Length - offset);
                    var error = MessagePackSerializer.Deserialize<string>(rest, serializerOptions);
                    var ex = default(RpcException);
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        ex = new RpcException(new Status((StatusCode)statusCode, detail));
                    }
                    else
                    {
                        ex = new RpcException(new Status((StatusCode)statusCode, detail), detail + Environment.NewLine + error);
                    }

                    (future as ITaskCompletion).TrySetException(ex);
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
                        var t = (Tuple<int, byte[], int, int>)state;
                        OnBroadcastEvent(t.Item1, new ArraySegment<byte>(t.Item2, t.Item3, t.Item4));
                    }, tuple);
                }
                else
                {
                    OnBroadcastEvent(methodId, new ArraySegment<byte>(data, offset, data.Length - offset));
                }
            }
        }

        protected async Task WriteMessageAsync<T>(int methodId, T message)
        {
            ThrowIfDisposed();

            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);
                    writer.WriteArrayHeader(2);
                    writer.Write(methodId);
                    MessagePackSerializer.Serialize(ref writer, message, serializerOptions);
                    writer.Flush();
                    return buffer.WrittenSpan.ToArray();
                }
            }

            var v = BuildMessage();
            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await connection.RawStreamingCall.RequestStream.WriteAsync(v).ConfigureAwait(false);
            }
        }

        protected async Task<TResponse> WriteMessageAsyncFireAndForget<TRequest, TResponse>(int methodId, TRequest message)
        {
            await WriteMessageAsync(methodId, message).ConfigureAwait(false);
#pragma warning disable CS8603 // Possible null reference return.
            return default;
#pragma warning restore CS8603 // Possible null reference return.
        }

        protected async Task<TResponse> WriteMessageWithResponseAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var mid = Interlocked.Increment(ref messageId);
            var tcs = new TaskCompletionSourceEx<TResponse>(); // use Ex
            responseFutures[mid] = (object)tcs;

            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);
                    writer.WriteArrayHeader(3);
                    writer.Write(mid);
                    writer.Write(methodId);
                    MessagePackSerializer.Serialize(ref writer, message, serializerOptions);
                    writer.Flush();
                    return buffer.WrittenSpan.ToArray();
                }
            }

            var v = BuildMessage();
            using (await asyncLock.LockAsync().ConfigureAwait(false))
            {
                await connection.RawStreamingCall.RequestStream.WriteAsync(v).ConfigureAwait(false);
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
            if (connection.RawStreamingCall == null) return;

            disposed = true;

            try
            {
                await connection.RequestStream.CompleteAsync().ConfigureAwait(false);
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
                    List<Exception> aggregateException = null;
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
                                if (aggregateException != null)
                                {
                                    aggregateException = new List<Exception>();
                                    aggregateException.Add(ex);
                                }
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
