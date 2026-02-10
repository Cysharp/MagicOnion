using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestBroadcastHub(PerfGroupService groupService) : StreamingHubBase<IPerTestBroadcastHub, IPerTestBroadcastHubReceiver>, IPerTestBroadcastHub
{
    public async ValueTask<string> JoinGroupAsync()
    {
        groupService.AddMember(Context.ContextId, Client);
        return $"{Context.ContextId} joined.";
    }

    public async ValueTask<string> LeaveGroupAsync()
    {
        groupService.RemoveMember(Context.ContextId);
        return $"{Context.ContextId} leaved.";
    }

    public async ValueTask UpdatePositionAsync(BroadcastPositionMessage position)
    {
        // Update position info for broadcasting
        groupService.UpdatePosition(position);
    }
}
