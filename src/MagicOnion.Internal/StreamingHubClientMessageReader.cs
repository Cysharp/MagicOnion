using MessagePack;

namespace MagicOnion.Internal
{
    internal ref struct StreamingHubClientMessageReader
    {
        readonly ReadOnlyMemory<byte> data;
        int position;

        public StreamingHubClientMessageReader(ReadOnlyMemory<byte> data)
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
                2 => StreamingHubMessageType.Broadcast,
                3 => StreamingHubMessageType.Response,
                4 => StreamingHubMessageType.ResponseWithError,
                5 => ReadMessageSubType(),
                _ => throw new InvalidOperationException($"Unknown message format: ArrayLength = {arrayLength}"),
            };
        }

        StreamingHubMessageType ReadMessageSubType()
        {
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadByte(data.Span.Slice(position), out var subType, out var read), read);
            return subType switch
            {
                0x00 /*   0:ClientResultRequest */ => StreamingHubMessageType.ClientResultRequest,
                0x7e /* 126:ClientHeartbeatResponse */ => StreamingHubMessageType.ClientHeartbeatResponse,
                0x7f /* 127:ServerHeartbeat */ => StreamingHubMessageType.ServerHeartbeat,
                _ => throw new InvalidOperationException($"Unknown Type: {subType}"),
            };
        }

        public (int MethodId, int Cosumed) ReadBroadcastMessageMethodId()
        {
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var methodId, out var readMethodId), readMethodId);
            return (methodId, position);
        }

        public (int MethodId, ReadOnlyMemory<byte> Body) ReadBroadcastMessage()
        {
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var methodId, out var readMethodId), readMethodId);
            return (methodId, data.Slice(position));
        }

        public (int MessageId, int MethodId, ReadOnlyMemory<byte> Body) ReadResponseMessage()
        {
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var messageId, out var readMessageId), readMessageId);
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt32(data.Span.Slice(position), out var methodId, out var readMethodId), readMethodId);
            return (messageId, methodId, data.Slice(position));
        }

        public (int MessageId, int StatusCode, string? Detail, string? Error) ReadResponseWithErrorMessage()
        {
            var reader = new MessagePackReader(data.Slice(position));
            var messageId = reader.ReadInt32();
            var statusCode = reader.ReadInt32();
            var detail = reader.ReadString();
            var error = reader.ReadString();

            return (messageId, statusCode, detail, error);
        }

        public (Guid ClientResultRequestMessageId, int MethodId, ReadOnlyMemory<byte> Body) ReadClientResultRequestMessage()
        {
            var reader = new MessagePackReader(data.Slice(position));
            //var type = reader.ReadByte(); // Type is already read by ReadMessageType
            reader.Skip(); // Dummy
            var clientRequestMessageId = MessagePackSerializer.Deserialize<Guid>(ref reader);
            var methodId = reader.ReadInt32();
            position += (int)reader.Consumed;

            return (clientRequestMessageId, methodId, data.Slice(position));
        }

        public (short Sequence, long ServerSentAt, ReadOnlyMemory<byte> Metadata) ReadServerHeartbeat()
        {
            // Type is already read by ReadMessageType (Byte)

            // Sequence (1)
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt16(data.Span.Slice(position), out var sequence, out var readLenSequence), readLenSequence);
            // ServerSentAt (2)
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt64(data.Span.Slice(position), out var serverSentAt, out var readLenServerSentAt), readLenServerSentAt);
            // Reserved (3)
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadNil(data.Span.Slice(position), out var readLenReserved), readLenReserved);

            return (sequence, serverSentAt, data.Slice(position));
        }

        public (short Sequence, long ClientSentAt) ReadClientHeartbeatResponse()
        {
            // Type is already read by ReadMessageType (Byte)

            // Sequence (1)
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt16(data.Span.Slice(position), out var sequence, out var readLenSequence), readLenSequence);
            // ClientSentAt (2)
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadInt64(data.Span.Slice(position), out var clientSentAt, out var readLenClientSentAt), readLenClientSentAt);
            // Reserved (3)
            VerifyResultAndAdvance(MessagePackPrimitives.TryReadNil(data.Span.Slice(position), out var readLenReserved), readLenReserved);

            return (sequence, clientSentAt);
        }
    }
}
