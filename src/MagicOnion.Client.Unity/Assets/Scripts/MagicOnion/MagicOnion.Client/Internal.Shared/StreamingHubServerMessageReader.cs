using System;
using MessagePack;

namespace MagicOnion.Internal
{
    internal ref struct StreamingHubServerMessageReader
    {
        readonly ReadOnlyMemory<byte> data;
        MessagePackReader reader;

        public StreamingHubServerMessageReader(ReadOnlyMemory<byte> data)
        {
            this.data = data;
            this.reader =  new MessagePackReader(data);
        }

        public StreamingHubMessageType ReadMessageType()
        {
            var arrayLength = this.reader.ReadArrayHeader();
            return arrayLength switch
            {
                2 => StreamingHubMessageType.RequestFireAndForget,
                3 => StreamingHubMessageType.Request,
                _ => throw new InvalidOperationException($"Unknown message format: ArrayLength = {arrayLength}"),
            };
        }

        public (int MethodId, ReadOnlyMemory<byte> Body) ReadRequestFireAndForget()
        {
            // void: [methodId, [argument]]
            var methodId = reader.ReadInt32();
            var consumed = (int)reader.Consumed;

            return (methodId, data.Slice(consumed));
        }

        public (int MessageId, int MethodId, ReadOnlyMemory<byte> Body) ReadRequest()
        {
            // T: [messageId, methodId, [argument]]
            var messageId = reader.ReadInt32();
            var methodId = reader.ReadInt32();
            var consumed = (int)reader.Consumed;

            return (messageId, methodId, data.Slice(consumed));
        }
    }
}
