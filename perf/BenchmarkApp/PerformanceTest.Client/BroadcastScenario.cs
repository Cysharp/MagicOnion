using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class BroadcastScenario : IScenario, IPerTestBroadcastHubReceiver
{
    IPerfTestService client = default!;
    IPerTestBroadcastHub hubClient = default!;
    PerformanceTestRunningContext context = default!;
    int connectionId;
    long begin;

    protected virtual int TargetFps => 0; // 0 means maximum speed

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        client = MagicOnionClient.Create<IPerfTestService>(channel);
        hubClient = await StreamingHubClient.ConnectAsync<IPerTestBroadcastHub, IPerTestBroadcastHubReceiver>(channel, this);
    }

    // Follow what the grpc-dotnet benchmark does, but this ServerStreaming benchmark seems meaningless as MoveNext may concatenate multiple responses from the server.
    // So most times MoveNext won't wait at all, and it may wait occasionally.
    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        context = ctx;
        this.connectionId = connectionId;
        await hubClient.JoinGroupAsync();

        // Only the first client triggers broadcast after warmup completes
        if (connectionId == 0)
        {
            // Wait for warmup to complete
            await ctx.WaitForReadyAsync();

            // Use exact duration to match client-side metrics collection period
            var duration = TimeSpan.FromSeconds(ctx.DurationSeconds);
            _ = client.BroadcastAsync(duration, TargetFps);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    public async Task CompleteAsync()
    {
        if (hubClient is not null)
        {
            await hubClient.LeaveGroupAsync();
            await hubClient.DisposeAsync();
        }
    }

    public void OnMessage(BroadcastPositionMessage message)
    {
        // Collect only Count
        context.Increment();
    }
}

public class Broadcast60FpsScenario : BroadcastScenario
{
    protected override int TargetFps => 60;
}

public class Broadcast30FpsScenario : BroadcastScenario
{
    protected override int TargetFps => 30;
}

public class Broadcast15FpsScenario : BroadcastScenario
{
    protected override int TargetFps => 15;
}
