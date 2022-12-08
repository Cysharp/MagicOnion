using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Utils;

namespace MagicOnion
{
    public interface IMagicOnionMessageSerializerProvider
    {
#if NET5_0_OR_GREATER
        IMagicOnionMessageSerializer Create(MethodType methodType, MethodInfo? methodInfo);
#else
        IMagicOnionMessageSerializer Create(MethodType methodType, MethodInfo methodInfo);
#endif
    }

    public interface IMagicOnionMessageSerializer
    {
        void Serialize<T>(IBufferWriter<byte> writer, in T value);
        T Deserialize<T>(in ReadOnlySequence<byte> bytes);
    }

    public static class MagicOnionMessageSerializerProvider
    {
        public static IMagicOnionMessageSerializerProvider Default { get; set; } = MessagePackMessageMagicOnionSerializerProvider.Instance;
    }
}
