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
            await client.CallMethodAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Increment();
        }
    }
}

public class StreamingHubValueTaskScenario : IScenario, IPerfTestHubReceiver
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
            await client.CallMethodValueTaskAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Increment();
        }
    }
}

public class StreamingHubComplexScenario : IScenario, IPerfTestHubReceiver
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
            await client.CallMethodComplexAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Increment();
        }
    }
}

public class StreamingHubComplexValueTaskScenario : IScenario, IPerfTestHubReceiver
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
            await client.CallMethodComplexValueTaskAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Increment();
        }
    }
}

public class PingpongStreamingHubScenario : IScenario, IPerfTestHubReceiver
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
            await client.PingpongAsync(new SimpleRequest
            {
                Payload = new byte[100],
                ResponseSize = 100,
                UseCache = false,
            });
            ctx.Increment();
        }
    }
}

public class PingpongCachedStreamingHubScenario : IScenario, IPerfTestHubReceiver
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
            await client.PingpongAsync(SimpleRequest.Cached);
            ctx.Increment();
        }
    }
}
