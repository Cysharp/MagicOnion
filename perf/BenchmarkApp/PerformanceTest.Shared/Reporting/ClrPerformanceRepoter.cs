using System.Collections.Concurrent;

namespace PerformanceTest.Shared.Reporting;

public class ClrPerformanceReporter
{
    readonly TimeSpan samplingInterval;
    readonly TimeProvider timeProvider;
    readonly ConcurrentBag<double> heapSizes;
    readonly ConcurrentBag<double> committedMemories;

    const int numberOfGenerations = 3; // gen0, gen1, gen2 (no loh, poh)
    static readonly string[] genNames = ["gen0", "gen1", "gen2"];
    readonly ConcurrentBag<long> gcCountGen0;
    readonly ConcurrentBag<long> gcCountGen1;
    readonly ConcurrentBag<long> gcCountGen2;

    CancellationTokenSource cancellationTokenSource;
    bool running;
    Lock @lock = new();
    long startTimestamp;
    long endTimestamp;

    public ClrPerformanceReporter() : this(TimeSpan.FromMilliseconds(100))
    { }

    public ClrPerformanceReporter(TimeSpan samplingInterval)
    {
        this.samplingInterval = samplingInterval;
        this.timeProvider = SystemTimeProvider.TimeProvider;
        cancellationTokenSource = new CancellationTokenSource();
        heapSizes = [];
        committedMemories = [];
        gcCountGen0 = [];
        gcCountGen1 = [];
        gcCountGen2 = [];
    }

    public void Start()
    {
        running = true;
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

                // GC Counts for each generation
                foreach (var (genName, count) in GetGarbageCollectionCounts())
                {
                    switch (genName)
                    {
                        case "gen0":
                            gcCountGen0.Add(count);
                            break;
                        case "gen1":
                            gcCountGen1.Add(count);
                            break;
                        case "gen2":
                            gcCountGen2.Add(count);
                            break;
                    }
                }

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
        var totalGCGen0 = gcCountGen0.Sum();
        var avgGCGen0 = gcCountGen0.Average();
        var totalGCGen1 = gcCountGen1.Sum();
        var avgGCGen1 = gcCountGen1.Average();
        var totalGCGen2 = gcCountGen2.Sum();
        var avgGCGen2 = gcCountGen2.Average();

        // Calculate allocation rate
        var elapsedSeconds = timeProvider.GetElapsedTime(startTimestamp, endTimestamp).TotalSeconds;

        return new ClrPerformanceResult(maxHeapSizeMB, avgHeapSizeMB, maxCommittedMemoryMB, avgCommittedMemoryMB, totalGCGen0, (int)avgGCGen0, totalGCGen1, (int)avgGCGen1, totalGCGen2, (int)avgGCGen2);
    }

    public ClrPerformanceResult GetResultAndClear()
    {
        lock (@lock)
        {
            var result = GetResult();
            heapSizes.Clear();
            committedMemories.Clear();
            gcCountGen0.Clear();
            gcCountGen1.Clear();
            gcCountGen2.Clear();

            return result;
        }
    }

    private static IEnumerable<(string, long)> GetGarbageCollectionCounts()
    {
        long collectionsFromHigherGeneration = 0;

        for (var gen = numberOfGenerations - 1; gen >= 0; --gen)
        {
            long collectionsFromThisGeneration = GC.CollectionCount(gen);

            yield return new(genNames[gen], collectionsFromThisGeneration - collectionsFromHigherGeneration);

            collectionsFromHigherGeneration = collectionsFromThisGeneration;
        }
    }
}

public readonly record struct ClrPerformanceResult(
    double MaxHeapSizeMB,
    double AvgHeapSizeMB,
    double MaxCommittedMemoryMB,
    double AvgCommittedMemoryMB,
    long TotalGcCountGen0,
    long AvgGcCountGen0,
    long TotalGcCountGen1,
    long AvgGcCountGen1,
    long TotalGcCountGen2,
    long AvgGcCountGen2)
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
