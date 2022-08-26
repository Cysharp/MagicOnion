using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestHub : StreamingHubBase<IPerfTestHub, IPerfTestHubReceiver>, IPerfTestHub
{
    public Task<int> CallMethodAsync(string arg1, int arg2)
    {
        return Task.FromResult(0);
    }
}