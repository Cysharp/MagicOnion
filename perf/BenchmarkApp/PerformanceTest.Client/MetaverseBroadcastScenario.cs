using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class MetaverseBroadcastScenario : IScenario, IMetaverseBroadcastHubReceiver
{
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
        var random = new Random(connectionId);
        
        // Join metaverse and start broadcast (only first client starts timer)
        await hubClient.JoinAsync(TargetFps);

        // Wait for warmup to complete
        await ctx.WaitForReadyAsync();
        
        // Simulate client position updates periodically
        var updateInterval = TimeSpan.FromSeconds(1.0 / 30); // 30Hz client updates
        using var updateTimer = new PeriodicTimer(updateInterval);
        
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
