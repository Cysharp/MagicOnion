using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server
{
    // TODO:create per request and set lock.
    public class StreamingHubContext
    {
        /// <summary>Raw gRPC Context.</summary>
        public ServiceContext ServiceContext { get; set; }
        public object HubInstance { get; set; }

        public ArraySegment<byte> Request { get; set; }

        // helper for reflection
        internal IFormatterResolver FormatterResolver => ServiceContext.FormatterResolver;
        public Guid ConnectionId => ServiceContext.ContextId;

        public AsyncLock AsyncWriterLock { get; internal set; }
        internal int MessageId { get; set; }
        internal int MethodId { get; set; }

        // helper for reflection

        internal async ValueTask WriteResponseMessage<T>(Task<T> value)
        {
            // MessageFormat:
            // response:  [messageId, methodId, response]
            byte[] buffer = null;
            var offset = 0;
            offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 3);
            offset += MessagePackBinary.WriteInt32(ref buffer, offset, MessageId);
            offset += MessagePackBinary.WriteInt32(ref buffer, offset, MethodId);

            var v2 = await value.ConfigureAwait(false);
            offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, v2, FormatterResolver);
            var result = MessagePackBinary.FastCloneWithResize(buffer, offset);
            using (await AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await ServiceContext.ResponseStream.WriteAsync(result).ConfigureAwait(false);
            }
        }

        internal ValueTask WrapToValueTask(Task task)
        {
            return new ValueTask(task);
        }
    }




    // instantiate per call.
    public abstract class StreamingHubBase<THubInterface, TReceiver> : ServiceBase<THubInterface>, IStreamingHub<THubInterface, TReceiver>
        where THubInterface : IStreamingHubMarker
    {
        static protected readonly ValueTask CompletedTask = new ValueTask();

        public IGroupRepository Group { get; private set; } // TODO:set this.

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
                            HubInstance = this,
                            ServiceContext = Context,
                            Request = new ArraySegment<byte>(data, offset, data.Length - offset),
                            MethodId = handler.MethodId,
                        };
                        await handler.MethodBody.Invoke(context);
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
                            HubInstance = this,
                            ServiceContext = Context,
                            Request = new ArraySegment<byte>(data, offset, data.Length - offset),
                            MethodId = handler.MethodId,
                            MessageId = messageId
                        };
                        await handler.MethodBody.Invoke(context);
                    }
                    else
                    {
                        throw new InvalidOperationException("foo bar baz"); // TODO:error message details.
                    }



                    // await writer.WriteAsync(
                }
                else
                {
                    throw new InvalidOperationException("Invalid data format.");
                }
            }
        }

        // Interface methods for Client

        Task IStreamingHub<THubInterface, TReceiver>.DisposeAsync()
        {
            throw new NotSupportedException();
        }

        Task IStreamingHub<THubInterface, TReceiver>.WaitForDisconnect()
        {
            throw new NotSupportedException();
        }
    }
}
