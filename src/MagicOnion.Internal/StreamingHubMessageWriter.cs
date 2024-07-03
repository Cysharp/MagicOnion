using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Internal
{
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
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(2);
            writer.Write(methodId);
            writer.Flush();
            messageSerializer.Serialize(bufferWriter, value);
        }

        /// <summary>
        /// Writes a request message of Hub method.
        /// </summary>
        public static void WriteRequestMessageVoid<T>(IBufferWriter<byte> bufferWriter, int methodId, T value, IMagicOnionSerializer messageSerializer)
        {
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(2);
            writer.Write(methodId);
            writer.Flush();
            messageSerializer.Serialize(bufferWriter, value);
        }

        /// <summary>
        /// Writes a request message of Hub method.
        /// </summary>
        public static void WriteRequestMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, int messageId, T value, IMagicOnionSerializer messageSerializer)
        {
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(3);
            writer.Write(messageId);
            writer.Write(methodId);
            writer.Flush();
            messageSerializer.Serialize(bufferWriter, value);
        }

        /// <summary>
        /// Writes an empty response message of Hub method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteResponseMessage(IBufferWriter<byte> bufferWriter, int methodId, int messageId)
        {
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(3);
            writer.Write(messageId);
            writer.Write(methodId);
            writer.WriteNil();
            writer.Flush();
        }

        /// <summary>
        /// Writes a response message of Hub method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteResponseMessage<T>(IBufferWriter<byte> bufferWriter, int methodId, int messageId, T v, IMagicOnionSerializer messageSerializer)
        {
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(3);
            writer.Write(messageId);
            writer.Write(methodId);
            writer.Flush();
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


        // Array(5)[127, Nil, Nil, Nil, <Extra>]
        static ReadOnlySpan<byte> ServerHeartbeatMessageForServerToClientHeader => new byte[] { 0x95, 0x7f, 0xc0, 0xc0, 0xc0 };

        /// <summary>
        /// Writes a server heartbeat message for sending from the server.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteServerHeartbeatMessageHeader(IBufferWriter<byte> bufferWriter)
        {
            bufferWriter.Write(ServerHeartbeatMessageForServerToClientHeader);
            //var writer = new MessagePackWriter(bufferWriter);
            //writer.WriteArrayHeader(5);
            //writer.Write(0x7f); // Type = 0x7f / 127 (Heartbeat)
            //writer.WriteNil();  // Dummy
            //writer.WriteNil();  // Dummy
            //writer.WriteNil();  // Dummy
            //writer.Flush();
            //                    // <Metadata>
        }

        // Array(4)[127, Nil, Nil, Nil]
        static ReadOnlySpan<byte> ServerHeartbeatMessageForClientToServer => new byte[] { 0x94, 0x7f, 0xc0, 0xc0, 0xc0 };

        /// <summary>
        /// Writes a server heartbeat message for sending response from the client.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteServerHeartbeatMessageResponse(IBufferWriter<byte> bufferWriter)
        {
            bufferWriter.Write(ServerHeartbeatMessageForClientToServer);
            //var writer = new MessagePackWriter(bufferWriter);
            //writer.WriteArrayHeader(4);
            //writer.Write(0x7f); // Type = 0x7f / 127 (Heartbeat)
            //writer.WriteNil(); // Dummy
            //writer.WriteNil(); // Dummy
            //writer.WriteNil(); // Dummy
            //writer.Flush();
        }

        // Array(4)[0x7e(126), Nil, Nil, <Extra>]
        static ReadOnlySpan<byte> ClientHeartbeatMessageHeader => new byte[] { 0x94, 0x7e, 0xc0, 0xc0 };

        /// <summary>
        /// Writes a client heartbeat message for sending from the client.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteClientHeartbeatMessageHeader(IBufferWriter<byte> bufferWriter)
        {
            bufferWriter.Write(ClientHeartbeatMessageHeader);
            //var writer = new MessagePackWriter(bufferWriter);
            //writer.WriteArrayHeader(4);
            //writer.Write(0x7f); // Type = 0x7e / 126 (ClientHeartbeat)
            //writer.WriteNil();  // Dummy
            //writer.WriteNil();  // Dummy
            //writer.Flush();
            //                    // <Extra>
        }

        // Array(5)[0x7e(126), Nil, Nil, Nil, <Extra>]
        static ReadOnlySpan<byte> ClientHeartbeatMessageResponseHeader => new byte[] { 0x95, 0x7e, 0xc0, 0xc0, 0xc0 };

        /// <summary>
        /// Writes a client heartbeat message for sending response from the server.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteClientHeartbeatMessageResponseHeader(IBufferWriter<byte> bufferWriter)
        {
            bufferWriter.Write(ClientHeartbeatMessageResponseHeader);
            //var writer = new MessagePackWriter(bufferWriter);
            //writer.WriteArrayHeader(5);
            //writer.Write(0x7f); // Type = 0x7e / 126 (Heartbeat)
            //writer.WriteNil(); // Dummy
            //writer.WriteNil(); // Dummy
            //writer.WriteNil(); // Dummy
            //writer.Flush();
            //                    // <Extra>
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
}
