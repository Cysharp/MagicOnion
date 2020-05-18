using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Metrics.Configuration;
using OpenTelemetry.Metrics.Export;
using OpenTelemetry.Trace;

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

        /// <summary>
        /// Create tags with context and put to metrics
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ITagContext CreateTag(ServiceContext context)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(context.CallContext.Method)).Build();
        }

        /// <summary>
        /// Create tags with context and put to metrics
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ITagContext CreateTag(StreamingHubContext context)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(context.Path)).Build();
        }

        /// <summary>
        /// Create tags with value and put to metrics
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ITagContext CreateTag(string value)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(value)).Build();
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

    /// <summary>
    /// Global filter. Handle Unary and most outside logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryCollectorFilterAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        public MagicOnionFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryCollectorFilter(serviceLocator.GetService<ITracer>(), serviceLocator.GetService<ISampler>());
        }
    }

    internal class OpenTelemetryCollectorFilter : MagicOnionFilterAttribute
    {
        readonly ITracer tracer;
        readonly ISampler sampler;

        public OpenTelemetryCollectorFilter(ITracer tracer, ISampler sampler)
        {
            this.tracer = tracer;
            this.sampler = sampler;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/semantic-conventions.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var spanBuilder = tracer.SpanBuilder(context.CallContext.Method).SetSpanKind(SpanKind.Server);

            if (sampler != null)
            {
                spanBuilder.SetSampler(sampler);
            }

            var span = spanBuilder.StartSpan();
            try
            {
                span.SetAttribute("component", "grpc");
                //span.SetAttribute("request.size", context.GetRawRequest().LongLength);

                await next(context);

                //span.SetAttribute("response.size", context.GetRawResponse().LongLength);
                span.SetAttribute("status_code", (long)context.CallContext.Status.StatusCode);
                span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode).WithDescription(context.CallContext.Status.Detail);
            }
            catch (Exception ex)
            {
                span.SetAttribute("exception", ex.ToString());

                span.SetAttribute("status_code", (long)context.CallContext.Status.StatusCode);
                span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode).WithDescription(context.CallContext.Status.Detail);
                throw;
            }
            finally
            {
                span.End();
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
            return new OpenTelemetryHubCollectorFilter(serviceLocator.GetService<ITracer>(), serviceLocator.GetService<ISampler>());
        }
    }

    internal class OpenTelemetryHubCollectorFilter : StreamingHubFilterAttribute
    {
        readonly ITracer tracer;
        readonly ISampler sampler;

        public OpenTelemetryHubCollectorFilter(ITracer tracer, ISampler sampler)
        {
            this.tracer = tracer;
            this.sampler = sampler;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/semantic-conventions.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var spanBuilder = tracer.SpanBuilder(context.ServiceContext.CallContext.Method).SetSpanKind(SpanKind.Server);

            if (sampler != null)
            {
                spanBuilder.SetSampler(sampler);
            }

            var span = spanBuilder.StartSpan();
            try
            {
                span.SetAttribute("component", "grpc");
                //span.SetAttribute("request.size", context.GetRawRequest().LongLength);

                await next(context);

                //span.SetAttribute("response.size", context.GetRawResponse().LongLength);
                span.SetAttribute("status_code", (long)context.ServiceContext.CallContext.Status.StatusCode);
                span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode).WithDescription(context.ServiceContext.CallContext.Status.Detail);
            }
            catch (Exception ex)
            {
                span.SetAttribute("exception", ex.ToString());

                span.SetAttribute("status_code", (long)context.ServiceContext.CallContext.Status.StatusCode);
                span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode).WithDescription(context.ServiceContext.CallContext.Status.Detail);
                throw;
            }
            finally
            {
                span.End();
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