using Grpc.Core;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public abstract class StreamingHubBase<THubInterface, TReceiver> : ServiceBase<THubInterface>, IStreamingHub<THubInterface, TReceiver>
        where THubInterface : IStreamingHub<THubInterface, TReceiver>
    {
        static protected readonly Task<Nil> NilTask = Task.FromResult(Nil.Default);
        static protected readonly ValueTask CompletedTask = new ValueTask();

        public HubGroupRepository Group { get; private set; }

        // Broadcast Commands

        [Ignore]
        protected TReceiver Broadcast(IGroup group)
        {
            return group.Broadcast<TReceiver>();
        }

        [Ignore]
        protected TReceiver BroadcastExceptSelf(IGroup group)
        {
            return group.BroadcastExcept<TReceiver>(Context.ContextId);
        }

        [Ignore]
        protected TReceiver BroadcastExcept(IGroup group, Guid except)
        {
            return group.BroadcastExcept<TReceiver>(except);
        }

        [Ignore]
        protected TReceiver BroadcastExcept(IGroup group, Guid[] excepts)
        {
            return group.BroadcastExcept<TReceiver>(excepts);
        }

        /// <summary>
        /// Called before connect, instead of constructor.
        /// </summary>
        protected virtual ValueTask OnConnecting()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Called after disconnect.
        /// </summary>
        protected virtual ValueTask OnDisconnected()
        {
            return CompletedTask;
        }

        public async Task<DuplexStreamingResult<byte[], byte[]>> Connect()
        {
            var streamingContext = GetDuplexStreamingContext<byte[], byte[]>();
            Context.AsyncWriterLock = new AsyncLock();

            var group = StreamingHubHandlerRepository.GetGroupRepository(Context.MethodHandler);
            this.Group = new HubGroupRepository(this.Context, group);
            try
            {
                await OnConnecting();
                await HandleMessageAsync();
            }
            finally
            {
                await OnDisconnected();
                await this.Group.DisposeAsync();
            }

            return streamingContext.Result();
        }

        async Task HandleMessageAsync()
        {
            var ct = Context.CallContext.CancellationToken;
            var reader = Context.RequestStream;
            var writer = Context.ResponseStream;

            var handlers = StreamingHubHandlerRepository.GetHandlers(Context.MethodHandler);

            // Main loop of StreamingHub.
            // Be careful to allocation and performance.
            while (await reader.MoveNext(ct))
            {
                var data = reader.Current;

                var length = MessagePackBinary.ReadArrayHeader(data, 0, out var readSize);
                var offset = readSize;

                if (length == 2)
                {
                    // void: [methodId, [argument]]
                    var methodId = MessagePackBinary.ReadInt32(data, offset, out readSize);
                    offset += readSize;

                    if (handlers.TryGetValue(methodId, out var handler))
                    {
                        var context = new StreamingHubContext() // create per invoke.
                        {
                            AsyncWriterLock = Context.AsyncWriterLock,
                            FormatterResolver = handler.resolver,
                            HubInstance = this,
                            ServiceContext = Context,
                            Request = new ArraySegment<byte>(data, offset, data.Length - offset),
                            Path = handler.ToString(),
                            MethodId = handler.MethodId,
                            MessageId = -1,
                            Timestamp = DateTime.UtcNow
                        };

                        var isErrorOrInterrupted = false;
                        Context.MethodHandler.logger.BeginInvokeHubMethod(context, context.Request, handler.RequestType);
                        try
                        {
                            await handler.MethodBody.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            isErrorOrInterrupted = true;
                            LogError(ex, context);
                        }
                        finally
                        {
                            Context.MethodHandler.logger.EndInvokeHubMethod(context, context.responseSize, context.responseType, (DateTime.UtcNow - context.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
                    }
                }
                else if (length == 3)
                {
                    // T: [messageId, methodId, [argument]]
                    var messageId = MessagePackBinary.ReadInt32(data, offset, out readSize);
                    offset += readSize;

                    var methodId = MessagePackBinary.ReadInt32(data, offset, out readSize);
                    offset += readSize;

                    if (handlers.TryGetValue(methodId, out var handler))
                    {
                        var context = new StreamingHubContext() // create per invoke.
                        {
                            AsyncWriterLock = Context.AsyncWriterLock,
                            FormatterResolver = handler.resolver,
                            HubInstance = this,
                            ServiceContext = Context,
                            Request = new ArraySegment<byte>(data, offset, data.Length - offset),
                            Path = handler.ToString(),
                            MethodId = handler.MethodId,
                            MessageId = messageId,
                            Timestamp = DateTime.UtcNow
                        };

                        var isErrorOrInterrupted = false;
                        Context.MethodHandler.logger.BeginInvokeHubMethod(context, context.Request, handler.RequestType);
                        try
                        {
                            await handler.MethodBody.Invoke(context);
                        }
                        catch (ReturnStatusException ex)
                        {
                            await context.WriteErrorMessage((int)ex.StatusCode, ex.Detail, null, false);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, context);
                            await context.WriteErrorMessage((int)StatusCode.Internal, "Erorr on " + handler.ToString(), ex, Context.MethodHandler.isReturnExceptionStackTraceInErrorDetail);
                        }
                        finally
                        {
                            Context.MethodHandler.logger.EndInvokeHubMethod(context, context.responseSize, context.responseType, (DateTime.UtcNow - context.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid data format.");
                }
            }
        }

        void LogError(Exception ex, StreamingHubContext context)
        {
            Logger.Error(ex, "StreamingHubHandler throws exception occured in " + context.Path);
        }

        // Interface methods for Client

        THubInterface IStreamingHub<THubInterface, TReceiver>.FireAndForget()
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        Task IStreamingHub<THubInterface, TReceiver>.DisposeAsync()
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        Task IStreamingHub<THubInterface, TReceiver>.WaitForDisconnect()
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }
    }
}
