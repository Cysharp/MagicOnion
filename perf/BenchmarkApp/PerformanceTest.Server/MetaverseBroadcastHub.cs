using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class MetaverseBroadcastHub(MetaverseGroupService groupService, MetaverseWorld metaverseWorld) : StreamingHubBase<IMetaverseBroadcastHub, IMetaverseBroadcastHubReceiver>, IMetaverseBroadcastHub
{
    public async ValueTask<string> JoinAsync(int targetFps)
    {
        groupService.AddMember(Context.ContextId, Client);
        
        // Start broadcast timer if this is the first client
        if (groupService.MemberCount == 1)
        {
            metaverseWorld.StartBroadcast(targetFps, () => groupService.BroadcastAllPositions(Context.ContextId));
        }
        
        return $"{Context.ContextId} joined metaverse world.";
    }

    public async ValueTask<string> LeaveAsync()
    {
        groupService.RemoveMember(Context.ContextId);
        
        // Stop broadcast timer if this is the last client
        if (groupService.MemberCount == 0)
        {
            metaverseWorld.StopBroadcast();
        }
        
        return $"{Context.ContextId} left metaverse world.";
    }

    public async ValueTask UpdatePositionAsync(BroadcastPositionMessage position)
    {
        groupService.UpdatePosition(Context.ContextId, position);
    }
}
