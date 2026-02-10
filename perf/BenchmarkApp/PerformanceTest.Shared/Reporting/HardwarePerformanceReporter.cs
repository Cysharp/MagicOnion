using System.Collections.Concurrent;
using System.Diagnostics;

namespace PerformanceTest.Shared.Reporting;

public class HardwarePerformanceReporter
{
    private readonly TimeSpan samplingInterval;
    private readonly TimeProvider timeProvider;
    private readonly Process currentProcess;
    private readonly int cpuCores;
    private readonly ConcurrentBag<double> cpuUsages;
    private readonly ConcurrentBag<double> memoryUsages;
    private CancellationTokenSource cancellationTokenSource;
    private bool running;
    private Lock @lock = new();

    public HardwarePerformanceReporter() : this(TimeSpan.FromMilliseconds(100))
    { }

    public HardwarePerformanceReporter(TimeSpan samplingInterval)
    {
        this.samplingInterval = samplingInterval;
        this.timeProvider = SystemTimeProvider.TimeProvider;
        currentProcess = Process.GetCurrentProcess();
        cpuCores = ApplicationInformation.Current.ProcessorCount;
        cpuUsages = new ConcurrentBag<double>();
        memoryUsages = new ConcurrentBag<double>();
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start()
    {
        running = true;

        Task.Run(async () =>
        {
            while (running)
            {
                // begin
                var start = timeProvider.GetTimestamp();
                TimeSpan startCpuTime = currentProcess.TotalProcessorTime;
                await Task.Delay(samplingInterval, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                if (cancellationTokenSource.IsCancellationRequested) break;

                // end
                TimeSpan endCpuTime = currentProcess.TotalProcessorTime;
                var duration = timeProvider.GetElapsedTime(start);

                // CPU usage
                var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
                var totalMsPassed = duration.TotalMilliseconds;
                var cpuUsagePercentage = (cpuUsedMs / totalMsPassed) * 100 / cpuCores;
                cpuUsages.Add(cpuUsagePercentage);

                // Memory usage = working set (Don't use Process.WorkingSet64 because it may be cached)
                var currentMemory = Environment.WorkingSet;
                memoryUsages.Add(currentMemory);
            }
        }, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public void Stop()
    {
        running = false;
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    public HardwarePerformanceResult GetResult()
    {
        if (cpuUsages.Count == 0)
            return HardwarePerformanceResult.Empty;

        Span<double> cpuData = cpuUsages.ToArray();
        var cpuRange = OutlierIqr.FindInlierRange(cpuData, 100.0);
        var filteredCpuData = cpuData[cpuRange];

        var maxCpuUsage = GetMax(filteredCpuData);
        var avgCpuUsage = GetAverage(filteredCpuData);
        var maxMemoryUsage = memoryUsages.Max() / 1024 / 1024;
        var avgMemoryUsage = memoryUsages.Average() / 1024 / 1024;

        return new HardwarePerformanceResult(maxCpuUsage, avgCpuUsage, maxMemoryUsage, avgMemoryUsage);
    }

    public HardwarePerformanceResult GetResultAndClear()
    {
        lock (@lock)
        {
            var result = GetResult();
            cpuUsages.Clear();
            memoryUsages.Clear();

            return result;
        }
    }

    private static double GetMax(ReadOnlySpan<double> data)
    {
        var max = double.MinValue;
        foreach (var value in data)
        {
            if (value > max) max = value;
        }
        return max;
    }

    private static double GetAverage(ReadOnlySpan<double> data)
    {
        if (data.Length == 0) return 0;
        var sum = 0.0;
        foreach (var value in data)
        {
            sum += value;
        }
        return sum / data.Length;
    }
}

public readonly record struct HardwarePerformanceResult(double MaxCpuUsagePercent, double AvgCpuUsagePercent, double MaxMemoryUsageMB, double AvgMemoryUsageMB)
{
    public static HardwarePerformanceResult Empty => empty;
    private static readonly HardwarePerformanceResult empty = new(0, 0, 0, 0);
}

/// <summary>
/// Aggregate hardware performance results across multiple rounds.
/// </summary>
public class HardwareMetricsAggregator
{
    readonly ConcurrentBag<HardwarePerformanceResult> allResults = [];

    /// <summary>
    /// Add a new round's hardware performance result to the aggregator.
    /// </summary>
    /// <param name="result"></param>
    public void AddResult(HardwarePerformanceResult result)
    {
        allResults.Add(result);
    }

    /// <summary>
    /// Calculate and return the aggregated hardware performance result across all rounds, including max and average CPU and memory usage.
    /// </summary>
    /// <returns></returns>
    public HardwarePerformanceResult GetResult()
    {
        if (allResults.Count == 0)
            return new HardwarePerformanceResult(0, 0, 0, 0);

        var maxCpu = allResults.Max(x => x.MaxCpuUsagePercent);
        var avgCpu = allResults.Average(x => x.AvgCpuUsagePercent); // Average of rounds averages. Not perfect but good enough for now.
        var maxMemory = allResults.Max(x => x.MaxMemoryUsageMB);
        var avgMemory = allResults.Average(x => x.AvgMemoryUsageMB);

        return new HardwarePerformanceResult(maxCpu, avgCpu, maxMemory, avgMemory);
    }

    public HardwarePerformanceResult GetResultAndClear()
    {
        var result = GetResult();
        allResults.Clear();

        return result;
    }
}
