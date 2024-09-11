using MagicOnion;

namespace PerformanceTest.Shared;

public interface IPerfTestHub : IStreamingHub<IPerfTestHub, IPerfTestHubReceiver>
{
    Task<int> CallMethodAsync(string arg1, int arg2, int arg3, int arg4);
    ValueTask<int> CallMethodValueTaskAsync(string arg1, int arg2, int arg3, int arg4);
    Task<ComplexResponse> CallMethodComplexAsync(string arg1, int arg2, int arg3, int arg4);
    ValueTask<ComplexResponse> CallMethodComplexValueTaskAsync(string arg1, int arg2, int arg3, int arg4);
    Task<(int StatusCode, byte[] Data)> CallMethodLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5);
    Task<SimpleResponse> PingpongAsync(SimpleRequest request);
}

public interface IPerfTestHubReceiver
{}
