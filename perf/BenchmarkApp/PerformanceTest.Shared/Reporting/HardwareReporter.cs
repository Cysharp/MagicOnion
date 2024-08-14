using System.Diagnostics;

namespace PerformanceTest.Shared.Reporting;

public class HardwareReporter
{
    private readonly TimeSpan samplingInterval;
    private readonly TimeProvider timeProvider;
    private readonly Process currentProcess;
    private readonly List<double> cpuUsages;
    private readonly List<double> memoryUsages;
    private CancellationTokenSource cancellationTokenSource;
    private bool running;

    public HardwareReporter() :this(TimeSpan.FromMilliseconds(100))
    { }

    public HardwareReporter(TimeSpan samplingInterval)
    {
        this.samplingInterval = samplingInterval;
        this.timeProvider = SystemTimeProvider.TimeProvider;
        currentProcess = Process.GetCurrentProcess();
        cpuUsages = [];
        memoryUsages = [];
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start()
    {
        running = true;
        Task.Run(async () =>
        {
            var start = timeProvider.GetTimestamp();
            while (running)
            {
                // get most recent CPU Time
                TimeSpan startCpuTime = currentProcess.TotalProcessorTime;
                await Task.Delay(samplingInterval, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                if (cancellationTokenSource.IsCancellationRequested) break;

                // get current CPU time
                TimeSpan endCpuTime = currentProcess.TotalProcessorTime;

                // CPU usage
                double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
                double cpuUsage = (cpuUsedMs / (Environment.ProcessorCount * samplingInterval.TotalMilliseconds * 10)) * 100;

                // Memory usage (working set)
                long workingSet = currentProcess.WorkingSet64;

                cpuUsages.Add(cpuUsage);
                memoryUsages.Add(workingSet);
            }
        }, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public void Stop()
    {
        running = false;
        cancellationTokenSource.Cancel();
    }

    public HardwareResult GetResult()
    {
        var maxCpuUsage = cpuUsages.Count > 0 ? cpuUsages.Max() : 0d;
        var avgCpuUsage = cpuUsages.Count > 0 ? cpuUsages.Average() : 0d;
        var maxMemoryUsage = memoryUsages.Count > 0 ? memoryUsages.Max() / 1024 / 1024: 0d;
        var avgMemoryUsage = memoryUsages.Count > 0 ? memoryUsages.Average() / 1024 / 1024: 0d;
        cpuUsages.Clear();
        memoryUsages.Clear();

        return new HardwareResult(maxCpuUsage, avgCpuUsage, maxMemoryUsage, avgMemoryUsage);
    }
}

public record HardwareResult(double MaxCpuUsage, double AvgCpuUsage,double MaxMemoryUsageMB, double AvgMemoryUsageMB);
