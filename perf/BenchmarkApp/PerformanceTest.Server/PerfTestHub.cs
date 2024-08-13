using MagicOnion.Server.Hubs;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestHub : StreamingHubBase<IPerfTestHub, IPerfTestHubReceiver>, IPerfTestHub
{
    public Task<int> CallMethodAsync(string arg1, int arg2, int arg3, int arg4)
    {
        return Task.FromResult(0);
    }

    public ValueTask<int> CallMethodValueTaskAsync(string arg1, int arg2, int arg3, int arg4)
    {
        return ValueTask.FromResult(0);
    }

    public Task<ComplexResponse> CallMethodComplexAsync(string arg1, int arg2, int arg3, int arg4)
    {
        return Task.FromResult(ComplexResponse.Cached);
    }

    public ValueTask<ComplexResponse> CallMethodComplexValueTaskAsync(string arg1, int arg2, int arg3, int arg4)
    {
        return ValueTask.FromResult(ComplexResponse.Cached);
    }

    public Task<(int StatusCode, byte[] Data)> CallMethodLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5)
    {
        return Task.FromResult((123, arg5));
    }

    public Task<SimpleResponse> PingpongAsync(SimpleRequest request)
    {
        return Task.FromResult(request.UseCache ? SimpleResponse.Cached : new SimpleResponse
        {
            Payload = request.ResponseSize == 0 ? [] : new byte[request.ResponseSize],
        });
    }

    public Task<SimpleResponse> PingpongCachedAsync(SimpleRequest request)
    {
        return Task.FromResult(SimpleResponse.Cached);
    }
}
