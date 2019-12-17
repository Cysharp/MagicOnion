using MagicOnion.Utils;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class StreamingHubContext
    {
        ConcurrentDictionary<string, object> items;

        /// <summary>Object storage per invoke.</summary>
        public ConcurrentDictionary<string, object> Items
        {
            get
            {
                lock (this) // lock per self! is this dangerous?
                {
                    if (items == null) items = new ConcurrentDictionary<string, object>();
                }
                return items;
            }
        }

        /// <summary>Raw gRPC Context.</summary>
        public ServiceContext ServiceContext { get; internal set; }
        public object HubInstance { get; internal set; }

        public ReadOnlyMemory<byte> Request { get; internal set; }
        public string Path { get; internal set; }
        public DateTime Timestamp { get; internal set; }

        // helper for reflection
        internal MessagePackSerializerOptions SerializerOptions { get; set; }
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
            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);

                    writer.WriteArrayHeader(3);
                    writer.Write(MessageId);
                    writer.Write(MethodId);
                    writer.WriteNil();
                    return buffer.WrittenSpan.ToArray();
                }
            }

            // await value.ConfigureAwait(false);

            var result = BuildMessage();
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
            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);
                    writer.WriteArrayHeader(3);
                    writer.Write(MessageId);
                    writer.Write(MethodId);
                    writer.Flush();
                    return buffer.WrittenSpan.ToArray();
                }
            }

            byte[] result = BuildMessage();
            using (await AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await ServiceContext.ResponseStream.WriteAsync(result).ConfigureAwait(false);
            }
            responseSize = result.Length;
            responseType = typeof(T);
        }

        internal async ValueTask WriteErrorMessage(int statusCode, string detail, Exception ex, bool isReturnExceptionStackTraceInErrorDetail)
        {
            // MessageFormat:
            // error-response:  [messageId, statusCode, detail, StringMessage]
            byte[] BuildMessage()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);
                    writer.WriteArrayHeader(4);
                    writer.Write(MessageId);
                    writer.Write(statusCode);
                    writer.Write(detail);

                    var msg = (isReturnExceptionStackTraceInErrorDetail)
                        ? ex.ToString()
                        : null;

                    if (msg != null)
                    {
                        MessagePackSerializer.Serialize(ref writer, msg, SerializerOptions);
                    }
                    else
                    {
                        writer.WriteNil();
                    }
                    writer.Flush();
                    return buffer.WrittenSpan.ToArray();
                }
            }

            var result = BuildMessage();
            using (await AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await ServiceContext.ResponseStream.WriteAsync(result).ConfigureAwait(false);
            }
            responseSize = result.Length;
        }
    }
}
