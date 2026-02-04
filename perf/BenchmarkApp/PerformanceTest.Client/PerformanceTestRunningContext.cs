using PerformanceTest.Shared.Reporting;
using System.Diagnostics;

public class PerformanceTestRunningContext
{
    int count;
    bool isRunning;
    Stopwatch stopwatch;
    List<List<double>> latencyPerConnection;
    int errorsPerConnection;
    List<Lock> locks;
    readonly TaskCompletionSource readyTcs = new();
    readonly ProfileService profileService;

    public TimeSpan Timeout { get; }
    public int DurationSeconds { get; }

    public PerformanceTestRunningContext(int connectionCount, (int WarmupSec, int RunSec) serverTimeout, DatadogMetricsRecorder recoder, ScenarioType scenario)
    {
        Timeout = TimeSpan.FromSeconds(serverTimeout.WarmupSec + serverTimeout.RunSec + 3); // add some sec for safely complete serverstreaming
        DurationSeconds = serverTimeout.RunSec;
        stopwatch = new Stopwatch();
        latencyPerConnection = new(connectionCount);
        errorsPerConnection = 0;
        locks = new(connectionCount);
        profileService = new ProfileService(TimeProvider.System, recoder, scenario);

        for (var i = 0; i < connectionCount; i++)
        {
            latencyPerConnection.Add([]);
            locks.Add(new());
        }
    }

    public Task WaitForReadyAsync() => readyTcs.Task;

    public void Ready()
    {
        isRunning = true;
        stopwatch.Start();
        profileService.Start();
        readyTcs.TrySetResult();
    }

    public async Task CompleteAsync()
    {
        isRunning = false;
        await profileService.StopAsync();
        stopwatch.Stop();
    }

    public void Increment()
    {
        if (isRunning)
        {
            Interlocked.Increment(ref count);
        }
    }

    public void Latency(int connectionId, TimeSpan duration)
    {
        lock (locks[connectionId])
        {
            latencyPerConnection[connectionId].Add(duration.TotalMilliseconds);
        }
    }

    public void LatencyThrottled(int connectionId, TimeSpan duration, int per)
    {
        if (count % per != 0) return;
        Latency(connectionId, duration);
    }

    public void Error()
    {
        Interlocked.Increment(ref errorsPerConnection);
    }

    public PerformanceResult GetResultAndClear()
    {
        var latency = MeasureLatency(latencyPerConnection);
        var hardware = profileService.GetResultAndClear();

        Clear();
        return new PerformanceResult(count, count / (double)stopwatch.Elapsed.TotalSeconds, errorsPerConnection, stopwatch.Elapsed, latency, hardware);

        static Latency MeasureLatency(List<List<double>> latencyPerConnection)
        {
            var totalCount = 0;
            var totalSum = 0.0;
            for (var i = 0; i < latencyPerConnection.Count; i++)
            {
                for (var j = 0; j < latencyPerConnection[i].Count; j++)
                {
                    totalSum += latencyPerConnection[i][j];
                    totalCount++;
                }

                latencyPerConnection[i].Sort();
            }
            var latencyMean = (totalCount != 0) ? totalSum / totalCount : totalSum;
            var latencyAllConnection = new List<double>(totalCount);
            foreach (var connections in latencyPerConnection) latencyAllConnection.AddRange(connections);
            // sort before get percentile
            latencyAllConnection.Sort();

            var latency50p = GetPercentile(50, latencyAllConnection);
            var latency75p = GetPercentile(75, latencyAllConnection);
            var latency90p = GetPercentile(90, latencyAllConnection);
            var latency99p = GetPercentile(99, latencyAllConnection);
            var latencyMax = GetPercentile(100, latencyAllConnection);
            var latency = new Latency(latencyMean, latency50p, latency75p, latency90p, latency99p, latencyMax);

            latencyAllConnection.Clear();
            return latency;
        }
        static double GetPercentile(int percent, IReadOnlyList<double> sortedData)
        {
            if (sortedData.Count == 0)
            {
                return 0.0;
            }
            if (percent == 100)
            {
                return sortedData[^1];
            }

            var i = ((long)percent * sortedData.Count) / 100.0 + 0.5;
            var fractionPart = i - Math.Truncate(i);

            return (1.0 - fractionPart) * sortedData[(int)Math.Truncate(i) - 1] + fractionPart * sortedData[(int)Math.Ceiling(i) - 1];
        }

        void Clear()
        {
            foreach (var list in latencyPerConnection)
            {
                list.Clear();
            }
            latencyPerConnection.Clear();
            locks.Clear();
        }
    }
}

public record PerformanceResult(int TotalRequests, double RequestsPerSecond, int Error, TimeSpan Duration, Latency Latency, HardwarePerformanceResult Hardware);
public record Latency(double Mean, double P50, double P75, double P90, double P99, double Max);
