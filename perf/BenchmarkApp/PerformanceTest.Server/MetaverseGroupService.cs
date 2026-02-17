using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class MetaverseGroupService(IMulticastGroupProvider groupProvider, TimeProvider timeProvider, MetaverseWorld metaverseWorld) : IDisposable
{
    readonly IMulticastSyncGroup<Guid, IMetaverseBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IMetaverseBroadcastHubReceiver>("MetaverseBroadcast");
    readonly ServerBroadcastMetricsContext metricsContext = new(timeProvider);
    int memberCount;

    public ServerBroadcastMetricsContext MetricsContext => metricsContext;
    public int MemberCount => memberCount;

    public void AddMember(Guid id, IMetaverseBroadcastHubReceiver receiver)
    {
        group.Add(id, receiver);
        var newCount = Interlocked.Increment(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
        metaverseWorld.AddClient(id);
    }

    public void RemoveMember(Guid id)
    {
        group.Remove(id);
        var newCount = Interlocked.Decrement(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
        metaverseWorld.RemoveClient(id);
    }

    public void UpdatePosition(Guid clientId, BroadcastPositionMessage position)
    {
        metaverseWorld.UpdateClientPosition(clientId, position.Position);
    }

    public void BroadcastAllPositions(Guid id)
    {
        var positions = metaverseWorld.GetAllClientPositions();
        var frameNumber = metaverseWorld.CurrentFrame;
        var message = new AllClientsPositionMessage(frameNumber, positions);

        // Broadcast to all clients except the sender
        group.Except(id).OnBroadcastAllPositions(message);
        metricsContext.IncrementMessageCount();
    }

    public void Dispose() => group.Dispose();
}
