using System.Collections.Concurrent;

namespace PerformanceTest.Shared.Reporting;

public class ClrPerformanceReporter
{
    readonly TimeSpan samplingInterval;
    readonly TimeProvider timeProvider;

    readonly ConcurrentBag<double> heapSizes;
    readonly ConcurrentBag<double> committedMemories;
    readonly ConcurrentBag<int> gcCountGen0Deltas;
    readonly ConcurrentBag<int> gcCountGen1Deltas;
    readonly ConcurrentBag<int> gcCountGen2Deltas;

    CancellationTokenSource cancellationTokenSource;
    bool running;
    Lock @lock = new();
    long startTimestamp;
    long endTimestamp;
    int prevGen0Count;
    int prevGen1Count;
    int prevGen2Count;

    public ClrPerformanceReporter() : this(TimeSpan.FromMilliseconds(100))
    { }

    public ClrPerformanceReporter(TimeSpan samplingInterval)
    {
        this.samplingInterval = samplingInterval;
        this.timeProvider = SystemTimeProvider.TimeProvider;
        cancellationTokenSource = new CancellationTokenSource();
        heapSizes = new ConcurrentBag<double>();
        committedMemories = new ConcurrentBag<double>();
        gcCountGen0Deltas = new ConcurrentBag<int>();
        gcCountGen1Deltas = new ConcurrentBag<int>();
        gcCountGen2Deltas = new ConcurrentBag<int>();
    }

    public void Start()
    {
        running = true;
        startTimestamp = timeProvider.GetTimestamp();
        prevGen0Count = GC.CollectionCount(0);
        prevGen1Count = GC.CollectionCount(1);
        prevGen2Count = GC.CollectionCount(2);

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

                // Calculate GC count deltas since last sample
                var currentGen0 = GC.CollectionCount(0);
                var currentGen1 = GC.CollectionCount(1);
                var currentGen2 = GC.CollectionCount(2);

                gcCountGen0Deltas.Add(currentGen0 - prevGen0Count);
                gcCountGen1Deltas.Add(currentGen1 - prevGen1Count);
                gcCountGen2Deltas.Add(currentGen2 - prevGen2Count);

                prevGen0Count = currentGen0;
                prevGen1Count = currentGen1;
                prevGen2Count = currentGen2;

                await Task.Delay(samplingInterval, timeProvider, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public void Stop()
    {
        running = false;
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

        // GC count statistics from deltas
        var totalGen0 = gcCountGen0Deltas.Sum();
        var avgGen0 = gcCountGen0Deltas.Count > 0 ? (int)gcCountGen0Deltas.Average() : 0;
        var totalGen1 = gcCountGen1Deltas.Sum();
        var avgGen1 = gcCountGen1Deltas.Count > 0 ? (int)gcCountGen1Deltas.Average() : 0;
        var totalGen2 = gcCountGen2Deltas.Sum();
        var avgGen2 = gcCountGen2Deltas.Count > 0 ? (int)gcCountGen2Deltas.Average() : 0;

        return new ClrPerformanceResult(
            maxHeapSizeMB, 
            avgHeapSizeMB, 
            maxCommittedMemoryMB, 
            avgCommittedMemoryMB, 
            totalGen0, 
            avgGen0, 
            totalGen1, 
            avgGen1, 
            totalGen2, 
            avgGen2);
    }

    public ClrPerformanceResult GetResultAndClear()
    {
        lock (@lock)
        {
            var result = GetResult();
            heapSizes.Clear();
            committedMemories.Clear();
            gcCountGen0Deltas.Clear();
            gcCountGen1Deltas.Clear();
            gcCountGen2Deltas.Clear();

            return result;
        }
    }
}

public readonly record struct ClrPerformanceResult(
    double MaxHeapSizeMB,
    double AvgHeapSizeMB,
    double MaxCommittedMemoryMB,
    double AvgCommittedMemoryMB,
    int TotalGcCountGen0,
    int AvgGcCountGen0,
    int TotalGcCountGen1,
    int AvgGcCountGen1,
    int TotalGcCountGen2,
    int AvgGcCountGen2)
{
    public static ClrPerformanceResult Empty => empty;
    private static readonly ClrPerformanceResult empty = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
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
            return new ClrPerformanceResult(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        var maxHeapSize = allResults.Max(x => x.MaxHeapSizeMB);
        var avgHeapSize = allResults.Average(x => x.AvgHeapSizeMB);
        var maxCommittedMemory = allResults.Max(x => x.MaxCommittedMemoryMB);
        var avgCommittedMemory = allResults.Average(x => x.AvgCommittedMemoryMB);
        var totalGcCountGen0 = allResults.Sum(x => x.TotalGcCountGen0);
        var avgGcCountGen0 = allResults.Average(x => x.AvgGcCountGen0);
        var totalGcCountGen1 = allResults.Sum(x => x.TotalGcCountGen1);
        var avgGcCountGen1 = allResults.Average(x => x.AvgGcCountGen1);
        var totalGcCountGen2 = allResults.Sum(x => x.TotalGcCountGen2);
        var avgGcCountGen2 = allResults.Average(x => x.AvgGcCountGen2);

        return new ClrPerformanceResult(maxHeapSize, avgHeapSize, maxCommittedMemory, avgCommittedMemory, totalGcCountGen0, (int)avgGcCountGen0, totalGcCountGen1, (int)avgGcCountGen1, totalGcCountGen2, (int)avgGcCountGen2);
    }

    public ClrPerformanceResult GetResultAndClear()
    {
        var result = GetResult();
        allResults.Clear();

        return result;
    }
}
