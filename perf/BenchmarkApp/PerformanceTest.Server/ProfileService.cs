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
            var result = hardwarehardwarePerformanceReporter.GetResult();
            await datadog.PutServerBenchmarkMetricsAsync(ApplicationInformation.Current, result);
        }
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
        var tags = MetricsTagCache.Get((recorder.TagBranch, recorder.TagLegend, recorder.TagStreams, applicationInfo), static x => [
            $"legend:{x.TagLegend}{x.TagStreams}",
            $"branch:{x.TagBranch}",
            $"streams:{x.TagStreams}",
            $"process_arch:{x.applicationInfo.ProcessArchitecture}",
            $"process_count:{x.applicationInfo.ProcessorCount}",
        ]);

        // Don't want to await each put. Let's send it to queue and await when benchmark ends.
        recorder.Record(recorder.SendAsync("benchmark.magiconion.server.cpu_usage_max", result.MaxCpuUsage, DatadogMetricsType.Gauge, tags, "percent"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.server.cpu_usage_avg", result.MaxCpuUsage, DatadogMetricsType.Gauge, tags, "percent"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.server.memory_usage_max", result.MaxMemoryUsageMB, DatadogMetricsType.Gauge, tags, "megabyte"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.server.memory_usage_avg", result.AvgMemoryUsageMB, DatadogMetricsType.Gauge, tags, "megabyte"));

        // wait until send complete
        await recorder.WaitSaveAsync();
    }
}
