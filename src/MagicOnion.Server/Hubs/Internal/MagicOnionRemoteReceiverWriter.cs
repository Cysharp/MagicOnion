using Cysharp.Runtime.Multicast.Remoting;
using MagicOnion.Internal;

namespace MagicOnion.Server.Hubs.Internal;

internal class MagicOnionRemoteReceiverWriter : IRemoteReceiverWriter
{
    readonly StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> writer;

    public MagicOnionRemoteReceiverWriter(StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> writer)
    {
        this.writer = writer;
    }

    public void Write(ReadOnlyMemory<byte> payload)
    {
        writer.QueueResponseStreamWrite(StreamingHubPayloadPool.Shared.RentOrCreate(payload.Span));
    }
}
