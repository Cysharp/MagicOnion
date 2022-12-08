using System.Buffers;
using System.Reflection;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Server.Tests;

public class XorMessagePackMagicOnionMessageSerializerProvider : IMagicOnionMessageSerializerProvider
{
    const int MagicNumber = 0x11;

    readonly MessagePackSerializerOptions serializerOptions;

    public static IMagicOnionMessageSerializerProvider Instance { get; } = new XorMessagePackMagicOnionMessageSerializerProvider(MessagePackSerializer.DefaultOptions);

    XorMessagePackMagicOnionMessageSerializerProvider(MessagePackSerializerOptions serializerOptions)
    {
        this.serializerOptions = serializerOptions;
    }

    class XorMessagePackMagicOnionMessageSerializer : IMagicOnionMessageSerializer
    {
        readonly MessagePackSerializerOptions serializerOptions;

        public XorMessagePackMagicOnionMessageSerializer(MessagePackSerializerOptions serializerOptions)
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

    public IMagicOnionMessageSerializer Create(MethodType methodType, MethodInfo methodInfo)
    {
        return new XorMessagePackMagicOnionMessageSerializer(serializerOptions);
    }
}
