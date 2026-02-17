using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestService(BroadcastGroupService group, TimeProvider timeProvider) : ServiceBase<IPerfTestService>, IPerfTestService
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
        var start = timeProvider.GetTimestamp();
        try
        {
            while (!ct.IsCancellationRequested && timeProvider.GetElapsedTime(start) < timeout)
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
    /// <summary>
    /// first client that calls BroadcastAsync will execute the broadcast loop, and other clients will receive cached response without executing the loop until the first client's broadcast loop finishes (either by timeout or cancellation).
    /// This is to prevent multiple simultaneous broadcasts which can cause performance degradation and inaccurate metrics collection.
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="targetFps"></param>
    /// <returns></returns>
    public async UnaryResult<BroadcastPositionMessage> BroadcastAsync(TimeSpan timeout, int targetFps)
    {
        // Accept only one request at single BroadcastAsync execution. If multiple clients call simultaneously, they will be dropped.
        if (Interlocked.CompareExchange(ref broadcastLock, 1, 0) != 0)
        {
            // another broadcast is in progress.
            return BroadcastPositionMessage.Cached;
        }

        // first client path (execute broadcast loop and collect metrics)
        try
        {
            var response = BroadcastPositionMessage.Cached;
            using var cts = new CancellationTokenSource(timeout);
            // Combine server timeout and client cancellation (when client disconnects)
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, Context.CallContext.CancellationToken);
            var ct = linkedCts.Token;
            var start = timeProvider.GetTimestamp();

            // Calculate interval from target FPS
            var intervalMs = targetFps > 0 ? 1000.0 / targetFps : 0;
            var interval = intervalMs > 0 ? TimeSpan.FromMilliseconds(intervalMs) : TimeSpan.Zero;

            // Start metrics collection
            group.StartMetricsCollection(targetFps);

            try
            {
                if (interval > TimeSpan.Zero)
                {
                    // FPS-controlled broadcast
                    using var timer = new PeriodicTimer(interval, timeProvider);
                    while (await timer.WaitForNextTickAsync(ct) && timeProvider.GetElapsedTime(start) < timeout)
                    {
                        group.SendMessageToAll(response);
                    }
                }
                else
                {
                    // Maximum speed broadcast (no delay)
                    while (!ct.IsCancellationRequested && timeProvider.GetElapsedTime(start) < timeout)
                    {
                        group.SendMessageToAll(response);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing.
            }
            finally
            {
                // Stop metrics collection, send metrics and clear collected metrics
                group.StopMetricsCollection();
                await group.SendAndClearMetricsAsync();
            }

            return BroadcastPositionMessage.Cached;
        }
        finally
        {
            Interlocked.Exchange(ref broadcastLock, 0);
        }
    }
}
