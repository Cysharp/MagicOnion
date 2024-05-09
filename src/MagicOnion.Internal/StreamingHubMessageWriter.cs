using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Grpc.Core;
using MagicOnion.Internal.Buffers;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Internal
{
    /// <remarks>
    ///     <para>StreamingHub message formats:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <term>Broadcast: from server to client</term>
    ///             <description>[MethodId(int), SerializedArgument]</description>
    ///         </item>
    ///         <item>
    ///             <term>Request: InvokeHubMethod (from client; void; fire-and-forget)</term>
    ///             <description>[MethodId(int), SerializedArguments]</description>
    ///         </item>
    ///         <item>
    ///             <term>Request: InvokeHubMethod (from client; non-void)</term>
    ///             <description>[MessageId(int), MethodId(int), SerializedArguments]</description>
    ///         </item>
    ///         <item>
    ///             <term>Response: InvokeHubMethod (from server to client)</term>
    ///             <description>[MessageId(int), MethodId(int), SerializedResponse]</description>
    ///         </item>
    ///         <item>
    ///             <term>Response: InvokeHubMethod (from server to client; with Exception)</term>
    ///             <description>[MessageId(int), StatusCode(int), Detail(string), Message(string)]</description>
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
    }

    internal enum StreamingHubMessageType
    {
        Broadcast,
        Request,
        RequestFireAndForget,
        Response,
        ResponseWithError,
    }
}
