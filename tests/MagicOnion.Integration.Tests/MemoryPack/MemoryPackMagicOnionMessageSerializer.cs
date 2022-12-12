using System.Buffers;
using System.Reflection;
using MagicOnion.Serialization;
using MemoryPack;

namespace MagicOnion.Integration.Tests.MemoryPack;

internal class MemoryPackMagicOnionMessageSerializerProvider : IMagicOnionMessageSerializerProvider
{
    public static IMagicOnionMessageSerializerProvider Instance { get; } = new MemoryPackMagicOnionMessageSerializerProvider();

    public IMagicOnionMessageSerializer Create(MethodType methodType, MethodInfo methodInfo)
    {
        return new MagicOnionMessageSerializer(MemoryPackSerializerOptions.Default);
    }

    class MagicOnionMessageSerializer : IMagicOnionMessageSerializer
    {
        readonly MemoryPackSerializerOptions serializerOptions;

        public MagicOnionMessageSerializer(MemoryPackSerializerOptions serializerOptions)
        {
            this.serializerOptions = serializerOptions;
        }

        public void Serialize<T>(IBufferWriter<byte> writer, in T value)
            => MemoryPackSerializer.Serialize(writer, value, serializerOptions);

        public T? Deserialize<T>(in ReadOnlySequence<byte> bytes)
            => MemoryPackSerializer.Deserialize<T>(bytes, serializerOptions);
    }
}
