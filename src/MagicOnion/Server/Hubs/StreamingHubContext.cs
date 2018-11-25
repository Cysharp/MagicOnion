using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class StreamingHubContext
    {
        /// <summary>Raw gRPC Context.</summary>
        public ServiceContext ServiceContext { get; set; }
        public object HubInstance { get; set; }

        public ArraySegment<byte> Request { get; set; }

        // helper for reflection
        internal IFormatterResolver FormatterResolver { get; set; }
        public Guid ConnectionId => ServiceContext.ContextId;

        public AsyncLock AsyncWriterLock { get; internal set; }
        internal int MessageId { get; set; }
        internal int MethodId { get; set; }

        // helper for reflection

        internal async ValueTask WriteResponseMessage<T>(Task<T> value)
        {
            // MessageFormat:
            // response:  [messageId, methodId, response]
            byte[] buffer = null; // TODO:byte buffer
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
}
