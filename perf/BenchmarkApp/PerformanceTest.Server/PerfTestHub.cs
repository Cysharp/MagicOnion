using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestHub : StreamingHubBase<IPerfTestHub, IPerfTestHubReceiver>, IPerfTestHub
{
    public Task<int> CallMethodAsync(string arg1, int arg2)
    {
        return Task.FromResult(0);
    }

    public Task<(int StatusCode, byte[] Data)> CallMethodLargePayloadAsync(string arg1, int arg2, byte[] arg3)
    {
        return Task.FromResult((123, arg3));
    }
}