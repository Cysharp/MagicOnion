using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Internal;

/// <remarks>
///     <para>StreamingHub message formats (from Server to Client):</para>
///     <list type="bullet">
///         <item>
///             <term>Response: InvokeHubMethod (from server to client)</term>
///             <description>Array(3): [MessageId(int), MethodId(int), SerializedResponse]</description>
///         </item>
///         <item>
///             <term>Response: InvokeHubMethod (from server to client; with Exception)</term>
///             <description>Array(4): [MessageId(int), StatusCode(int), Detail(string), Message(string)]</description>
///         </item>
///         <item>
///             <term>Broadcast: from server to client</term>
///             <description>Array(2): [MethodId(int), SerializedArgument]</description>
///         </item>
///         <item>
///             <term>ClientInvoke/Request: InvokeClientMethod (from server to client)</term>
///             <description>Array(5): [Type=0x00, Nil, ClientResultMessageId(Guid), MethodId(int), SerializedArguments]</description>
///         </item>
///         <item>
///             <term>ServerHeartbeat/Request:</term>
///             <description>Array(5): [Type=0x7f, Nil, Nil, Nil, Extras]</description>
///         </item>
///         <item>
///             <term>ClientHeartbeat/Response:</term>
///             <description>Array(5): [Type=0x7e, Nil, Nil, Nil, [ClientTime(long)]]</description>
///         </item>
///     </list>
///     <para>StreamingHub message formats (from Client to Server):</para>
///     <list type="bullet">
///         <item>
///             <term>Request: InvokeHubMethod (from client; void; fire-and-forget)</term>
///             <description>Array(2): [MethodId(int), SerializedArguments]</description>
///         </item>
///         <item>
///             <term>Request: InvokeHubMethod (from client; non-void)</term>
///             <description>Array(3): [MessageId(int), MethodId(int), SerializedArguments]</description>
///         </item>
///         <item>
///             <term>ClientInvoke/Response: InvokeClientMethod (from client to server)</term>
///             <description>Array(4): [Type=0x00, ClientResultMessageId(Guid), MethodId(int), SerializedResponse]</description>
///         </item>
///         <item>
///             <term>ClientInvoke/Response: InvokeClientMethod (from client to server; with Exception)</term>
///             <description>Array(4): [Type=0x01, ClientResultMessageId(Guid), MethodId(int), [StatusCode(int), Detail(string), Message(string)]]</description>
///         </item>
///         <item>
///             <term>ServerHeartbeat/Response:</term>
///             <description>Array(4): [Type=0x7f, Nil, Nil, Nil]</description>
///         </item>
///         <item>
///             <term>ClientHeartbeat/Request:</term>
///             <description>Array(4): [Type=0x7e, Nil, Nil, [ClientTime(long)]]</description>
///         </item>
///     </list>
/// </remarks>
internal static class StreamingHubMessageWriter
{
    /// <summary>
    /// Writes a broadcast message of Hub method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBroadcastMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, T value, IMagicOnionSerializer messageSerializer)
    {
        var bytesWritten = 0;
        var written = 0;
        Span<byte> header = bufferWriter.GetSpan(16);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(header.Slice(bytesWritten), 2, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), methodId, out written), written, ref bytesWritten);
        bufferWriter.Advance(bytesWritten);

        messageSerializer.Serialize(bufferWriter, value);
    }

    /// <summary>
    /// Writes a request message of Hub method.
    /// </summary>
    public static void WriteRequestMessageVoid<T>(IBufferWriter<byte> bufferWriter, int methodId, T value, IMagicOnionSerializer messageSerializer)
    {
        var bytesWritten = 0;
        var written = 0;
        Span<byte> header = bufferWriter.GetSpan(16);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(header.Slice(bytesWritten), 2, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), methodId, out written), written, ref bytesWritten);
        bufferWriter.Advance(bytesWritten);

        messageSerializer.Serialize(bufferWriter, value);
    }

    /// <summary>
    /// Writes a request message of Hub method.
    /// </summary>
    public static void WriteRequestMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, int messageId, T value, IMagicOnionSerializer messageSerializer)
    {
        var bytesWritten = 0;
        var written = 0;
        Span<byte> header = bufferWriter.GetSpan(16);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(header.Slice(bytesWritten), 3, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), messageId, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), methodId, out written), written, ref bytesWritten);
        bufferWriter.Advance(bytesWritten);

        messageSerializer.Serialize(bufferWriter, value);
    }

    /// <summary>
    /// Writes an empty response message of Hub method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteResponseMessage(IBufferWriter<byte> bufferWriter, int methodId, int messageId)
    {
        var bytesWritten = 0;
        var written = 0;
        Span<byte> header = bufferWriter.GetSpan(16);

        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(header.Slice(bytesWritten), 3, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), messageId, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), methodId, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteNil(header.Slice(bytesWritten), out written), written, ref bytesWritten);
        bufferWriter.Advance(bytesWritten);
    }

    /// <summary>
    /// Writes a response message of Hub method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteResponseMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, int messageId, T v, IMagicOnionSerializer messageSerializer)
    {
        var bytesWritten = 0;
        var written = 0;
        Span<byte> header = bufferWriter.GetSpan(16);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(header.Slice(bytesWritten), 3, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), messageId, out written), written, ref bytesWritten);
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(header.Slice(bytesWritten), methodId, out written), written, ref bytesWritten);
        bufferWriter.Advance(bytesWritten);

        messageSerializer.Serialize(bufferWriter, v);
    }

    /// <summary>
    /// Write an error response message of Hub method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteResponseMessageForError(IBufferWriter<byte> bufferWriter, int messageId, int statusCode, string detail, Exception? ex, bool isReturnExceptionStackTraceInErrorDetail)
    {
        var writer = new MessagePackWriter(bufferWriter);
        writer.WriteArrayHeader(4);
        writer.Write(messageId);
        writer.Write(statusCode);
        writer.Write(detail);

        var msg = (isReturnExceptionStackTraceInErrorDetail && ex != null)
            ? ex.ToString()
            : null;

        writer.Write(msg);
        writer.Flush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteClientResultRequestMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, Guid messageId, T request, IMagicOnionSerializer messageSerializer)
    {
        var writer = new MessagePackWriter(bufferWriter);
        writer.WriteArrayHeader(5);
        writer.Write(0); // Type = ClientResultRequest (0)
        writer.WriteNil(); // Dummy
        MessagePackSerializer.Serialize(ref writer, messageId);
        writer.Write(methodId);
        writer.Flush();
        messageSerializer.Serialize(bufferWriter, request);
    }

    /// <summary>
    /// Writes a response message for client result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteClientResultResponseMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, Guid messageId, T response, IMagicOnionSerializer messageSerializer)
    {
        var writer = new MessagePackWriter(bufferWriter);
        writer.WriteArrayHeader(4);
        writer.Write(0); // Result = 0 (success)
        MessagePackSerializer.Serialize(ref writer, messageId);
        writer.Write(methodId);
        writer.Flush();
        messageSerializer.Serialize(bufferWriter, response);
    }

    /// <summary>
    /// Writes an error response message for client result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteClientResultResponseMessageForError(IBufferWriter<byte> bufferWriter, int methodId, Guid messageId, int statusCode, string detail, Exception? ex, IMagicOnionSerializer messageSerializer)
    {
        var writer = new MessagePackWriter(bufferWriter);
        writer.WriteArrayHeader(4);
        writer.Write(1); // Result = 1 (failed)
        MessagePackSerializer.Serialize(ref writer, messageId);
        writer.Write(methodId);

        writer.WriteArrayHeader(3);
        {
            writer.Write(statusCode);
            writer.Write(detail);
            writer.Write(ex?.ToString());
        }
        writer.Flush();
    }

    /// <summary>
    /// Writes a server heartbeat message for sending from the server.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteServerHeartbeatMessageHeader(IBufferWriter<byte> bufferWriter, short sequence, DateTimeOffset serverSentAt)
    {
        var written = 0;
        var bytesWritten = 0;
        var buffer = bufferWriter.GetSpan(32);

        // Array(5)[127, Sequence(int8), ServerSentAt(long; UnixTimeMs), Nil, <Metadata>]
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(buffer.Slice(bytesWritten), 5, out written), written, ref bytesWritten);
        // Type = 0x7f / 127 (Heartbeat)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), 0x7f, out written), written, ref bytesWritten);
        // 1:Sequence
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), sequence, out written), written, ref bytesWritten);
        // 2:ServerSentAt
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), serverSentAt.ToUnixTimeMilliseconds(), out written), written, ref bytesWritten);
        // 3:Reserved (Nil)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteNil(buffer.Slice(bytesWritten), out written), written, ref bytesWritten);

        bufferWriter.Advance(bytesWritten);
    }

    /// <summary>
    /// Writes a server heartbeat message for sending response from the client.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteServerHeartbeatMessageResponse(Span<byte> buffer, short sequence, long serverSentAt, out int bytesWritten)
    {
        Debug.Assert(buffer.Length >= 5 + 5 + 5 + 5 + 9);
        var written = bytesWritten = 0;

        // Array(4)[127, Sequence(int8), ServerSentAt(long; UnixTimeMs), Nil]
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(buffer.Slice(bytesWritten), 4, out written), written, ref bytesWritten);
        // Type = 0x7f / 127 (Heartbeat)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), 0x7f, out written), written, ref bytesWritten);
        // 1:Sequence
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), sequence, out written), written, ref bytesWritten);
        // 2:ServerSentAt
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), serverSentAt, out written), written, ref bytesWritten);
        // 3:Reserved (Nil)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteNil(buffer.Slice(bytesWritten), out written), written, ref bytesWritten);
    }

    /// <summary>
    /// Writes a client heartbeat message for sending from the client.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteClientHeartbeatMessage(Span<byte> buffer, short sequence, long clientSentAtElapsedFromOriginMs, out int bytesWritten)
    {
        Debug.Assert(buffer.Length >= 5 + 5 + 5 + 5 + 9);
        var written = bytesWritten = 0;

        // Array(4)[0x7e(126), Sequence(int8), ClientSentAtElapsedFromOrigin(long; Ms), <Extra>]
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(buffer.Slice(bytesWritten), 4, out written), written, ref bytesWritten);
        // 0:Type = 0x7e / 126 (ClientHeartbeat)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), 0x7e, out written), written, ref bytesWritten);
        // 1:Sequence
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), sequence, out written), written, ref bytesWritten);
        // 2:ClientSentAtElapsedFromOrigin
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), clientSentAtElapsedFromOriginMs, out written), written, ref bytesWritten);
        // 3:Reserved (Nil)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteNil(buffer.Slice(bytesWritten), out written), written, ref bytesWritten);
    }

    /// <summary>
    /// Writes a client heartbeat message for sending response from the server.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteClientHeartbeatMessageResponse(IBufferWriter<byte> bufferWriter, short sequence, long clientSentAt)
    {
        var written = 0;
        var bytesWritten = 0;
        var buffer = bufferWriter.GetSpan(32);

        // Array(5)[0x7e(126), Sequence(int8), ClientSentAtElapsedFromOrigin(long; Ms), Nil, <Extra>]
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteArrayHeader(buffer.Slice(bytesWritten), 5, out written), written, ref bytesWritten);
        // 0:Type = 0x7e / 126 (Heartbeat)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), 0x7e, out written), written, ref bytesWritten);
        // 1:Sequence
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), sequence, out written), written, ref bytesWritten);
        // 2:ClientSentAtElapsedFromOrigin
        VerifyResultAndAdvance(MessagePackPrimitives.TryWrite(buffer.Slice(bytesWritten), clientSentAt, out written), written, ref bytesWritten);
        // 3:Reserved (Nil)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteNil(buffer.Slice(bytesWritten), out written), written, ref bytesWritten);
        // 4:Reserved (Nil)
        VerifyResultAndAdvance(MessagePackPrimitives.TryWriteNil(buffer.Slice(bytesWritten), out written), written, ref bytesWritten);

        bufferWriter.Advance(bytesWritten);
    }

    static void VerifyResultAndAdvance(bool result, int written, ref int bytesWrittenTotal)
    {
        if (!result)
        {
            throw new InvalidOperationException($"Insufficient buffer size.");
        }

        bytesWrittenTotal += written;
    }
}

