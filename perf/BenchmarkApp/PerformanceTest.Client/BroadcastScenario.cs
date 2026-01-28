using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class BroadcastScenario : IScenario, IPerTestBroadcastHubReceiver
{
    IPerfTestService client = default!;
    IPerTestBroadcastHub hubClient = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;
    PerformanceTestRunningContext context = default!;
    int connectionId;
    long begin;

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        client = MagicOnionClient.Create<IPerfTestService>(channel);
        hubClient = await StreamingHubClient.ConnectAsync<IPerTestBroadcastHub, IPerTestBroadcastHubReceiver>(channel, this);
    }

    // Follow what the grpc-dotnet benchmark does, but this ServerStreaming benchmark seems meaningless as MoveNext may concatenate multiple responses from the server.
    // So most times MoveNext won't wait at all, and it may wait occasionally.
    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        var start = TimeProvider.System.GetTimestamp();
        context = ctx;
        this.connectionId = connectionId;
        begin = timeProvider.GetTimestamp();
        await hubClient.JoinGroupAsync();
        await client.BroadcastAsync(ctx.Timeout);
        while (!cancellationToken.IsCancellationRequested && TimeProvider.System.GetElapsedTime(start) < ctx.Timeout)
        {
            await Task.Delay(10, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    public async Task CompleteAsync()
    {
        await hubClient.DisposeAsync();
    }

    public void OnMessage(SimpleResponse response)
    {
        // Collect only Count
        context.Increment();
        //context.LatencyThrottled(connectionId, timeProvider.GetElapsedTime(begin), 100); // avoid OOM
        //begin = timeProvider.GetTimestamp();
    }
}
