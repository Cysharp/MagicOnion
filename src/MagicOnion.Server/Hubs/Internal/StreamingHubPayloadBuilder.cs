using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MagicOnion.Serialization;

namespace MagicOnion.Server.Hubs.Internal;

internal static class StreamingHubPayloadBuilder
{
    public static StreamingHubPayload Build(int methodId, int messageId)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteResponseMessage(buffer, methodId, messageId);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    public static StreamingHubPayload Build<T>(int methodId, int messageId, T v, IMagicOnionSerializer messageSerializer)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteResponseMessage(buffer, methodId, messageId, v, messageSerializer);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    public static StreamingHubPayload BuildError(int messageId, int statusCode, string detail, Exception? ex, bool isReturnExceptionStackTraceInErrorDetail)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteResponseMessageForError(buffer, messageId, statusCode, detail, ex, isReturnExceptionStackTraceInErrorDetail);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }
}
