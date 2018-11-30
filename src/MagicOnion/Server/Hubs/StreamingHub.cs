using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public abstract class StreamingHubBase<THubInterface, TReceiver> : ServiceBase<THubInterface>, IStreamingHub<THubInterface, TReceiver>
        where THubInterface : IStreamingHubMarker
    {
        static protected readonly Task<Nil> NilTask = Task.FromResult(Nil.Default);
        static protected readonly ValueTask CompletedTask = new ValueTask();

        public IGroupRepository Group { get; private set; }

        // Broadcast Commands

        [Ignore]
        protected TReceiver Broadcast(IGroup group)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType;
            return (TReceiver)Activator.CreateInstance(type, group);
        }

        [Ignore]
        protected TReceiver BroadcastExceptSelf(IGroup group)
        {
            return BroadcastExcept(group, Context.ContextId);
        }

        [Ignore]
        protected TReceiver BroadcastExcept(IGroup group, Guid except)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptOne;
            return (TReceiver)Activator.CreateInstance(type, new object[] { group, except });
        }

        [Ignore]
        protected TReceiver BroadcastExcept(IGroup group, Guid[] excepts)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptMany;
            return (TReceiver)Activator.CreateInstance(type, new object[] { group, excepts });
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

            Group = StreamingHubHandlerRepository.GetGroupRepository(Context.MethodHandler);
            try
            {
                await OnConnecting();
                await HandleMessageAsync();
            }
            finally
            {
                await OnDisconnected();
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
                            MethodId = handler.MethodId,
                            MessageId = -1
                        };

                        try
                        {
                            await handler.MethodBody.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            // TODO:nanika error log.
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("foo bar baz"); // TODO:error message details.
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
                            MethodId = handler.MethodId,
                            MessageId = messageId
                        };

                        try
                        {
                            await handler.MethodBody.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            // error-response:  [messageId, Nil, StringMessage]
                            await context.WriteErrorMessage(ex, Context.MethodHandler.isReturnExceptionStackTraceInErrorDetail);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("foo bar baz"); // TODO:error message details.
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid data format.");
                }
            }
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
