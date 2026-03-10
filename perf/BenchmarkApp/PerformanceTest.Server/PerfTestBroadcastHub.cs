using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestBroadcastHub(PerfGroupService groupService) : StreamingHubBase<IPerTestBroadcastHub, IPerTestBroadcastHubReceiver>, IPerTestBroadcastHub
{
    public async ValueTask<string> JoinGroupAsync()
    {
#if MAGICONION_NUGET_SERVER
        var group = await Group.AddAsync("PerformanceTestBroadcast");
        groupService.AddMember(Context.ContextId, BroadcastToSelf(group));
#else
        groupService.AddMember(Context.ContextId, Client);
#endif
        return $"{Context.ContextId} joined.";
    }

    public async ValueTask<string> LeaveGroupAsync()
    {
        groupService.RemoveMember(Context.ContextId);
        return $"{Context.ContextId} leaved.";
    }
}
