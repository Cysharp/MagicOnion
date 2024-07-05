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
                5 => reader.ReadByte() switch
                {
                    0x00 /*   0:ClientResultRequest */ => StreamingHubMessageType.ClientResultRequest,
                    0x7e /* 126:ClientHeartbeatResponse */ => StreamingHubMessageType.ClientHeartbeatResponse,
                    0x7f /* 127:ServerHeartbeat */ => StreamingHubMessageType.ServerHeartbeat,
                    var x => throw new InvalidOperationException($"Unknown Type: {x}"),
                },
                _ => throw new InvalidOperationException($"Unknown message format: ArrayLength = {arrayLength}"),
            };
        }

        public (int MethodId, int Cosumed) ReadBroadcastMessageMethodId()
        {
            return (reader.ReadInt32(), (int)reader.Consumed);
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
            //var type = reader.ReadByte(); // Type is already read by ReadMessageType
            reader.Skip(); // Dummy
            var clientRequestMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var methodId = reader.ReadInt32();
            var offset = (int)reader.Consumed;

            return (clientRequestMessageId, methodId, data.Slice(offset));
        }

        public (byte Sequence, long ServerSentAt, ReadOnlyMemory<byte> Metadata) ReadServerHeartbeat()
        {
            //var type = reader.ReadByte(); // Type is already read by ReadMessageType
            var sequence = reader.ReadByte(); // Sequence
            var serverSentAt = reader.ReadInt64(); // ServerSentAt (2)
            reader.Skip(); // Dummy (3)

            return (sequence, serverSentAt, data.Slice((int)reader.Consumed));
        }

        public (byte Sequence, long ClientSentAt) ReadClientHeartbeatResponse()
        {
            //var type = reader.ReadByte(); // Type is already read by ReadMessageType
            var sequence = reader.ReadByte(); // Sequence
            var clientSentAt = reader.ReadInt64(); // ClientSentAt (2)
            reader.Skip(); // Reserved (3)

            return (sequence, clientSentAt);
        }
    }
}
