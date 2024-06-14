using BenchmarkDotNet.Attributes;
using MagicOnion.Client.DynamicClient;

namespace Microbenchmark.Client;

[MemoryDiagnoser, RankColumn]
[ShortRunJob]
public class HubReceiverBroadcastBenchmarks
{
    StreamingHubClientTestHelper<ITestHub, ITestHubReceiver> helper = default!;
    ITestHub client = default!;
    TestHubReceiver receiver = default!;

    static ReadOnlySpan<byte> BroadcastMessage_Parameter_Zero => [0x92, 0xce, 0x76, 0xe4, 0x37, 0x1b /* 1994667803 */, 0xc0 /* Nil */]; // [MethodId(int), SerializedArgument]
    static ReadOnlySpan<byte> BroadcastMessage_Parameter_Many => [0x92, 0xce, 0x4c, 0xb8, 0x83, 0xca /* 1287160778 */, 0x93, 0xa6, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x21, 0xcd, 0x30, 0x39, 0xc3 /* [ "Hello", 12345, true ] */]; // [MethodId(int), SerializedArgument]

    void Setup()
    {
        this.receiver = new TestHubReceiver();
        this.helper = new StreamingHubClientTestHelper<ITestHub, ITestHubReceiver>(receiver, DynamicStreamingHubClientFactoryProvider.Instance);
        this.client = helper.ConnectAsync().GetAwaiter().GetResult();
    }

    [GlobalSetup(Targets = [nameof(Parameter_Zero), nameof(Parameter_Many)])]
    public void UnsetSynchronizationContext()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        Setup();
    }

    [GlobalSetup(Targets = [nameof(Parameter_Zero_With_SynchronizationContext), nameof(Parameter_Many_With_SynchronizationContext)])]
    public void SetSynchronizationContext()
    {
        SynchronizationContext.SetSynchronizationContext(new MySynchronizationContext());
        Setup();
    }

    [Benchmark]
    public void Parameter_Zero()
    {
        helper.WriteResponseRaw(BroadcastMessage_Parameter_Zero);
        receiver.Received.Wait();
    }

    [Benchmark]
    public void Parameter_Many()
    {
        helper.WriteResponseRaw(BroadcastMessage_Parameter_Many);
        receiver.Received.Wait();
    }

    [Benchmark]
    public void Parameter_Zero_With_SynchronizationContext()
    {
        helper.WriteResponseRaw(BroadcastMessage_Parameter_Zero);
        receiver.Received.Wait();
    }

    [Benchmark]
    public void Parameter_Many_With_SynchronizationContext()
    {
        helper.WriteResponseRaw(BroadcastMessage_Parameter_Many);
        receiver.Received.Wait();
    }
}
