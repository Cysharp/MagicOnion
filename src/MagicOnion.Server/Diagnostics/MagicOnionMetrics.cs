using System.Diagnostics;
using System.Diagnostics.Metrics;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Diagnostics;

internal class MagicOnionMetrics : IDisposable
{
    public const string MeterName = "MagicOnion.Server";

    static readonly object BoxedTrue = true;
    static readonly object BoxedFalse = false;

    readonly Meter meter;
    readonly UpDownCounter<long> streamingHubConnections;
    readonly Histogram<long> streamingHubMethodDuration;
    readonly Counter<long> streamingHubMethodCompletedCounter;
    readonly Counter<long> streamingHubMethodExceptionCounter;

    public MagicOnionMetrics(IMeterFactory meterFactory)
    {
        meter = meterFactory.Create(MeterName);

        streamingHubConnections = meter.CreateUpDownCounter<long>(
            "magiconion.server.streaminghub.connections",
            unit: "{connection}"
        );
        streamingHubMethodDuration = meter.CreateHistogram<long>(
            "magiconion.server.streaminghub.method_duration",
            unit: "ms"
        );
        streamingHubMethodCompletedCounter = meter.CreateCounter<long>(
            "magiconion.server.streaminghub.method_completed",
            unit: "{request}"
        );
        streamingHubMethodExceptionCounter = meter.CreateCounter<long>(
            "magiconion.server.streaminghub.exceptions",
            unit: "{exception}"
        );
    }

    public void StreamingHubConnectionIncrement(in MetricsContext context, string serviceInterfaceType)
    {
        if (context.StreamingHubConnectionsEnabled)
        {
            streamingHubConnections.Add(1, InitializeTagListForStreamingHub(serviceInterfaceType));
        }
    }

    public void StreamingHubConnectionDecrement(in MetricsContext context, string serviceInterfaceType)
    {
        if (context.StreamingHubConnectionsEnabled)
        {
            streamingHubConnections.Add(-1, InitializeTagListForStreamingHub(serviceInterfaceType));
        }
    }

    public void StreamingHubMethodCompleted(in MetricsContext context, StreamingHubHandler handler, long startingTimestamp, long endingTimestamp, bool isErrorOrInterrupted)
    {
        if (context.StreamingHubMethodDurationEnabled || context.StreamingHubMethodCompletedCounterEnabled)
        {
            var tags = InitializeTagListForStreamingHub(handler.HubName);
            tags.Add("rpc.method", handler.MethodInfo.Name);
            tags.Add("magiconion.streaminghub.is_error", isErrorOrInterrupted ? BoxedTrue : BoxedFalse);
            streamingHubMethodDuration.Record((long)TimeProvider.System.GetElapsedTime(startingTimestamp, endingTimestamp).TotalMilliseconds, tags);
            streamingHubMethodCompletedCounter.Add(1, tags);
        }
    }

    public void StreamingHubException(in MetricsContext context, StreamingHubHandler handler, Exception exception)
    {
        if (context.StreamingHubMethodExceptionCounterEnabled)
        {
            var tags = InitializeTagListForStreamingHub(handler.HubName);
            tags.Add("rpc.method", handler.MethodInfo.Name);
            tags.Add("error.type", exception.GetType().FullName!);
            streamingHubMethodExceptionCounter.Add(1, tags);
        }
    }

    static TagList InitializeTagListForStreamingHub(string hubName)
    {
        return new TagList()
        {
            {"rpc.system", "magiconion"},
            {"rpc.service", hubName},
        };
    }

    public void Dispose()
    {
        meter.Dispose();
    }

    // NOTE: A context needs to be created for each request. An instance of MagicOnionMetrics is registered as a singleton.
    public MetricsContext CreateContext()
        => new(
            streamingHubConnections.Enabled,
            streamingHubMethodDuration.Enabled,
            streamingHubMethodCompletedCounter.Enabled,
            streamingHubMethodExceptionCounter.Enabled
        );
}

internal readonly record struct MetricsContext(
    bool StreamingHubConnectionsEnabled,
    bool StreamingHubMethodDurationEnabled,
    bool StreamingHubMethodCompletedCounterEnabled,
    bool StreamingHubMethodExceptionCounterEnabled
);
