namespace PerformanceTest.Server;

public class ServerBroadcastMetricsContext
{
    readonly TimeProvider timeProvider;
    readonly Lock clientCountLock = new();
    long messageCount;
    bool isRunning;
    long startTimestamp;
    TimeSpan elapsed;
    int targetFps;
    int clientCountAtStart;
    int clientCountAtEnd;
    int minClientCount;
    int maxClientCount;
    long totalClientCountSamples;
    long clientCountSampleCount;

    public ServerBroadcastMetricsContext(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public void Start(int targetFps)
    {
        isRunning = true;
        this.targetFps = targetFps;
        Interlocked.Exchange(ref messageCount, 0);
        startTimestamp = timeProvider.GetTimestamp();
        
        lock (clientCountLock)
        {
            clientCountAtStart = 0;
            clientCountAtEnd = 0;
            minClientCount = int.MaxValue;
            maxClientCount = 0;
            totalClientCountSamples = 0;
            clientCountSampleCount = 0;
        }
    }

    public void Stop()
    {
        isRunning = false;
        elapsed = timeProvider.GetElapsedTime(startTimestamp);
    }

    public void IncrementMessageCount()
    {
        if (isRunning)
        {
            Interlocked.Increment(ref messageCount);
        }
    }

    public void UpdateClientCount(int count)
    {
        if (!isRunning) return;

        lock (clientCountLock)
        {
            // Update start count if this is the first update
            if (clientCountSampleCount == 0)
            {
                clientCountAtStart = count;
            }

            clientCountAtEnd = count;
            
            if (count < minClientCount)
            {
                minClientCount = count;
            }

            if (count > maxClientCount)
            {
                maxClientCount = count;
            }

            totalClientCountSamples += count;
            clientCountSampleCount++;
        }
    }

    public ServerBroadcastMetricsResult GetCurrentResult()
    {
        var count = Interlocked.Read(ref messageCount);
        var currentElapsed = isRunning ? timeProvider.GetElapsedTime(startTimestamp) : elapsed;
        var actualFps = currentElapsed.TotalSeconds > 0 ? count / currentElapsed.TotalSeconds : 0;

        lock (clientCountLock)
        {
            var avgClientCount = clientCountSampleCount > 0 ? (double)totalClientCountSamples / clientCountSampleCount : 0;
            var finalMinClientCount = minClientCount == int.MaxValue ? 0 : minClientCount;

            return new ServerBroadcastMetricsResult(
                count,
                targetFps,
                actualFps,
                currentElapsed,
                clientCountAtStart,
                clientCountAtEnd,
                finalMinClientCount,
                maxClientCount,
                avgClientCount);
        }
    }

    public ServerBroadcastMetricsResult GetResult()
    {
        var count = Interlocked.Read(ref messageCount);
        var actualFps = elapsed.TotalSeconds > 0 ? count / elapsed.TotalSeconds : 0;

        lock (clientCountLock)
        {
            var avgClientCount = clientCountSampleCount > 0 ? (double)totalClientCountSamples / clientCountSampleCount : 0;
            var finalMinClientCount = minClientCount == int.MaxValue ? 0 : minClientCount;

            return new ServerBroadcastMetricsResult(
                count,
                targetFps,
                actualFps,
                elapsed,
                clientCountAtStart,
                clientCountAtEnd,
                finalMinClientCount,
                maxClientCount,
                avgClientCount);
        }
    }

    public void Reset()
    {
        Interlocked.Exchange(ref messageCount, 0);
        elapsed = TimeSpan.Zero;
        targetFps = 0;
        
        lock (clientCountLock)
        {
            clientCountAtStart = 0;
            clientCountAtEnd = 0;
            minClientCount = int.MaxValue;
            maxClientCount = 0;
            totalClientCountSamples = 0;
            clientCountSampleCount = 0;
        }
    }
}

public record ServerBroadcastMetricsResult(
long TotalMessages, 
int TargetFps,
double ActualFps, 
TimeSpan Duration, 
int ClientCountAtStart,
int ClientCountAtEnd,
int MinClientCount,
int MaxClientCount,
double AvgClientCount);

