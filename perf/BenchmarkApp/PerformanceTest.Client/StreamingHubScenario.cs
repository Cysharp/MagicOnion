using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public class StreamingHubScenario : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await client.CallMethodAsync("Foo", 123);
            ctx.Increment();
        }
    }
}