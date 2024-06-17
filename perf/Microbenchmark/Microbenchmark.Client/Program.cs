using BenchmarkDotNet.Running;
using MagicOnion;
using Microbenchmark.Client;

//BenchmarkRunner.Run<HubReceiverBroadcastBenchmarks>();
BenchmarkRunner.Run<HubMethodBenchmarks>();

#if FALSE
var b = new HubMethodBenchmarks();
for (var i = 0; i < 1000000; i++)
    await b.Parameter_Zero_Return_ValueType();
#endif

class MySynchronizationContext : SynchronizationContext;

class TestHubReceiver : ITestHubReceiver
{
    public ManualResetEventSlim Received { get; } = new();

    public void Parameter_Zero()
    {
        Received.Set();
    }

    public void Parameter_Many(string arg0, int arg1, bool arg2)
    {
        Received.Set();
    }
}

public interface ITestHub : IStreamingHub<ITestHub, ITestHubReceiver>
{
    void Void_Parameter_Zero_NoReturn();
    ValueTask ValueTask_Parameter_Zero_NoReturn();

    Task<int> Parameter_Zero_Return_ValueType();
    Task<int> Parameter_Many_Return_ValueType(string arg0, int arg1, bool arg2);
    Task<string> Parameter_Zero_Return_RefType();
    Task<string> Parameter_Many_Return_RefType(string arg0, int arg1, bool arg2);
}

public interface ITestHubReceiver
{
    void Parameter_Zero();
    void Parameter_Many(string arg0, int arg1, bool arg2);
}
