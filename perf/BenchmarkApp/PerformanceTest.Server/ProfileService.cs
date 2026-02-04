using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class ProfileService : BackgroundService
{
    readonly DatadogMetricsRecorder datadog;
    readonly HardwarePerformanceReporter hardwarehardwarePerformanceReporter;
    readonly PeriodicTimer timer;

    public ProfileService(TimeProvider timeProvider, DatadogMetricsRecorder datadogRecorder)
    {
        datadog = datadogRecorder;
        hardwarehardwarePerformanceReporter = new HardwarePerformanceReporter();
        timer = new PeriodicTimer(TimeSpan.FromSeconds(10), timeProvider);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        hardwarehardwarePerformanceReporter.Start();
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var result = hardwarehardwarePerformanceReporter.GetResultAndClear();
            await datadog.PutServerHardwareMetricsAsync(ApplicationInformation.Current, result);
        }

        hardwarehardwarePerformanceReporter.Stop();
    }
}

static class DatadogMetricsRecorderExtensions
{
    /// <summary>
    /// Put Server Hardware metrics to background. 
    /// </summary>
    /// <param name="recorder"></param>
    /// <param name="applicationInfo"></param>
    /// <param name="result"></param>
    public static async Task PutServerHardwareMetricsAsync(this DatadogMetricsRecorder recorder, ApplicationInformation applicationInfo, HardwarePerformanceResult result)
    {
        if (string.IsNullOrEmpty(recorder.TagMagicOnion))
            return;

        Post(recorder, applicationInfo, result, false);
        if (DatadogMetricsRecorder.EnableLatestTag)
        {
            Post(recorder, applicationInfo, result, true);
        }

        // wait until send complete
        await recorder.WaitSaveAsync();

        static void Post(DatadogMetricsRecorder recorder, ApplicationInformation applicationInfo, HardwarePerformanceResult result, bool isMagicOnionLatest)
        {
            var magicOnionTag = isMagicOnionLatest ? recorder.TagLatestMagicOnion : recorder.TagMagicOnion;
            var tags = MetricsTagCache.Get((recorder.TagBranch, recorder.TagLegend, recorder.TagStreams, recorder.TagProtocol, recorder.TagSerialization, magicOnionTag, applicationInfo), static x => [
                $"legend:{x.TagLegend}{x.TagStreams}",
                $"branch:{x.TagBranch}",
                $"magiconion:{x.magicOnionTag}",
                $"protocol:{x.TagProtocol}",
                $"process_arch:{x.applicationInfo.ProcessArchitecture}",
                $"process_count:{x.applicationInfo.ProcessorCount}",
                $"serialization:{x.TagSerialization}",
                $"streams:{x.TagStreams}",
            ]);

            // Don't want to await each put. Let's send it to queue and await when benchmark ends.
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.cpu_usage_max", result.MaxCpuUsagePercent, DatadogMetricsType.Gauge, tags, "percent"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.cpu_usage_avg", result.AvgCpuUsagePercent, DatadogMetricsType.Gauge, tags, "percent"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.memory_usage_max", result.MaxMemoryUsageMB, DatadogMetricsType.Gauge, tags, "megabyte"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.memory_usage_avg", result.AvgMemoryUsageMB, DatadogMetricsType.Gauge, tags, "megabyte"));
        }
    }

    /// <summary>
    /// Put Server Broadcast metrics to background. 
    /// </summary>
    public static async Task PutServerBroadcastMetricsAsync(this DatadogMetricsRecorder recorder, ApplicationInformation applicationInfo, ServerBroadcastMetricsResult result)
    {
        if (string.IsNullOrEmpty(recorder.TagMagicOnion))
            return;

        Post(recorder, applicationInfo, result, false);
        if (DatadogMetricsRecorder.EnableLatestTag)
        {
            Post(recorder, applicationInfo, result, true);
        }

        // wait until send complete
        await recorder.WaitSaveAsync();

        static void Post(DatadogMetricsRecorder recorder, ApplicationInformation applicationInfo, ServerBroadcastMetricsResult result, bool isMagicOnionLatest)
        {
            var magicOnionTag = isMagicOnionLatest ? recorder.TagLatestMagicOnion : recorder.TagMagicOnion;
            var tags = MetricsTagCache.Get((recorder.TagBranch, recorder.TagLegend, recorder.TagStreams, recorder.TagProtocol, recorder.TagSerialization, magicOnionTag, applicationInfo, result.TargetFps), static x => [
                $"legend:{x.TagLegend}{x.TagStreams}",
                $"branch:{x.TagBranch}",
                $"magiconion:{x.magicOnionTag}",
                $"protocol:{x.TagProtocol}",
                $"process_arch:{x.applicationInfo.ProcessArchitecture}",
                $"process_count:{x.applicationInfo.ProcessorCount}",
                $"serialization:{x.TagSerialization}",
                $"streams:{x.TagStreams}",
                $"fps:{x.TargetFps}",
            ]);          

            // Send broadcast-specific metrics
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.broadcast.total_messages", result.TotalMessages, DatadogMetricsType.Gauge, tags, "message"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.broadcast.total_messages_sent", result.TotalMessagesSent, DatadogMetricsType.Gauge, tags, "message"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.server.broadcast.client_count", result.AvgClientCount, DatadogMetricsType.Gauge, tags, "count"));
        }
    }
}
