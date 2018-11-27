using Grpc.Core;
using Grpc.Core.Logging;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MagicOnion.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    public abstract class StreamingHubClientBase<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        readonly string host;
        readonly CallOptions option;
        readonly CallInvoker callInvoker;
        readonly ILogger logger;

        protected readonly IFormatterResolver resolver;

        DuplexStreamingResult<byte[], byte[]> connection;
        protected TReceiver receiver;
        Task subscription;
        TaskCompletionSource<object> waitForDisconnect = new TaskCompletionSource<object>();

        // {messageId, TaskCompletionSource}
        ConcurrentDictionary<int, object> responseFutures = new ConcurrentDictionary<int, object>();
        protected CancellationTokenSource cts = new CancellationTokenSource();
        int messageId = 0;
        bool disposed;

        protected StreamingHubClientBase(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
        {
            this.callInvoker = callInvoker;
            this.host = host;
            this.option = option;
            this.resolver = resolver;
            this.logger = logger ?? GrpcEnvironment.Logger;
        }

        protected abstract Method<byte[], byte[]> DuplexStreamingAsyncMethod { get; }

        // call immediately after create.
        public void __ConnectAndSubscribe(TReceiver receiver)
        {
            var callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, host, option);
            var streamingResult = new DuplexStreamingResult<byte[], byte[]>(callResult, resolver);

            this.connection = streamingResult;
            this.receiver = receiver;
            this.subscription = StartSubscribe();
        }

        protected abstract void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data);
        protected abstract Task OnBroadcastEvent(int methodId, ArraySegment<byte> data);

        async Task StartSubscribe()
        {
            var reader = connection.RawStreamingCall.ResponseStream;
            try
            {
                while (await reader.MoveNext(cts.Token))
                {
                    try
                    {
                        var data = reader.Current;
                        // MessageFormat:
                        // broadcast: [methodId, [argument]]
                        // response:  [messageId, methodId, response]

                        var readSize = 0;
                        var offset = 0;
                        var arrayLength = MessagePackBinary.ReadArrayHeader(data, offset, out readSize);
                        offset += readSize;

                        if (arrayLength == 3)
                        {
                            var messageId = MessagePackBinary.ReadInt32(data, offset, out readSize);
                            offset += readSize;

                            if (responseFutures.TryGetValue(messageId, out var future))
                            {
                                var methodId = MessagePackBinary.ReadInt32(data, offset, out readSize);
                                offset += readSize;

                                try
                                {
                                    OnResponseEvent(methodId, future, new ArraySegment<byte>(data, offset, data.Length - offset));
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
                        else
                        {
                            var methodId = MessagePackBinary.ReadInt32(data, offset, out readSize);
                            offset += readSize;

                            await OnBroadcastEvent(methodId, new ArraySegment<byte>(data, offset, data.Length - offset));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Error(ex, "Error on consume received message, but keep subscribe.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    return;
                }

                logger?.Error(ex, "Error on subscribing message.");
            }
            finally
            {
                try
                {
                    await DisposeAsyncCore(false);
                }
                finally
                {
                    waitForDisconnect.TrySetResult(null);
                }
            }
        }

        protected Task WriteMessageAsync<T>(int methodId, T message)
        {
            ThrowIfDisposed();

            // TODO:Buffer.Rent
            byte[] buffer = null;
            var offset = 0;
            offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 2);
            offset += MessagePackBinary.WriteInt32(ref buffer, offset, methodId);
            offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, message, resolver);

            return connection.RawStreamingCall.RequestStream.WriteAsync(MessagePackBinary.FastCloneWithResize(buffer, offset));
        }

        protected async Task<TResponse> WriteMessageWithResponseAsync<TRequest, TResponse>(int methodId, TRequest message)
        {
            ThrowIfDisposed();

            var mid = Interlocked.Increment(ref messageId);
            var tcs = new TaskCompletionSourceEx<TResponse>(); // use Ex
            responseFutures[mid] = (object)tcs;

            // TODO:Rent Pool
            byte[] buffer = null;
            var offset = 0;
            offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 3);
            offset += MessagePackBinary.WriteInt32(ref buffer, offset, mid);
            offset += MessagePackBinary.WriteInt32(ref buffer, offset, methodId);
            offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, message, resolver);

            await connection.RawStreamingCall.RequestStream.WriteAsync(MessagePackBinary.FastCloneWithResize(buffer, offset)).ConfigureAwait(false);

            return await tcs.Task; // TODO:if server throws error, return forever? requires error handling.
        }

        void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException("StreamingHubClient");
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

            await connection.RequestStream.CompleteAsync();
            // TODO:complete and cancellation, which should be first???
            cts.Cancel();
            cts.Dispose();
            try
            {
                if (waitSubscription)
                {
                    if (subscription != null)
                    {
                        await subscription;
                    }
                }

                // cleanup completion
                foreach (var item in responseFutures)
                {
                    (item.Value as ITaskCompletion).TrySetCanceled();
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
