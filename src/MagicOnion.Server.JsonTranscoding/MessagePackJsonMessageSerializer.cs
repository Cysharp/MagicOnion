using MagicOnion.Serialization;
using System.Buffers;
using System.Text;
using MagicOnion.Internal.Buffers;
using MessagePack;

namespace MagicOnion.Server.JsonTranscoding;

public class MessagePackJsonMessageSerializer : IMagicOnionSerializer
{
    readonly MessagePackSerializerOptions options;

    public MessagePackJsonMessageSerializer(MessagePackSerializerOptions options)
    {
        this.options = options;
    }

    public void Serialize<T>(IBufferWriter<byte> writer, in T value)
    {
        using var bufferWriter = ArrayPoolBufferWriter.RentThreadStaticWriter();
        MessagePackSerializer.Serialize(bufferWriter, value, options);

        var json = MessagePackSerializer.ConvertToJson(bufferWriter.WrittenMemory, options);
        writer.Write(Encoding.UTF8.GetBytes(json));
    }

    public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
    {
        var messagePackBytes = MessagePackSerializer.ConvertFromJson(Encoding.UTF8.GetString(bytes.ToArray()));
        return MessagePackSerializer.Deserialize<T>(messagePackBytes, options);
    }
}
