using System.Buffers;

namespace MagicOnion.Client.Tests;

public class MockSerializationContext : SerializationContext
{
    readonly ArrayBufferWriter<byte> _writer = new ArrayBufferWriter<byte>();

    public override IBufferWriter<byte> GetBufferWriter()
        => _writer;

    public int? PayloadLength { get; private set; }

    public override void SetPayloadLength(int payloadLength)
        => PayloadLength = payloadLength;

    public override void Complete(byte[] payload)
    {
        _writer.Clear();
        _writer.Write(payload);
    }

    public override void Complete()
    {
    }

    public ReadOnlyMemory<byte> ToMemory()
        => _writer.WrittenMemory;
}