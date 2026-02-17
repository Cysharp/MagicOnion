using Cysharp.Runtime.Multicast;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class BroadcastGroupService(IMulticastGroupProvider groupProvider, TimeProvider timeProvider, DatadogMetricsRecorder datadogRecorder, ILogger<BroadcastGroupService> logger) : IDisposable
{
    readonly IMulticastSyncGroup<Guid, IPerTestBroadcastHubReceiver> group = groupProvider.GetOrAddSynchronousGroup<Guid, IPerTestBroadcastHubReceiver>("PerformanceTest");
    readonly ServerBroadcastMetricsContext metricsContext = new(timeProvider);
    int memberCount;

    public void SendMessageToAll(BroadcastPositionMessage response)
    {
        group.All.OnMessage(response);
        metricsContext.IncrementMessageCount();
    }

    public void AddMember(Guid id, IPerTestBroadcastHubReceiver receiver)
    {
        group.Add(id, receiver);
        var newCount = Interlocked.Increment(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
    }

    public void RemoveMember(Guid id)
    {
        group.Remove(id);
        var newCount = Interlocked.Decrement(ref memberCount);
        metricsContext.UpdateClientCount(newCount);
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
}
