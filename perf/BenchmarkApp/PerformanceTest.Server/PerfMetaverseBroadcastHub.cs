using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfMetaverseBroadcastHub(MetaverseGroupService groupService, MetaverseWorld metaverseWorld) : StreamingHubBase<IMetaverseBroadcastHub, IMetaverseBroadcastHubReceiver>, IMetaverseBroadcastHub
{
    public async ValueTask<string> JoinAsync(int targetFps)
    {
        groupService.AddMember(Context.ContextId, Client);

        return $"{Context.ContextId} joined metaverse world.";
    }

    public async ValueTask<string> LeaveAsync()
    {
        groupService.RemoveMember(Context.ContextId);

        return $"{Context.ContextId} left metaverse world.";
    }

    public async ValueTask UpdatePositionAsync(BroadcastPositionMessage position)
    {
        groupService.UpdatePosition(Context.ContextId, position);
    }

    public async ValueTask StartBroadcast(int targetFps)
    {
        // start metrics collection
        groupService.StartMetricsCollection(targetFps);
        // start broadcast timer
        metaverseWorld.StartBroadcast(targetFps, () => groupService.BroadcastAllPositions());
    }

    public async ValueTask StopBroadcast()
    {
        // stop broadcast timer
        metaverseWorld.StopBroadcast();

        // Stop metrics collection, send metrics and clear collected metrics
        groupService.StopMetricsCollection();
        await groupService.SendAndClearMetricsAsync();
    }
}
