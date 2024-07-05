using System.Reflection;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Integration.Tests;

public class StreamingHubStressTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubStressTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubStressTestHub> factory;

    public StreamingHubStressTest(MagicOnionApplicationFactory<StreamingHubStressTestHub> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new[] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
        yield return new[] { new TestStreamingHubClientFactory("Static", MagicOnionGeneratedClientInitializer.StreamingHubClientFactoryProvider) };
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Short(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cts = new CancellationTokenSource();
        var count = 0;

        var tasks = Enumerable.Range(0, Environment.ProcessorCount * 10).Select(async x =>
        {
            var httpClient = factory.CreateDefaultClient();
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
            var receiver = Substitute.For<IStreamingHubStressTestHubReceiver>();
            var client = await clientFactory.CreateAndConnectAsync<IStreamingHubStressTestHub, IStreamingHubStressTestHubReceiver>(channel, receiver);

            await tcs.Task;

            while (!cts.IsCancellationRequested)
            {
                var response = await client.Method((x * 100) + count, $"Task{x}-{count}", x % 2 == 0).AsTask().WaitAsync(cts.Token);
                Interlocked.Increment(ref count);
            }

            await Task.Delay(500);
        });

        // Act
        tcs.TrySetResult();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException e) when (e.CancellationToken == cts.Token)
        {
        }
    }
}


public interface IStreamingHubStressTestHub : IStreamingHub<IStreamingHubStressTestHub, IStreamingHubStressTestHubReceiver>
{
    ValueTask<(int, string, bool)> Method(int arg0, string arg1, bool arg2);
}

public interface IStreamingHubStressTestHubReceiver
{
    void OnMessage();
}

[Heartbeat(Enable = true, Interval = 1000, Timeout = 1000)]
public class StreamingHubStressTestHub : StreamingHubBase<IStreamingHubStressTestHub, IStreamingHubStressTestHubReceiver>, IStreamingHubStressTestHub
{
    IGroup<IStreamingHubStressTestHubReceiver> defaultGroup = default!;

    protected override async ValueTask OnConnected()
    {
        defaultGroup = await Group.AddAsync("_");
    }

    public ValueTask<(int, string, bool)> Method(int arg0, string arg1, bool arg2)
    {
        defaultGroup.All.OnMessage();
        return new ValueTask<(int, string, bool)>((arg0, arg1, arg2));
    }
}
