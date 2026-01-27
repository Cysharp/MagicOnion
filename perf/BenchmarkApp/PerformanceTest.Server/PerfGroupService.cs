using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfGroupService(IMulticastGroupProvider groupProvider) : IDisposable
{
    private readonly IMulticastSyncGroup<Guid, IPerTestBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IPerTestBroadcastHubReceiver>("PerformanceTest");

    public void SendMessageToAll(SimpleResponse response) => group.All.OnMessage(response);
    public void AddMember(Guid id, IPerTestBroadcastHubReceiver receiver) => group.Add(id, receiver);
    public void RemoveMember(Guid id) => group.Remove(id);

    public void Dispose() => group.Dispose();
}
