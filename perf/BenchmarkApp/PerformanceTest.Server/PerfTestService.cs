using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.Extensions.Logging;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestService(PerfGroupService group, ILogger<PerfTestService> logger) : ServiceBase<IPerfTestService>, IPerfTestService
{
    public UnaryResult<ServerInformation> GetServerInformationAsync()
    {
        return UnaryResult.FromResult(new ServerInformation(
            Environment.MachineName,
            ApplicationInformation.Current.BenchmarkerVersion,
            ApplicationInformation.Current.IsLatestMagicOnion,
            ApplicationInformation.Current.MagicOnionVersion,
            ApplicationInformation.Current.GrpcNetVersion,
            ApplicationInformation.Current.MessagePackVersion,
            ApplicationInformation.Current.MemoryPackVersion,
            ApplicationInformation.Current.IsReleaseBuild,
            ApplicationInformation.Current.FrameworkDescription,
            ApplicationInformation.Current.OSDescription,
            ApplicationInformation.Current.OSArchitecture,
            ApplicationInformation.Current.ProcessArchitecture,
            ApplicationInformation.Current.CpuModelName,
            ApplicationInformation.Current.IsServerGC,
            ApplicationInformation.Current.ProcessorCount,
            ApplicationInformation.Current.IsAttached));
    }

    public UnaryResult<Nil> UnaryParameterless()
    {
        return new UnaryResult<Nil>(Nil.Default);
    }

    public UnaryResult<string> UnaryArgRefReturnRef(string arg1, int arg2, int arg3)
    {
        return new UnaryResult<string>(arg1);
    }

    public UnaryResult<string> UnaryArgDynamicArgumentTupleReturnRef(string arg1, int arg2, int arg3, int arg4)
    {
        return new UnaryResult<string>(arg1 + arg2);
    }

    public UnaryResult<int> UnaryArgDynamicArgumentTupleReturnValue(string arg1, int arg2, int arg3, int arg4)
    {
        return new UnaryResult<int>(arg2);
    }

    public UnaryResult<(int StatusCode, byte[] Data)> UnaryLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5)
    {
        return UnaryResult.FromResult((123, arg5));
    }

    public UnaryResult<ComplexResponse> UnaryComplexAsync(string arg1, int arg2, int arg3, int arg4)
    {
        return UnaryResult.FromResult(ComplexResponse.Cached);
    }

    public async Task<ServerStreamingResult<SimpleResponse>> ServerStreamingAsync(TimeSpan timeout)
    {
        var response = SimpleResponse.Cached;
        var stream = GetServerStreamingContext<SimpleResponse>();

        var ct = stream.ServiceContext.CallContext.CancellationToken;
        var start = TimeProvider.System.GetTimestamp();
        try
        {
            while (!ct.IsCancellationRequested && TimeProvider.System.GetElapsedTime(start) < timeout)
            {
                await stream.WriteAsync(response);
            }
        }
        catch (OperationCanceledException)
        {
            // do nothing.
        }

        return stream.Result();
    }

    int broadcastLock = 0;
    public async UnaryResult<SimpleResponse> BroadcastAsync(TimeSpan timeout)
    {
        // Accept only one request at single BroadcastAsync execution. If multiple clients call simultaneously, they will be dropped.
        if (Interlocked.CompareExchange(ref broadcastLock, 1, 0) != 0)
        {
            // another broadcast is in progress.
            return SimpleResponse.Cached;
        }

        try
        {
            var response = SimpleResponse.Cached;
            using var cts = new CancellationTokenSource(timeout);
            var ct = cts.Token;
            var start = TimeProvider.System.GetTimestamp();


            // Start metrics collection
            group.MetricsContext.Start();
            
            // Record initial client count
            group.MetricsContext.UpdateClientCount(group.MemberCount);

            // Start periodic metrics logging task
            var metricsLoggingTask = Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10), TimeProvider.System);
                while (await timer.WaitForNextTickAsync(ct))
                {
                    var currentResult = group.MetricsContext.GetCurrentResult();
                    logger.LogInformation(
                        "[Server Broadcast Metrics (Periodic)] Clients: {ClientsAtStart}->{ClientsAtEnd} (Min: {MinClients}, Max: {MaxClients}, Avg: {AvgClients:F1}), Total Messages: {TotalMessages:N0}, Messages/sec: {MessagesPerSecond:N2}, Duration: {Duration}",
                        currentResult.ClientCountAtStart,
                        currentResult.ClientCountAtEnd,
                        currentResult.MinClientCount,
                        currentResult.MaxClientCount,
                        currentResult.AvgClientCount,
                        currentResult.TotalMessages,
                        currentResult.MessagesPerSecond,
                        currentResult.Duration);
                }
            }, ct);

            try
            {
                while (!ct.IsCancellationRequested && TimeProvider.System.GetElapsedTime(start) < timeout)
                {
                    group.SendMessageToAll(response);
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing.
            }
            finally
            {
                // Wait for metrics logging task to complete
                try
                {
                    await metricsLoggingTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
                catch
                {
                    // Ignore exceptions from metrics logging task
                }

                // Stop metrics collection and get result
                group.MetricsContext.Stop();
                var result = group.MetricsContext.GetResult();

                // Log final server-side broadcast metrics
                logger.LogInformation(
                    "[Server Broadcast Metrics (Final)] Clients: {ClientsAtStart}->{ClientsAtEnd} (Min: {MinClients}, Max: {MaxClients}, Avg: {AvgClients:F1}), Total Messages: {TotalMessages:N0}, Messages/sec: {MessagesPerSecond:N2}, Duration: {Duration}",
                    result.ClientCountAtStart,
                    result.ClientCountAtEnd,
                    result.MinClientCount,
                    result.MaxClientCount,
                    result.AvgClientCount,
                    result.TotalMessages,
                    result.MessagesPerSecond,
                    result.Duration);

                group.MetricsContext.Reset();
            }

            return SimpleResponse.Cached;
        }
        finally
        {
            Interlocked.Exchange(ref broadcastLock, 0);
        }
    }
}
