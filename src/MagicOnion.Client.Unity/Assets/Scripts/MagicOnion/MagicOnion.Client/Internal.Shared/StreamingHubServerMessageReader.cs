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
                4 => reader.ReadByte() switch
                {
                    0x00 => StreamingHubMessageType.ClientResultResponse,
                    0x01 => StreamingHubMessageType.ClientResultResponseWithError,
                    0x7e => StreamingHubMessageType.ClientHeartbeat,
                    0x7f => StreamingHubMessageType.ServerHeartbeatResponse,
                    var subType => throw new InvalidOperationException($"Unknown client response message: {subType}"),
                },
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

        public (Guid ClientResultMessageId, int ClientMethodId, ReadOnlyMemory<byte> Body) ReadClientResultResponse()
        {
            // T: [0, clientResultMessageId, methodId, result]
            var clientResultMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var clientMethodId = reader.ReadInt32();
            var consumed = (int)reader.Consumed;

            return (clientResultMessageId, clientMethodId, data.Slice(consumed));
        }

        public (Guid ClientResultMessageId, int ClientMethodId, int StatusCode, string Detail, string Message) ReadClientResultResponseForError()
        {
            // T: [1, clientResultMessageId, methodId, [statusCode, detail, message]]
            var clientResultMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var clientMethodId = reader.ReadInt32();
            var bodyArray = reader.ReadArrayHeader();
            if (bodyArray != 3) throw new InvalidOperationException($"Invalid ClientResponse: The BodyArray length is {bodyArray}");

            var statusCode = reader.ReadInt32();
            var detail = reader.ReadString() ?? string.Empty;
            var message = reader.ReadString() ?? string.Empty;

            return (clientResultMessageId, clientMethodId, statusCode, detail, message);
        }

        public (short Sequence, long ClientSentAt, ReadOnlyMemory<byte> Extra) ReadClientHeartbeat()
        {
            // [Sequence(int8), ClientSentAt(long), <Extra>]
            var sequence = reader.ReadInt16(); // Sequence
            var clientSentAt = reader.ReadInt64(); // ClientSentAt
            var extra = data.Slice((int)reader.Consumed);

            return (sequence, clientSentAt, data.Slice((int)reader.Consumed));
        }

        public short ReadServerHeartbeatResponse()
        {
            // [Sequence(int8), Nil, Nil]
            var sequence = reader.ReadInt16(); // Sequence
            reader.Skip(); // Dummy
            reader.Skip(); // Dummy

            return sequence;
        }
    }
}
