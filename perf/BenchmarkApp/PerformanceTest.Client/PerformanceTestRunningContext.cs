using PerformanceTest.Shared.Reporting;
using System.Diagnostics;

public class PerformanceTestRunningContext
{
    int count;
    bool isRunning;
    Stopwatch stopwatch;
    HardwareReporter hardwareReporter;
    List<List<double>> latencyPerConnection;
    List<object> locks;

    public PerformanceTestRunningContext(int connectionCount)
    {
        stopwatch = new Stopwatch();
        hardwareReporter = new HardwareReporter();
        latencyPerConnection = new (connectionCount);
        locks = new (connectionCount);
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
        hardwareReporter.Start();
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

    public void Complete()
    {
        isRunning = false;
        stopwatch.Stop();
        hardwareReporter.Stop();
    }

    public PerformanceResult GetResult()
    {
        var latency = MeasureLatency();
        latencyPerConnection.Clear();
        locks.Clear();
        return new PerformanceResult(count, count / (double)stopwatch.Elapsed.TotalSeconds, stopwatch.Elapsed, latency, hardwareReporter.GetResult());

        Latency MeasureLatency()
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
    }
}

public record PerformanceResult(int TotalRequests, double RequestsPerSecond, TimeSpan Duration, Latency Latency, HardwareResult hardware);
public record Latency(double Mean, double P50, double P75, double P90, double P99, double Max);
