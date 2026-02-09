using Cysharp.Runtime.Multicast.Remoting;
using MagicOnion.Internal;
using MagicOnion.Server.Hubs.Internal.DataChannel;

namespace MagicOnion.Server.Hubs.Internal;

internal class MagicOnionRemoteReceiverWriter : IRemoteReceiverWriter
{
    readonly StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> writer;
    readonly HubReceiverMethodReliabilityMap reliabilityMap;
    readonly ServerDataChannel? dataChannel;

    public IRemoteClientResultPendingTaskRegistry PendingTasks { get; }

    public MagicOnionRemoteReceiverWriter(
        StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> writer,
        IRemoteClientResultPendingTaskRegistry pendingTasks,
        HubReceiverMethodReliabilityMap reliabilityMap,
        ServerDataChannel? dataChannel)
    {
        this.writer = writer;
        PendingTasks = pendingTasks;
        this.reliabilityMap = reliabilityMap;
        this.dataChannel = dataChannel;
    }

    public void Write(InvocationWriteContext context)
    {
        var payload = StreamingHubPayloadPool.Shared.RentOrCreate(context.Payload.Span);
        if (dataChannel is not null &&
            reliabilityMap.ReliabilityByMethodId.TryGetValue(context.MethodId, out var reliability) &&
            reliability != TransportReliability.Reliable)
        {
            // Unreliable or Reliable unordered
            dataChannel.SendPayload(payload);
        }
        else
        {
            writer.QueueResponseStreamWrite(payload);
        }
    }
}