internal enum StreamingHubMessageType
{
    /// <summary>
    /// Request: Client -> Server
    /// </summary>
    Request,
    /// <summary>
    /// Request: Client -> Server / Fire-and-Forget
    /// </summary>
    RequestFireAndForget,
    /// <summary>
    /// Request: Client -> Server -> Client
    /// </summary>
    Response,
    /// <summary>
    /// Request: Client -> Server -(Error)-> Client
    /// </summary>
    ResponseWithError,

    /// <summary>
    /// Broadcast: Server -> Client
    /// </summary>
    Broadcast,

    /// <summary>
    /// ClientResult: Server -> Client
    /// </summary>
    ClientResultRequest,
    /// <summary>
    /// ClientResult: Server -> Client -> Server
    /// </summary>
    ClientResultResponse,
    /// <summary>
    /// ClientResult: Server -> Client -(Error)-> Server
    /// </summary>
    ClientResultResponseWithError,


    /// <summary>
    /// Heartbeat: Server -> Client -> Server
    /// </summary>
    ServerHeartbeatResponse,
    /// <summary>
    /// Heartbeat: Server -> Client
    /// </summary>
    ServerHeartbeat,

    /// <summary>
    /// Heartbeat: Client -> Server
    /// </summary>
    ClientHeartbeat,
    /// <summary>
    /// Heartbeat: Client -> Server -> Client
    /// </summary>
    ClientHeartbeatResponse,
}
