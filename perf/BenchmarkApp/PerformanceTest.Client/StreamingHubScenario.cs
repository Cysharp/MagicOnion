using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public class StreamingHubScenario : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        //this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            await client.CallMethodAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public async Task CompleteAsync()
    {
        await this.client.DisposeAsync();
    }
}

public class StreamingHubValueTaskScenario : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            await client.CallMethodValueTaskAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public async Task CompleteAsync()
    {
        await this.client.DisposeAsync();
    }
}

public class StreamingHubComplexScenario : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            await client.CallMethodComplexAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public async Task CompleteAsync()
    {
        await this.client.DisposeAsync();
    }
}

public class StreamingHubComplexValueTaskScenario : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            await client.CallMethodComplexValueTaskAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public async Task CompleteAsync()
    {
        await this.client.DisposeAsync();
    }
}
