using PerformanceTest.Shared.Reporting;
using System.Diagnostics;

public class PerformanceTestRunningContext
{
    int count;
    bool isRunning;
    Stopwatch stopwatch;
    HardwarePerformanceReporter hardwarePerformanceReporter;
    List<List<double>> latencyPerConnection;
    int errorsPerConnection;
    List<object> locks;

    public TimeSpan Timeout { get; }

    public PerformanceTestRunningContext(int connectionCount, (int WarmupSec, int RunSec) serverTimeout)
    {
        Timeout = TimeSpan.FromSeconds(serverTimeout.WarmupSec + serverTimeout.RunSec + 3); // add some sec for safely complete serverstreaming
        stopwatch = new Stopwatch();
        hardwarePerformanceReporter = new HardwarePerformanceReporter();
        latencyPerConnection = new(connectionCount);
        errorsPerConnection = 0;
        locks = new(connectionCount);
        for (var i = 0; i < connectionCount; i++)
        {
            latencyPerConnection.Add([]);
            locks.Add(new());
        }
    }

    public void Ready()
    {
        isRunning = true;
        stopwatch.Start();
        hardwarePerformanceReporter.Start();
    }

    public void Complete()
    {
        isRunning = false;
        stopwatch.Stop();
        hardwarePerformanceReporter.Stop();
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

    public PerformanceResult GetResult()
    {
        var latency = MeasureLatency(latencyPerConnection);
        Clear();
        return new PerformanceResult(count, count / (double)stopwatch.Elapsed.TotalSeconds, errorsPerConnection, stopwatch.Elapsed, latency, hardwarePerformanceReporter.GetResultAndClear());

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

public record PerformanceResult(int TotalRequests, double RequestsPerSecond, int errors, TimeSpan Duration, Latency Latency, HardwarePerformanceResult hardware);
public record Latency(double Mean, double P50, double P75, double P90, double P99, double Max);
