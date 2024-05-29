using System;
using MessagePack;

namespace MagicOnion.Internal
{
    internal ref struct StreamingHubClientMessageReader
    {
        readonly ReadOnlyMemory<byte> data;
        MessagePackReader reader;

        public StreamingHubClientMessageReader(ReadOnlyMemory<byte> data)
        {
            this.data = data;
            this.reader = new MessagePackReader(data);
        }

        public StreamingHubMessageType ReadMessageType()
        {
            var arrayLength = this.reader.ReadArrayHeader();
            return arrayLength switch
            {
                2 => StreamingHubMessageType.Broadcast,
                3 => StreamingHubMessageType.Response,
                4 => StreamingHubMessageType.ResponseWithError,
                5 => StreamingHubMessageType.ClientResultRequest,
                _ => throw new InvalidOperationException($"Unknown message format: ArrayLength = {arrayLength}"),
            };
        }

        public (int MethodId, ReadOnlyMemory<byte> Body) ReadBroadcastMessage()
        {
            var methodId = reader.ReadInt32();
            var offset = (int)reader.Consumed;
            return (methodId, data.Slice(offset));
        }

        public (int MessageId, int MethodId, ReadOnlyMemory<byte> Body) ReadResponseMessage()
        {
            var messageId = reader.ReadInt32();
            var methodId = reader.ReadInt32();
            var offset = (int)reader.Consumed;
            return (messageId, methodId, data.Slice(offset));
        }

        public (int MessageId, int StatusCode, string? Detail, string? Error) ReadResponseWithErrorMessage()
        {
            var messageId = reader.ReadInt32();
            var statusCode = reader.ReadInt32();
            var detail = reader.ReadString();
            var error = reader.ReadString();

            return (messageId, statusCode, detail, error);
        }

        public (Guid ClientResultRequestMessageId, int MethodId, ReadOnlyMemory<byte> Body) ReadClientResultRequestMessage()
        {
            var type = reader.ReadByte(); // reserved
            _ = reader.ReadByte(); // dummy
            var clientRequestMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var methodId = reader.ReadInt32();
            var offset = (int)reader.Consumed;

            return (clientRequestMessageId, methodId, data.Slice(offset));
        }
    }
}
