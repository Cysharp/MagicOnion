using System.Buffers;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Integration.Tests;

public class XorMagicOnionMessagePackSerializer : MagicOnionMessagePackMessageSerializer
{
    const int MagicNumber = 0x11;

    public static MagicOnionMessagePackMessageSerializer Default { get; } = new XorMagicOnionMessagePackSerializer(MessagePackSerializer.DefaultOptions, enableFallback: false);

    private XorMagicOnionMessagePackSerializer(MessagePackSerializerOptions serializerOptions, bool enableFallback)
        : base(serializerOptions, enableFallback)
    { }

    protected override MagicOnionMessagePackMessageSerializer Create(MessagePackSerializerOptions serializerOptions, bool enableFallback)
        => new XorMagicOnionMessagePackSerializer(serializerOptions, enableFallback);

    protected override T Deserialize<T>(in ReadOnlySequence<byte> bytes, MessagePackSerializerOptions serializerOptions)
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

    protected override void Serialize<T>(IBufferWriter<byte> writer, in T value, MessagePackSerializerOptions serializerOptions)
    {
        var serialized = MessagePackSerializer.Serialize(value, serializerOptions);
        for (var i = 0; i < serialized.Length; i++)
        {
            serialized[i] ^= MagicNumber;
        }
        writer.Write(serialized);
    }
}
