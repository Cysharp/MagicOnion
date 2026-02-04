using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfGroupService(IMulticastGroupProvider groupProvider, TimeProvider timeProvider) : IDisposable
{
    readonly IMulticastSyncGroup<Guid, IPerTestBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IPerTestBroadcastHubReceiver>("PerformanceTest");
    readonly ServerBroadcastMetricsContext metricsContext = new(timeProvider);
    int memberCount;

    public ServerBroadcastMetricsContext MetricsContext => metricsContext;
    public int MemberCount => memberCount;

    public void SendMessageToAll(BroadcastPositionMessage response)
    {
        group.All.OnMessage(response);
        metricsContext.IncrementMessageCount();
    }

    public void AddMember(Guid id, IPerTestBroadcastHubReceiver receiver)
    {
        group.Add(id, receiver);
        var newCount = Interlocked.Increment(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
    }

    public void RemoveMember(Guid id)
    {
        group.Remove(id);
        var newCount = Interlocked.Decrement(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
    }

    public void Dispose() => group.Dispose();
}
