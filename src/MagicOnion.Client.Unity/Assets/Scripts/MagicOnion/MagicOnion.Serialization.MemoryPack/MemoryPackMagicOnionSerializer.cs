#nullable enable
using System.Buffers;
using System.Reflection;
using Grpc.Core;
using MemoryPack;

namespace MagicOnion.Serialization.MemoryPack
{
    public partial class MemoryPackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
    {
        readonly MemoryPackSerializerOptions serializerOptions;
        public static MemoryPackMagicOnionSerializerProvider Instance { get; } = new MemoryPackMagicOnionSerializerProvider(MemoryPackSerializerOptions.Default);

        MemoryPackMagicOnionSerializerProvider(MemoryPackSerializerOptions serializerOptions)
        {
            this.serializerOptions = serializerOptions;
        }

        static MemoryPackMagicOnionSerializerProvider()
        {
            DynamicArgumentTupleFormatter.Register();
        }

        public MemoryPackMagicOnionSerializerProvider WithOptions(MemoryPackSerializerOptions serializerOptions)
            => new MemoryPackMagicOnionSerializerProvider(serializerOptions);

        public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
        {
            return new MagicOnionSerializer(serializerOptions);
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

            public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
                => MemoryPackSerializer.Deserialize<T>(bytes, serializerOptions)!;
        }
    }
}
