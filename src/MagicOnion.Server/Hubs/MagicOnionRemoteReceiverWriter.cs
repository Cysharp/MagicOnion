using Multicaster.Remoting;

namespace MagicOnion.Server.Hubs;

internal class MagicOnionRemoteReceiverWriter : IRemoteReceiverWriter
{
    readonly StreamingServiceContext<byte[], byte[]> writer;

    public MagicOnionRemoteReceiverWriter(StreamingServiceContext<byte[], byte[]> writer)
    {
        this.writer = writer;
    }

    public void Write(ReadOnlyMemory<byte> payload)
    {
        writer.QueueResponseStreamWrite(payload.ToArray());
    }
}
