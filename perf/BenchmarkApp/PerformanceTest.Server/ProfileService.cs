using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

class ProfileService : BackgroundService
{
    private DatadogMetricsRecorder datadog;
    private readonly HardwarePerformanceReporter hardwarehardwarePerformanceReporter;
    private readonly PeriodicTimer timer;

    public ProfileService(TimeProvider timeProvider, IConfiguration configuration)
    {
        var tagString = configuration.GetValue<string>("Tags") ?? "";
        var validate = configuration.GetValue<bool?>("Validate") ?? false;
        datadog = DatadogMetricsRecorder.Create(tagString, validate);
        hardwarehardwarePerformanceReporter = new HardwarePerformanceReporter();
        timer = new PeriodicTimer(TimeSpan.FromSeconds(10), timeProvider);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        hardwarehardwarePerformanceReporter.Start();
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var result = hardwarehardwarePerformanceReporter.GetResultAndClear();
            await datadog.PutServerBenchmarkMetricsAsync(ApplicationInformation.Current, result);
        }

        hardwarehardwarePerformanceReporter.Stop();
    }
}

static class DatadogMetricsRecorderExtensions
{
    /// <summary>
    /// Put Server Benchmark metrics to background. 
    /// </summary>
    /// <param name="recorder"></param>
    /// <param name="applicationInfo"></param>
    /// <param name="result"></param>
    public static async Task PutServerBenchmarkMetricsAsync(this DatadogMetricsRecorder recorder, ApplicationInformation applicationInfo, HardwarePerformanceResult result)
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
}
