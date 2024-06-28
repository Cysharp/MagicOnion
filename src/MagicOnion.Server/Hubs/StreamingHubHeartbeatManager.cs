using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using MagicOnion.Internal;
using MagicOnion.Server.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Hubs;

internal interface IStreamingHubHeartbeatManager : IDisposable
{
    StreamingHubHeartbeatHandle Register(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext);
    void Unregister(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext);
}

internal class StreamingHubHeartbeatHandle : IDisposable
{
    readonly IStreamingHubHeartbeatManager manager;
    readonly CancellationTokenSource timeoutToken;
    readonly TimeSpan timeoutDuration;
    bool disposed;

    public IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> ServiceContext { get; }
    public CancellationToken TimeoutToken => timeoutToken.Token;

    public StreamingHubHeartbeatHandle(IStreamingHubHeartbeatManager manager, IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext, TimeSpan timeoutDuration)
    {
        this.manager = manager;
        this.ServiceContext = serviceContext;
        this.timeoutDuration = timeoutDuration;
        this.timeoutToken = new CancellationTokenSource();
    }

    public void RestartTimeoutTimer()
    {
        if (disposed || timeoutDuration == Timeout.InfiniteTimeSpan) return;
        timeoutToken.CancelAfter(timeoutDuration);
    }

    public void Ack()
    {
        if (disposed || timeoutDuration == Timeout.InfiniteTimeSpan) return;
        timeoutToken.CancelAfter(Timeout.InfiniteTimeSpan);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        manager.Unregister(ServiceContext);
        timeoutToken.Dispose();
    }
}

internal class NopStreamingHubHeartbeatManager : IStreamingHubHeartbeatManager
{
    public static IStreamingHubHeartbeatManager Instance { get; } = new NopStreamingHubHeartbeatManager();

    NopStreamingHubHeartbeatManager() {}

    public StreamingHubHeartbeatHandle Register(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext)
        => new(this, serviceContext, Timeout.InfiniteTimeSpan);
    public void Unregister(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext) { }
    public void Dispose() { }
}

internal class StreamingHubHeartbeatManager : IStreamingHubHeartbeatManager
{
    static ReadOnlySpan<byte> Nil => [0xc0];

    readonly object timerGate = new();
    readonly IStreamingHubHeartbeatMetadataProvider? heartbeatMetadataProvider;
    readonly TimeSpan heartbeatInterval;
    readonly TimeSpan timeoutDuration;
    readonly ILogger logger;

    PeriodicTimer? timer;
    int registeredCount;
    ConcurrentDictionary<Guid, StreamingHubHeartbeatHandle> contexts = new();

    public StreamingHubHeartbeatManager(TimeSpan heartbeatInterval, TimeSpan timeoutDuration, IStreamingHubHeartbeatMetadataProvider? heartbeatMetadataProvider, ILogger<StreamingHubHeartbeatManager> logger)
    {
        this.heartbeatInterval = heartbeatInterval;
        this.timeoutDuration = timeoutDuration;
        this.heartbeatMetadataProvider = heartbeatMetadataProvider;
        this.logger = logger;
    }

    public StreamingHubHeartbeatHandle Register(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext)
    {
        var handle = new StreamingHubHeartbeatHandle(this, serviceContext, timeoutDuration);
        if (contexts.TryAdd(serviceContext.ContextId, handle))
        {
            if (Interlocked.Increment(ref registeredCount) == 1)
            {
                lock (timerGate)
                {
                    if (timer is null)
                    {
                        timer = new PeriodicTimer(heartbeatInterval);
                        MagicOnionServerLog.BeginHeartbeatTimer(this.logger, serviceContext.CallContext.Method, heartbeatInterval, timeoutDuration);
                        _ = StartHeartbeatAsync(timer, serviceContext.CallContext.Method);
                    }
                }
            }

            handle.TimeoutToken.UnsafeRegister(_ => MagicOnionServerLog.HeartbeatTimedOut(logger, serviceContext.CallContext.Method, serviceContext.ContextId), null);

            return handle;
        }

        return contexts[serviceContext.ContextId];
    }

    async Task StartHeartbeatAsync(PeriodicTimer runningTimer, string method)
    {
        Debug.Assert(runningTimer != null);

        var writer = new ArrayBufferWriter<byte>();
        while (await runningTimer.WaitForNextTickAsync())
        {
            StreamingHubMessageWriter.WriteHeartbeatMessageForServerToClientHeader(writer);
            if (!(heartbeatMetadataProvider?.TryWriteMetadata(writer) ?? false))
            {
                writer.Write(Nil);
            }


            MagicOnionServerLog.SendHeartbeat(this.logger, method);
            try
            {
                foreach (var (contextId, handle) in contexts)
                {
                    handle.RestartTimeoutTimer();
                    handle.ServiceContext.QueueResponseStreamWrite(StreamingHubPayloadPool.Shared.RentOrCreate(writer.WrittenSpan));
                }
            }
            catch { /* Ignore */ }

            writer.Clear();
        }
    }

    public void Unregister(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext)
    {
        if (contexts.TryRemove(serviceContext.ContextId, out _))
        {
            if (Interlocked.Decrement(ref registeredCount) == 0)
            {
                lock (timerGate)
                {
                    if (Volatile.Read(ref registeredCount) == 0 && timer is not null)
                    {
                        MagicOnionServerLog.ShutdownHeartbeatTimer(this.logger, serviceContext.CallContext.Method);
                        timer.Dispose();
                        timer = null;
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}

public interface IStreamingHubHeartbeatMetadataProvider
{
    bool TryWriteMetadata(IBufferWriter<byte> writer);
}
