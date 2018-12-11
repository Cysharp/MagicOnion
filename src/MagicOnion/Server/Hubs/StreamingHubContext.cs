using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class StreamingHubContext
    {
        /// <summary>Raw gRPC Context.</summary>
        public ServiceContext ServiceContext { get; internal set; }
        public object HubInstance { get; internal set; }

        public ArraySegment<byte> Request { get; internal set; }
        public string Path { get; internal set; }
        public DateTime Timestamp { get; internal set; }

        // helper for reflection
        internal IFormatterResolver FormatterResolver { get; set; }
        public Guid ConnectionId => ServiceContext.ContextId;

        public AsyncLock AsyncWriterLock { get; internal set; }
        internal int MessageId { get; set; }
        internal int MethodId { get; set; }

        internal int responseSize = -1;
        internal Type responseType;

        // helper for reflection
        internal async ValueTask WriteResponseMessageNil(Task value)
        {
            if (MessageId == -1) // don't write.
            {
                return;
            }

            // MessageFormat:
            // response:  [messageId, methodId, response]
            var rent = System.Buffers.ArrayPool<byte>.Shared.Rent(ushort.MaxValue);
            var buffer = rent;
            byte[] result;
            try
            {
                var offset = 0;
                offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 3);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, MessageId);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, MethodId);

                await value.ConfigureAwait(false);
                offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, Nil.Default, FormatterResolver);

                result = MessagePackBinary.FastCloneWithResize(buffer, offset);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(rent);
            }
            using (await AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await ServiceContext.ResponseStream.WriteAsync(result).ConfigureAwait(false);
            }
            responseSize = result.Length;
            responseType = typeof(Nil);
        }

        internal async ValueTask WriteResponseMessage<T>(Task<T> value)
        {
            if (MessageId == -1) // don't write.
            {
                return;
            }

            // MessageFormat:
            // response:  [messageId, methodId, response]
            var rent = System.Buffers.ArrayPool<byte>.Shared.Rent(ushort.MaxValue);
            var buffer = rent;
            byte[] result;
            try
            {
                var offset = 0;
                offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 3);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, MessageId);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, MethodId);

                var v2 = await value.ConfigureAwait(false);
                offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, v2, FormatterResolver);
                result = MessagePackBinary.FastCloneWithResize(buffer, offset);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(rent);
            }
            using (await AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await ServiceContext.ResponseStream.WriteAsync(result).ConfigureAwait(false);
            }
            responseSize = result.Length;
            responseType = typeof(T);
        }

        internal async ValueTask WriteErrorMessage(Exception ex, bool isReturnExceptionStackTraceInErrorDetail)
        {
            // MessageFormat:
            // error-response:  [messageId, Nil, StringMessage]
            var rent = System.Buffers.ArrayPool<byte>.Shared.Rent(ushort.MaxValue);
            var buffer = rent;
            byte[] result;
            try
            {
                var offset = 0;
                offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 3);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, MessageId);
                offset += MessagePackBinary.WriteNil(ref buffer, offset);

                var msg = (isReturnExceptionStackTraceInErrorDetail)
                    ? ex.ToString()
                    : "Internal Server Error";

                offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, msg, FormatterResolver);
                result = MessagePackBinary.FastCloneWithResize(buffer, offset);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(rent);
            }
            using (await AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await ServiceContext.ResponseStream.WriteAsync(result).ConfigureAwait(false);
            }
            responseSize = result.Length;
        }
    }
}
