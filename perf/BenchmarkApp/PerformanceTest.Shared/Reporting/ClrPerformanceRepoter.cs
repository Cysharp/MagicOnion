using System.Collections.Concurrent;

namespace PerformanceTest.Shared.Reporting;

public class ClrPerformanceReporter
{
    readonly TimeSpan samplingInterval;
    readonly TimeProvider timeProvider;
    readonly ConcurrentBag<double> heapSizes;
    readonly ConcurrentBag<double> committedMemories;
    CancellationTokenSource cancellationTokenSource;
    bool running;
    Lock @lock = new();
    long startAllocatedBytes;
    long endAllocatedBytes;
    long startTimestamp;
    long endTimestamp;

    public ClrPerformanceReporter() : this(TimeSpan.FromMilliseconds(100))
    { }

    public ClrPerformanceReporter(TimeSpan samplingInterval)
    {
        this.samplingInterval = samplingInterval;
        this.timeProvider = SystemTimeProvider.TimeProvider;
        cancellationTokenSource = new CancellationTokenSource();
        heapSizes = new ConcurrentBag<double>();
        committedMemories = new ConcurrentBag<double>();
    }

    public void Start()
    {
        running = true;
        startAllocatedBytes = GC.GetTotalAllocatedBytes();
        startTimestamp = timeProvider.GetTimestamp();

        Task.Run(async () =>
        {
            while (running)
            {
                if (cancellationTokenSource.IsCancellationRequested) break;

                // Current heap size (actual memory in use)
                heapSizes.Add(GC.GetTotalMemory(forceFullCollection: false));
                
                // Committed memory (OS-level memory commitment)
                try
                {
                    committedMemories.Add(GC.GetGCMemoryInfo().TotalCommittedBytes);
                }
                catch
                {
                    // GetGCMemoryInfo may fail before first GC
                }

                await Task.Delay(samplingInterval, timeProvider, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public void Stop()
    {
        running = false;
        endAllocatedBytes = GC.GetTotalAllocatedBytes();
        endTimestamp = timeProvider.GetTimestamp();
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    public ClrPerformanceResult GetResult()
    {
        if (heapSizes.Count == 0)
            return ClrPerformanceResult.Empty;

        var maxHeapSizeMB = heapSizes.Max() / 1024 / 1024;
        var avgHeapSizeMB = heapSizes.Average() / 1024 / 1024;
        var maxCommittedMemoryMB = committedMemories.Count > 0 ? committedMemories.Max() / 1024 / 1024 : 0;
        var avgCommittedMemoryMB = committedMemories.Count > 0 ? committedMemories.Average() / 1024 / 1024 : 0;

        // Calculate allocation rate
        var totalAllocatedBytes = endAllocatedBytes - startAllocatedBytes;
        var totalAllocatedMB = totalAllocatedBytes / 1024.0 / 1024.0;
        var elapsedSeconds = timeProvider.GetElapsedTime(startTimestamp, endTimestamp).TotalSeconds;
        var allocationRateMBPerSec = elapsedSeconds > 0 ? totalAllocatedMB / elapsedSeconds : 0;

        return new ClrPerformanceResult(maxHeapSizeMB, avgHeapSizeMB, maxCommittedMemoryMB, avgCommittedMemoryMB, totalAllocatedMB, allocationRateMBPerSec);
    }

    public ClrPerformanceResult GetResultAndClear()
    {
        lock (@lock)
        {
            var result = GetResult();
            heapSizes.Clear();
            committedMemories.Clear();

            return result;
        }
    }
}

public readonly record struct ClrPerformanceResult(
    double MaxHeapSizeMB, 
    double AvgHeapSizeMB, 
    double MaxCommittedMemoryMB, 
    double AvgCommittedMemoryMB,
    double TotalAllocatedMB,
    double AllocationRateMBPerSec)
{
    public static ClrPerformanceResult Empty => empty;
    private static readonly ClrPerformanceResult empty = new(0, 0, 0, 0, 0, 0);
}

/// <summary>
/// Aggregate CLR performance results across multiple rounds.
/// </summary>
public class ClrMetricsAggregator
{
    readonly ConcurrentBag<ClrPerformanceResult> allResults = [];

    /// <summary>
    /// Add a new round's CLR performance result to the aggregator.
    /// </summary>
    /// <param name="result"></param>
    public void AddResult(ClrPerformanceResult result)
    {
        allResults.Add(result);
    }

    /// <summary>
    /// Calculate and return the aggregated CLR performance result across all rounds.
    /// </summary>
    /// <returns></returns>
    public ClrPerformanceResult GetResult()
    {
        if (allResults.Count == 0)
            return new ClrPerformanceResult(0, 0, 0, 0, 0, 0);

        var maxHeapSize = allResults.Max(x => x.MaxHeapSizeMB);
        var avgHeapSize = allResults.Average(x => x.AvgHeapSizeMB);
        var maxCommittedMemory = allResults.Max(x => x.MaxCommittedMemoryMB);
        var avgCommittedMemory = allResults.Average(x => x.AvgCommittedMemoryMB);
        var totalAllocated = allResults.Sum(x => x.TotalAllocatedMB);
        var avgAllocationRate = allResults.Average(x => x.AllocationRateMBPerSec);

        return new ClrPerformanceResult(maxHeapSize, avgHeapSize, maxCommittedMemory, avgCommittedMemory, totalAllocated, avgAllocationRate);
    }

    public ClrPerformanceResult GetResultAndClear()
    {
        var result = GetResult();
        allResults.Clear();

        return result;
    }
}
