using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MagicOnion;
using MagicOnion.Client.DynamicClient;
using Microbenchmark.Client;

BenchmarkRunner.Run<Benchmarks>();

[MemoryDiagnoser, RankColumn]
[ShortRunJob]
public class Benchmarks
{
    readonly StreamingHubClientTestHelper<ITestHub, ITestHubReceiver> helper;
    readonly Task responseTask;
    readonly ITestHub client;

    static ReadOnlySpan<byte> IntResponse => [0xcd, 0x30, 0x39 /* 12345 (int) */];
    static ReadOnlySpan<byte> NilResponse => [0xc0 /* Nil */];
    static ReadOnlySpan<byte> StringResponse => [0xa6, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x21 /* "Hello!" (string) */];

    public Benchmarks()
    {
        this.helper = new StreamingHubClientTestHelper<ITestHub, ITestHubReceiver>(new TestHubReceiver(), DynamicStreamingHubClientFactoryProvider.Instance);
        this.responseTask = Task.Run(async () =>
        {
            while (true)
            {
                var req = await helper.ReadRequestNoDeserializeAsync();
                if (req.MethodId is - 2087943100 or 1273874383)
                {
                    // Parameter_Zero_Return_ValueType, Parameter_Many_Return_ValueType
                    helper.WriteResponse(req.MessageId, req.MethodId, IntResponse);
                }
                else if (req.MethodId is -1841486598)
                {
                    // ValueTask_Parameter_Zero_NoReturn
                    helper.WriteResponse(req.MessageId, req.MethodId, NilResponse);
                }
                else if (req.MethodId is -440496944 or -1110031569)
                {
                    // Parameter_Zero_Return_RefType, Parameter_Many_Return_RefType
                    helper.WriteResponse(req.MessageId, req.MethodId, StringResponse);
                }
            }
        });
        this.client = helper.ConnectAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task Void_Parameter_Zero_NoReturn()
    {
        client.Void_Parameter_Zero_NoReturn();
    }

    [Benchmark]
    public async Task ValueTask_Parameter_Zero_NoReturn()
    {
        await client.ValueTask_Parameter_Zero_NoReturn();
    }

    [Benchmark]
    public async Task Parameter_Zero_Return_ValueType()
    {
        var value = await client.Parameter_Zero_Return_ValueType();
    }

    [Benchmark]
    public async Task Parameter_Many_Return_ValueType()
    {
        var value = await client.Parameter_Many_Return_ValueType("Hello", 12345, true);
    }

    [Benchmark]
    public async Task Parameter_Zero_Return_RefType()
    {
        var value = await client.Parameter_Zero_Return_RefType();
    }

    [Benchmark]
    public async Task Parameter_Many_Return_RefType()
    {
        var value = await client.Parameter_Many_Return_RefType("Hello", 12345, true);
    }
}

class TestHubReceiver : ITestHubReceiver;

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

}
