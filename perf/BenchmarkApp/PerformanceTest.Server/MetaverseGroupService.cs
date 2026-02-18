using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class MetaverseGroupService(IMulticastGroupProvider groupProvider, TimeProvider timeProvider, MetaverseWorld metaverseWorld, DatadogMetricsRecorder datadogRecorder, ILogger<BroadcastGroupService> logger) : IDisposable
{
    readonly IMulticastSyncGroup<Guid, IMetaverseBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IMetaverseBroadcastHubReceiver>("MetaverseBroadcast");
    readonly ServerBroadcastMetricsContext metricsContext = new(timeProvider);
    int memberCount;

    // Circular buffer pool for broadcast messages to avoid allocating new array every time.
    // Using 8 buffers to handle slow serialization/broadcast scenarios (e.g., 2000 clients at 15fps)
    BroadcastPositionMessage[][] bufferPool = new BroadcastPositionMessage[8][];
    int currentBufferIndex;

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

    public void BroadcastAllPositions()
    {
        var buffer = GetNextBuffer(memberCount);
        metaverseWorld.WriteAllClientPositions(buffer.AsSpan());
        var message = new AllClientsPositionMessage(metaverseWorld.CurrentFrame, buffer);

        // Broadcast to all clients
        group.All.OnBroadcastAllPositions(message);
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
        var result = metricsContext.GetResult();
        metricsContext.Reset();

        // Send metrics to Datadog
        await datadogRecorder.PutServerBroadcastMetricsAsync(ApplicationInformation.Current, result);

        logger.LogInformation("Scenario: {scenario}, BroadCast Metrics: {@MetricsResult}", DatadogMetricsRecorder.Scenario, result);
    }

    public void Dispose() => group.Dispose();

    BroadcastPositionMessage[] GetNextBuffer(int capacity)
    {
        // Get next buffer from circular pool
        var index = Interlocked.Increment(ref currentBufferIndex) % bufferPool.Length;
        ref var buffer = ref bufferPool[index];

        if (buffer is null || buffer.Length != capacity)
        {
            buffer = new BroadcastPositionMessage[capacity];
        }

        return buffer;
    }
}
