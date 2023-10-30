using System.Buffers;
using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Serialization
{
    /// <summary>
    /// Provides a serializer for request/response of MagicOnion services and hub methods.
    /// </summary>
    public interface IMagicOnionSerializerProvider
    {
        /// <summary>
        /// Create a serializer for the service method.
        /// </summary>
        /// <param name="methodType">gRPC method type of the method.</param>
        /// <param name="methodInfo">A method info for an implementation of the service method. It is a hint that handling request parameters on the server, which may be passed null on the client.</param>
        /// <returns></returns>
        IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo);
    }

    /// <summary>
    /// Provides a processing for message serialization.
    /// </summary>
    public interface IMagicOnionSerializer
    {
        void Serialize<T>(IBufferWriter<byte> writer, in T value);
        T Deserialize<T>(in ReadOnlySequence<byte> bytes);
    }
}
