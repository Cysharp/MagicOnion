using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class MetaverseGroupService(IMulticastGroupProvider groupProvider, TimeProvider timeProvider, MetaverseWorld metaverseWorld, DatadogMetricsRecorder datadogRecorder, ILogger<BroadcastGroupService> logger) : IDisposable
{
    readonly IMulticastSyncGroup<Guid, IMetaverseBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IMetaverseBroadcastHubReceiver>("MetaverseBroadcast");
    readonly ServerBroadcastMetricsContext metricsContext = new(timeProvider);
    int memberCount;

    // double buffering for broadcast messages to avoid allocating new array every time. We will toggle between buffer0 and buffer1 for each broadcast. (make sure complete writing to buffer within 2 frames)
    BroadcastPositionMessage[] buffer0 = [];
    BroadcastPositionMessage[] buffer1 = [];
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
        metaverseWorld.GetAllClientPositions(buffer.AsSpan());
        var frameNumber = metaverseWorld.CurrentFrame;
        var message = new AllClientsPositionMessage(frameNumber, buffer);

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
        // Toggle between buffer0 and buffer1
        var useBuffer0 = Interlocked.Increment(ref currentBufferIndex) % 2 == 0;
        ref var buffer = ref useBuffer0 ? ref buffer0 : ref buffer1;

        if (buffer.Length != capacity)
        {
            buffer = new BroadcastPositionMessage[capacity];
        }

        return buffer;
    }
}
