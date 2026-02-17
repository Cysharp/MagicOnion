using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class MetaverseGroupService(IMulticastGroupProvider groupProvider, TimeProvider timeProvider, MetaverseWorld metaverseWorld, DatadogMetricsRecorder datadogRecorder, ILogger<BroadcastGroupService> logger) : IDisposable
{
    readonly IMulticastSyncGroup<Guid, IMetaverseBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IMetaverseBroadcastHubReceiver>("MetaverseBroadcast");
    readonly ServerBroadcastMetricsContext metricsContext = new(timeProvider);
    int memberCount;

    public ServerBroadcastMetricsContext MetricsContext => metricsContext;
    public int MemberCount => memberCount;

    public void AddMember(Guid id, IMetaverseBroadcastHubReceiver receiver)
    {
        group.Add(id, receiver);
        metaverseWorld.AddClient(id);
        var newCount = Interlocked.Increment(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
    }

    public void RemoveMember(Guid id)
    {
        group.Remove(id);
        metaverseWorld.RemoveClient(id);
        var newCount = Interlocked.Decrement(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
    }

    public void UpdatePosition(Guid clientId, BroadcastPositionMessage position)
    {
        metaverseWorld.UpdateClientPosition(clientId, position.Position);
    }

    public void BroadcastAllPositions(Guid id)
    {
        var positions = metaverseWorld.GetAllClientPositions();
        var frameNumber = metaverseWorld.CurrentFrame;
        var message = new AllClientsPositionMessage(frameNumber, positions);

        // Broadcast to all clients except the sender
        group.Except(id).OnBroadcastAllPositions(message);
        metricsContext.IncrementMessageCount();
    }

    public void StartMetricsCollection(int targetFps)
    {
        metricsContext.Start(targetFps);
        // Record initial client count
        metricsContext.UpdateClientCount(memberCount);
        // NOTE: run periodically send metrics to Datadog every 10 seconds if needed. For simplicity, we will send metrics only at the end of the test currently.
    }

    public void StopMetricsCollection()
    {
        metricsContext.Stop();
    }

    public async ValueTask SendAndClearMetricsAsync()
    {
        // Stop metrics collection and get result
        var result = metricsContext.GetResult(true);
        metricsContext.Reset();

        // Send metrics to Datadog
        await datadogRecorder.PutServerBroadcastMetricsAsync(ApplicationInformation.Current, result);

        logger.LogInformation("Scenario: {scenario}, BroadCast Metrics: {@MetricsResult}", DatadogMetricsRecorder.Scenario, result);
    }

    public void Dispose() => group.Dispose();
}
