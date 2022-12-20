using System.Buffers;
using System.Reflection;
using MagicOnion.Serialization;

namespace MagicOnion.Integration.Tests;

public class XorMessagePackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
{
    const int MagicNumber = 0x11;

    readonly MessagePackSerializerOptions serializerOptions;

    public static IMagicOnionSerializerProvider Instance { get; } = new XorMessagePackMagicOnionSerializerProvider(MessagePackSerializer.DefaultOptions);

    XorMessagePackMagicOnionSerializerProvider(MessagePackSerializerOptions serializerOptions)
    {
        this.serializerOptions = serializerOptions;
    }

    class XorMessagePackMagicOnionSerializer : IMagicOnionSerializer
    {
        readonly MessagePackSerializerOptions serializerOptions;

        public XorMessagePackMagicOnionSerializer(MessagePackSerializerOptions serializerOptions)
        {
            this.serializerOptions = serializerOptions;
        }

        public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
        {
            var array = ArrayPool<byte>.Shared.Rent((int)bytes.Length);
            try
            {
                bytes.CopyTo(array);
                for (var i = 0; i < bytes.Length; i++)
                {
                    array[i] ^= MagicNumber;
                }
                return MessagePackSerializer.Deserialize<T>(array.AsMemory(0, (int)bytes.Length), serializerOptions);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public void Serialize<T>(IBufferWriter<byte> writer, in T value)
        {
            var serialized = MessagePackSerializer.Serialize(value, serializerOptions);
            for (var i = 0; i < serialized.Length; i++)
            {
                serialized[i] ^= MagicNumber;
            }
            writer.Write(serialized);
        }
    }

    public IMagicOnionSerializer Create(MethodType methodType, MethodInfo methodInfo)
    {
        return new XorMessagePackMagicOnionSerializer(serializerOptions);
    }
}
