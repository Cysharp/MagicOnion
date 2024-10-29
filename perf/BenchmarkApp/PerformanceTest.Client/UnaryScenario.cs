using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public class UnaryScenario : IScenario
{
    IPerfTestService client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = MagicOnionClient.Create<IPerfTestService>(channel);
        return ValueTask.CompletedTask;
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            await client.UnaryArgDynamicArgumentTupleReturnValue("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public Task CompleteAsync()
    {
        return Task.CompletedTask;
    }
}

public class UnaryComplexScenario : IScenario
{
    IPerfTestService client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = MagicOnionClient.Create<IPerfTestService>(channel);
        return ValueTask.CompletedTask;
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            await client.UnaryComplexAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public Task CompleteAsync()
    {
        return Task.CompletedTask;
    }
}
