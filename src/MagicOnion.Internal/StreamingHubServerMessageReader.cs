using MessagePack;

namespace MagicOnion.Internal
{
    internal ref struct StreamingHubServerMessageReader
    {
        readonly ReadOnlyMemory<byte> data;
        int position;

        public StreamingHubServerMessageReader(ReadOnlyMemory<byte> data)
        {
            this.data = data;
            this.position = 0;
        }

        void VerifyResultAndAdvance(MessagePackPrimitives.DecodeResult result, int readLen)
        {
            if (result != MessagePackPrimitives.DecodeResult.Success)
            {
                throw new InvalidOperationException($"Invalid message format: {result}");
            }
            position += readLen;
        }

        public StreamingHubMessageType ReadMessageType()
        {
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadArrayHeader(data.Span.Slice(position), out var arrayLength, out var read), read);

            return arrayLength switch
            {
                2 => StreamingHubMessageType.RequestFireAndForget,
                3 => StreamingHubMessageType.Request,
                4 => ReadMessageSubType(),
                _ => throw new InvalidOperationException($"Unknown message format: ArrayLength = {arrayLength}"),
            };
        }
        StreamingHubMessageType ReadMessageSubType()
        {
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadByte(data.Span.Slice(position), out var subType, out var read), read);

            return subType switch
            {
                0x00 => StreamingHubMessageType.ClientResultResponse,
                0x01 => StreamingHubMessageType.ClientResultResponseWithError,
                0x7e => StreamingHubMessageType.ClientHeartbeat,
                0x7f => StreamingHubMessageType.ServerHeartbeatResponse,
                _ => throw new InvalidOperationException($"Unknown client response message: {subType}"),
            };
        }

        public (int MethodId, ReadOnlyMemory<byte> Body) ReadRequestFireAndForget()
        {
            // void: [methodId, [argument]]
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var methodId, out var read), read);

            return (methodId, data.Slice(position));
        }

        public (int MessageId, int MethodId, ReadOnlyMemory<byte> Body) ReadRequest()
        {
            // T: [messageId, methodId, [argument]]
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var messageId, out var readLenMessageId), readLenMessageId);
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var methodId, out var readLenMethodId), readLenMethodId);

            return (messageId, methodId, data.Slice(position));
        }

        public (Guid ClientResultMessageId, int ClientMethodId, ReadOnlyMemory<byte> Body) ReadClientResultResponse()
        {
            // T: [0, clientResultMessageId, methodId, result]
            var reader = new MessagePackReader(data.Slice(position));
            var clientResultMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var clientMethodId = reader.ReadInt32();
            position += (int)reader.Consumed;

            return (clientResultMessageId, clientMethodId, data.Slice(position));
        }

        public (Guid ClientResultMessageId, int ClientMethodId, int StatusCode, string Detail, string Message) ReadClientResultResponseForError()
        {
            // T: [1, clientResultMessageId, methodId, [statusCode, detail, message]]
            var reader = new MessagePackReader(data.Slice(position));
            var clientResultMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var clientMethodId = reader.ReadInt32();
            var bodyArray = reader.ReadArrayHeader();
            if (bodyArray != 3) throw new InvalidOperationException($"Invalid ClientResponse: The BodyArray length is {bodyArray}");

            var statusCode = reader.ReadInt32();
            var detail = reader.ReadString() ?? string.Empty;
            var message = reader.ReadString() ?? string.Empty;

            position += (int)reader.Consumed;

            return (clientResultMessageId, clientMethodId, statusCode, detail, message);
        }

        public (short Sequence, long ClientSentAt, ReadOnlyMemory<byte> Extra) ReadClientHeartbeat()
        {
            // [Sequence(int16), ClientSentAt(long), <Extra>]
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt16(data.Span.Slice(position), out var sequence, out var readLenSequence), readLenSequence); // Sequence
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt64(data.Span.Slice(position), out var clientSentAt, out var readLenClientSentAt), readLenClientSentAt); // ClientSentAt

            return (sequence, clientSentAt, data.Slice(position));
        }

        public short ReadServerHeartbeatResponse()
        {
            // [Sequence(int16), Nil, Nil]
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt16(data.Span.Slice(position), out var sequence, out var readLenSequence), readLenSequence); // Sequence

            return sequence;
        }
    }
}
