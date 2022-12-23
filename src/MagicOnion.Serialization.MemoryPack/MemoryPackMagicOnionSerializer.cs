#nullable enable
using System.Buffers;
using System.Reflection;
using Grpc.Core;
using MemoryPack;

namespace MagicOnion.Serialization.MemoryPack
{
    public class MemoryPackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
    {
        public static IMagicOnionSerializerProvider Instance { get; } = new MemoryPackMagicOnionSerializerProvider();

        static MemoryPackMagicOnionSerializerProvider()
        {
            DynamicArgumentTupleFormatter.Register();
        }

        public IMagicOnionSerializer Create(MethodType methodType, MethodInfo methodInfo)
        {
            return new MagicOnionSerializer(MemoryPackSerializerOptions.Default);
        }

        class MagicOnionSerializer : IMagicOnionSerializer
        {
            readonly MemoryPackSerializerOptions serializerOptions;

            public MagicOnionSerializer(MemoryPackSerializerOptions serializerOptions)
            {
                this.serializerOptions = serializerOptions;
            }

            public void Serialize<T>(IBufferWriter<byte> writer, in T value)
                => MemoryPackSerializer.Serialize(writer, value, serializerOptions);

            public T? Deserialize<T>(in ReadOnlySequence<byte> bytes)
                => MemoryPackSerializer.Deserialize<T>(bytes, serializerOptions);
        }
    }
}
