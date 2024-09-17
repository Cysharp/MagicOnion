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
        var prevMemory = currentProcess.WorkingSet64;
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

                // Memory usage = working set
                var currentMemory = currentProcess.WorkingSet64;
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
        var filteredCpuUsages = OutlinerHelper.RemoveOutlinerByIQR(cpuUsages.ToArray(), 100.0);
        var maxCpuUsage = cpuUsages.Count > 0 ? filteredCpuUsages.Max() : 0d;
        var avgCpuUsage = cpuUsages.Count > 0 ? filteredCpuUsages.Average() : 0d;
        var maxMemoryUsage = memoryUsages.Count > 0 ? memoryUsages.Max() / 1024 / 1024 : 0d;
        var avgMemoryUsage = memoryUsages.Count > 0 ? memoryUsages.Average() / 1024 / 1024 : 0d;

        return new HardwarePerformanceResult(maxCpuUsage, avgCpuUsage, maxMemoryUsage, avgMemoryUsage);
    }

    public HardwarePerformanceResult GetResultAndClear()
    {
        var result = GetResult();
        cpuUsages.Clear();
        memoryUsages.Clear();

        return result;
    }
}

public record HardwarePerformanceResult(double MaxCpuUsagePercent, double AvgCpuUsagePercent,double MaxMemoryUsageMB, double AvgMemoryUsageMB);
