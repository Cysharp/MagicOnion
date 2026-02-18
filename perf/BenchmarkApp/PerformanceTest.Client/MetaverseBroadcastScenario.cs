using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class MetaverseBroadcastScenario : IScenario, IMetaverseBroadcastHubReceiver
{
    const int clientFps = 15;
    static Random random = new(76); // Fixed seed for reproducibility

    IMetaverseBroadcastHub hubClient = default!;
    PerformanceTestRunningContext context = default!;
    int connectionId;

    protected virtual int TargetFps => 30; // Default 30 FPS

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        hubClient = await StreamingHubClient.ConnectAsync<IMetaverseBroadcastHub, IMetaverseBroadcastHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        context = ctx;
        this.connectionId = connectionId;
        await hubClient.JoinAsync(TargetFps);

        // Wait for warmup to complete
        await ctx.WaitForReadyAsync();

        // jitter to avoid all clients sending updates at the same time
        await Task.Delay(random.Next(0, 500), cancellationToken);

        if (connectionId == 0)
        {
            await hubClient.StartBroadcast(TargetFps);
        }

        // Simulate client position updates periodically (fixed to 15 FPS for all scenarios to isolate broadcast performance)
        var interval = TimeSpan.FromMilliseconds(1000.0 / clientFps);
        using var updateTimer = new PeriodicTimer(interval);
        
        try
        {
            while (await updateTimer.WaitForNextTickAsync(cancellationToken))
            {
                // Send random position update
                var position = new BroadcastPositionMessage(
                    connectionId,
                    new Vector3(
                        (float)(random.NextDouble() * 1000),
                        (float)(random.NextDouble() * 100),
                        (float)(random.NextDouble() * 1000)
                    )
                );
                
                await hubClient.UpdatePositionAsync(position);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }

        if (connectionId == 0)
        {
            await hubClient.StopBroadcast();
        }
    }

    public async Task CompleteAsync()
    {
        if (hubClient is not null)
        {
            await hubClient.LeaveAsync();
            await hubClient.DisposeAsync();
        }
    }

    public void OnBroadcastAllPositions(AllClientsPositionMessage message)
    {
        // Collect metrics for each broadcast received
        context.Increment();
    }
}

public class MetaverseBroadcast15FpsScenario : MetaverseBroadcastScenario
{
    protected override int TargetFps => 15;
}

public class MetaverseBroadcast30FpsScenario : MetaverseBroadcastScenario
{
    protected override int TargetFps => 30;
}

public class MetaverseBroadcast60FpsScenario : MetaverseBroadcastScenario
{
    protected override int TargetFps => 60;
}
