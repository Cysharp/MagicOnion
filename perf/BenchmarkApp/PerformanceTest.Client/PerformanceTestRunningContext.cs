using System.Diagnostics;

public class PerformanceTestRunningContext
{
    int count;
    bool isRunning;
    Stopwatch stopwatch;

    public PerformanceTestRunningContext()
    {
        stopwatch = new Stopwatch();
    }

    public void Ready()
    {
        isRunning = true;
        stopwatch.Start();
    }

    public void Increment()
    {
        if (isRunning)
        {
            Interlocked.Increment(ref count);
        }
    }

    public void Complete()
    {
        isRunning = false;
        stopwatch.Stop();
    }

    public PerformanceResult GetResult()
    {
        return new PerformanceResult(count, count / (double)stopwatch.Elapsed.TotalSeconds, stopwatch.Elapsed);
    }
}

public record PerformanceResult(int TotalRequests, double RequestsPerSecond, TimeSpan Duration);
