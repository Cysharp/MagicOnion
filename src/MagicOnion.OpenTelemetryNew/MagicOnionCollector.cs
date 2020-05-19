using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Metrics.Configuration;
using OpenTelemetry.Metrics.Export;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace MagicOnion.OpenTelemetry
{
    public class OpenTelemetryCollectorLogger : IMagicOnionLogger
    {
        static readonly string MethodKey = "MagicOnion/keys/Method";
        readonly IEnumerable<KeyValuePair<string, string>> defaultLabels;
        readonly ConcurrentDictionary<string, HashSet<KeyValuePair<string, string>>> labelCache = new ConcurrentDictionary<string, HashSet<KeyValuePair<string, string>>>();

        readonly MeasureMetric<double> buildServiceDefinitionMeasure;
        readonly CounterMetric<long> unaryRequestCounter;
        readonly MeasureMetric<long> unaryResponseSizeMeasure;
        readonly CounterMetric<long> unaryErrorCounter;
        readonly MeasureMetric<double> unaryElapsedMeasure;
        readonly CounterMetric<long> streamingHubErrorCounter;
        readonly MeasureMetric<double> streamingHubElapsedMeasure;
        readonly CounterMetric<long> streamingHubRequestCounter;
        readonly MeasureMetric<long> streamingHubResponseSizeMeasure;
        readonly CounterMetric<long> connectCounter;
        readonly CounterMetric<long> disconnectCounter;

        public OpenTelemetryCollectorLogger(MeterFactory meterFactory, IEnumerable<KeyValuePair<string, string>> defaultLabels = null)
        {
            // configure defaultTags included as default tag
            this.defaultLabels = defaultLabels ?? Array.Empty<KeyValuePair<string, string>>();

            // todo: how to description?
            var meter = meterFactory.GetMeter("MagicOnion");

            // Service build time. ms
            buildServiceDefinitionMeasure = meter.CreateDoubleMeasure("MagicOnion/measure/BuildServiceDefinition"); // sum
            // Unary request count. num
            unaryRequestCounter = meter.CreateInt64Counter("MagicOnion/measure/UnaryRequest"); // sum
            // Unary API response size. bytes
            unaryResponseSizeMeasure = meter.CreateInt64Measure("MagicOnion/measure/UnaryResponseSize"); // sum
            // Unary API error Count. num
            unaryErrorCounter = meter.CreateInt64Counter("MagicOnion/measure/UnaryErrorCount"); // sum
            // Unary API elapsed time. ms
            unaryElapsedMeasure = meter.CreateDoubleMeasure("MagicOnion/measure/UnaryElapsed"); // sum
            // StreamingHub API error Count. num
            streamingHubErrorCounter = meter.CreateInt64Counter("MagicOnion/measure/StreamingHubErrorCount"); // sum
            // StreamingHub API elapsed time. ms
            streamingHubElapsedMeasure = meter.CreateDoubleMeasure("MagicOnion/measure/StreamingHubElapsed"); // sum
            // StreamingHub request count. num
            streamingHubRequestCounter = meter.CreateInt64Counter("MagicOnion/measure/StreamingRequest"); // sum
            // StreamingHub API response size. bytes
            streamingHubResponseSizeMeasure = meter.CreateInt64Measure("MagicOnion/measure/StreamingHubResponseSize"); // sum
            // ConnectCount - DisconnectCount = current connect count. (successfully disconnected)
            // StreamingHub connect count. num
            connectCounter = meter.CreateInt64Counter("MagicOnion/measure/Connect"); // sum
            // StreamingHub disconnect count. num
            disconnectCounter = meter.CreateInt64Counter("MagicOnion/measure/Disconnect"); // sum
        }

        IEnumerable<KeyValuePair<string, string>> CreateLabel(ServiceContext context)
        {
            var label = labelCache.GetOrAdd(nameof(EndBuildServiceDefinition), new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( MethodKey, nameof(context.CallContext.Method)),
            });
            return label;
        }
        IEnumerable<KeyValuePair<string, string>> CreateLabel(StreamingHubContext context)
        {
            var label = labelCache.GetOrAdd(nameof(EndBuildServiceDefinition), new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( MethodKey, nameof(context.Path)),
            });
            return label;
        }
        IEnumerable<KeyValuePair<string, string>> CreateLabel(string value)
        {
            var label = labelCache.GetOrAdd(nameof(EndBuildServiceDefinition), new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( MethodKey, value),
            });
            return label;
        }

        public void BeginBuildServiceDefinition()
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            buildServiceDefinitionMeasure.Record(default(SpanContext), elapsed, CreateLabel(nameof(EndBuildServiceDefinition)));
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                connectCounter.Add(default(SpanContext), 1, CreateLabel(context));
            }
            else if (context.MethodType == MethodType.Unary)
            {
                unaryRequestCounter.Add(default(SpanContext), 1, CreateLabel(context));
            }
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                disconnectCounter.Add(default(SpanContext), 1, CreateLabel(context));
            }
            else if (context.MethodType == MethodType.Unary)
            {
                var spanContext = default(SpanContext);
                var label = CreateLabel(context);
                unaryElapsedMeasure.Record(spanContext, elapsed, label);
                unaryResponseSizeMeasure.Record(spanContext, response.LongLength, label);
                if (isErrorOrInterrupted)
                {
                    unaryErrorCounter.Add(spanContext, 1, label);
                }
            }
        }

        public void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type)
        {
            streamingHubRequestCounter.Add(default(SpanContext), 1, CreateLabel(context));
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var spanContext = default(SpanContext);
            var label = CreateLabel(context);
            streamingHubElapsedMeasure.Record(spanContext, elapsed, label);
            streamingHubRequestCounter.Add(spanContext, responseSize, label);
            if (isErrorOrInterrupted)
            {
                streamingHubErrorCounter.Add(spanContext, 1, label);
            }
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            // TODO:require more debugging aid(broadcast methodName).
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
        }
    }

    public class MagicOnionOpenTelemetryOption
    {
        /// <summary>
        /// Service Name for the app. default is Assembly name.
        /// </summary>
        public string ServiceName { get; }

        public MagicOnionOpenTelemetryOption()
        {
            ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
        }

        public MagicOnionOpenTelemetryOption(string serviceName)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// Global filter. Handle Unary and most outside logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryCollectorFilterAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        public MagicOnionFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryCollectorFilter(serviceLocator.GetService<TracerFactory>(), serviceLocator.GetService<MagicOnionOpenTelemetryOption>());
        }
    }

    internal class OpenTelemetryCollectorFilter : MagicOnionFilterAttribute
    {
        readonly TracerFactory tracerFactcory;
        readonly string serviceName;

        public OpenTelemetryCollectorFilter(TracerFactory tracerFactory, MagicOnionOpenTelemetryOption telemetryOption)
        {
            this.tracerFactcory = tracerFactory;
            this.serviceName = telemetryOption.ServiceName;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var tracer = tracerFactcory.GetTracer(context.CallContext.Method);
            tracer.CurrentSpan.SetAttribute("rpc.service", serviceName);

            // incoming kind: SERVER
            using (tracer.StartActiveSpan($"grpc.{serviceName}/{context.CallContext.Method}", SpanKind.Server, out var span))
            {
                try
                {
                    span.SetAttribute("net.peer.ip", context.CallContext.Peer);
                    span.SetAttribute("message.type", "RECIEVED");
                    span.SetAttribute("message.id", context.ContextId);
                    span.SetAttribute("message.uncompressed_size", context.GetRawRequest().LongLength);

                    await next(context);

                    span.SetAttribute("status_code", (long)context.CallContext.Status.StatusCode);
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode).WithDescription(context.CallContext.Status.Detail);
                }
                catch (Exception ex)
                {
                    span.SetAttribute("exception", ex.ToString());
                    span.SetAttribute("status_code", (long)context.CallContext.Status.StatusCode);
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode).WithDescription(context.CallContext.Status.Detail);
                }
            }
        }
    }

    /// <summary>
    /// StreamingHub Filter. Handle Streaming Hub logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryHubCollectorFilterAttribute : Attribute, IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public int Order { get; set; }

        public StreamingHubFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryHubCollectorFilter(serviceLocator.GetService<TracerFactory>(), serviceLocator.GetService<MagicOnionOpenTelemetryOption>());
        }
    }

    internal class OpenTelemetryHubCollectorFilter : StreamingHubFilterAttribute
    {
        readonly TracerFactory tracerFactcory;
        readonly string serviceName;

        public OpenTelemetryHubCollectorFilter(TracerFactory tracerFactory, MagicOnionOpenTelemetryOption telemetryOption)
        {
            this.tracerFactcory = tracerFactory;
            this.serviceName = telemetryOption.ServiceName;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var tracer = tracerFactcory.GetTracer(context.ServiceContext.CallContext.Method);
            tracer.CurrentSpan.SetAttribute("rpc.service", serviceName);

            // incoming kind: SERVER
            using (tracer.StartActiveSpan($"grpc.{serviceName}/{context.ServiceContext.CallContext.Method}", SpanKind.Server, out var span))
            {
                try
                {
                    span.SetAttribute("net.peer.ip", context.ServiceContext.CallContext.Peer);
                    span.SetAttribute("message.type", "RECIEVED");
                    span.SetAttribute("message.id", context.ServiceContext.ContextId);
                    span.SetAttribute("message.uncompressed_size", context.ServiceContext.GetRawRequest().LongLength);

                    await next(context);

                    span.SetAttribute("status_code", (long)context.ServiceContext.CallContext.Status.StatusCode);
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode).WithDescription(context.ServiceContext.CallContext.Status.Detail);
                }
                catch (Exception ex)
                {
                    span.SetAttribute("exception", ex.ToString());
                    span.SetAttribute("status_code", (long)context.ServiceContext.CallContext.Status.StatusCode);
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode).WithDescription(context.ServiceContext.CallContext.Status.Detail);
                }
            }
        }
    }

    public static class OpenTelemetrygRpcStatusHelper
    {
        // gRPC StatusCode and OpenTelemetry.CanonicalCode is same.
        public static global::OpenTelemetry.Trace.Status ConvertStatus(StatusCode code)
        {
            switch (code)
            {
                case StatusCode.OK:
                    return global::OpenTelemetry.Trace.Status.Ok;
                case StatusCode.Cancelled:
                    return global::OpenTelemetry.Trace.Status.Cancelled;
                case StatusCode.Unknown:
                    return global::OpenTelemetry.Trace.Status.Unknown;
                case StatusCode.InvalidArgument:
                    return global::OpenTelemetry.Trace.Status.InvalidArgument;
                case StatusCode.DeadlineExceeded:
                    return global::OpenTelemetry.Trace.Status.DeadlineExceeded;
                case StatusCode.NotFound:
                    return global::OpenTelemetry.Trace.Status.NotFound;
                case StatusCode.AlreadyExists:
                    return global::OpenTelemetry.Trace.Status.AlreadyExists;
                case StatusCode.PermissionDenied:
                    return global::OpenTelemetry.Trace.Status.PermissionDenied;
                case StatusCode.Unauthenticated:
                    return global::OpenTelemetry.Trace.Status.Unauthenticated;
                case StatusCode.ResourceExhausted:
                    return global::OpenTelemetry.Trace.Status.ResourceExhausted;
                case StatusCode.FailedPrecondition:
                    return global::OpenTelemetry.Trace.Status.FailedPrecondition;
                case StatusCode.Aborted:
                    return global::OpenTelemetry.Trace.Status.Aborted;
                case StatusCode.OutOfRange:
                    return global::OpenTelemetry.Trace.Status.OutOfRange;
                case StatusCode.Unimplemented:
                    return global::OpenTelemetry.Trace.Status.Unimplemented;
                case StatusCode.Internal:
                    return global::OpenTelemetry.Trace.Status.Internal;
                case StatusCode.Unavailable:
                    return global::OpenTelemetry.Trace.Status.Unavailable;
                case StatusCode.DataLoss:
                    return global::OpenTelemetry.Trace.Status.DataLoss;
                default:
                    // custom status code? use Unknown.
                    return global::OpenTelemetry.Trace.Status.Unknown;
            }
        }
    }
}