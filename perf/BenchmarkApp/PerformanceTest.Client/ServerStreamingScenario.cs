using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class ServerStreamingScenario : IScenario
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
        var stream = await client.ServerStreamingAsync();
        while(!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            var begin = timeProvider.GetTimestamp();
            //_ = stream.ResponseStream.Current;
            await stream.ResponseStream.MoveNext(cancellationToken);
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public Task CompleteAsync()
    {
        return Task.CompletedTask;
    }
}
