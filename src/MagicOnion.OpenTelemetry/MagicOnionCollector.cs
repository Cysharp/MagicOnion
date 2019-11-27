using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Stats;
using OpenTelemetry.Stats.Aggregations;
using OpenTelemetry.Stats.Measures;
using OpenTelemetry.Tags;
using OpenTelemetry.Trace;

namespace MagicOnion.OpenTelemetry
{
    public class OpenTelemetryCollectorLogger : IMagicOnionLogger
    {
        static readonly TagKey MethodKey = TagKey.Create("MagicOnion/keys/Method");
        static readonly List<TagKey> TagKeys = new List<TagKey>() { MethodKey };

        #region BuildServiceDefinition
        static readonly string BuildServiceDefinitionName = "MagicOnion/measure/BuildServiceDefinition";
        static readonly IMeasureDouble BuildServiceDefinition = MeasureDouble.Create(BuildServiceDefinitionName, "Service build time.", "ms");
        #endregion

        #region UnaryRequest
        static readonly string UnaryRequestCountName = "MagicOnion/measure/UnaryRequest";
        static readonly IMeasureLong UnaryRequestCount = MeasureLong.Create(UnaryRequestCountName, "Unary request count.", "num");
        #endregion

        #region UnaryResponseSize
        static readonly string UnaryResponseSizeName = "MagicOnion/measure/UnaryResponseSize";
        static readonly IMeasureLong UnaryResponseSize = MeasureLong.Create(UnaryResponseSizeName, "Unary API response size.", "bytes");
        #endregion

        #region UnaryErrorCount
        static readonly string UnaryErrorCountName = "MagicOnion/measure/UnaryErrorCount";
        static readonly IMeasureLong UnaryErrorCount = MeasureLong.Create(UnaryErrorCountName, "Unary API error Count.", "num");
        #endregion

        #region UnaryElapsed
        static readonly string UnaryElapsedName = "MagicOnion/measure/UnaryElapsed";
        static readonly IMeasureDouble UnaryElapsed = MeasureDouble.Create(UnaryElapsedName, "Unary API elapsed time.", "ms");
        #endregion

        #region StreamingHubErrorCount
        static readonly string StreamingHubErrorCountName = "MagicOnion/measure/StreamingHubErrorCount";
        static readonly IMeasureLong StreamingHubErrorCount = MeasureLong.Create(StreamingHubErrorCountName, "StreamingHub API error Count.", "num");
        #endregion

        #region StreamingHubElapsed
        static readonly string StreamingHubElapsedName = "MagicOnion/measure/StreamingHubElapsed";
        static readonly IMeasureDouble StreamingHubElapsed = MeasureDouble.Create(StreamingHubElapsedName, "StreamingHub API elapsed time.", "ms");
        #endregion

        #region StreamingHubResponseSize
        static readonly string StreamingHubResponseSizeName = "MagicOnion/measure/StreamingHubResponseSize";
        static readonly IMeasureLong StreamingHubResponseSize = MeasureLong.Create(StreamingHubResponseSizeName, "StreamingHub API response size.", "bytes");
        #endregion

        #region ConnectCount
        static readonly string ConnectCountName = "MagicOnion/measure/Connect";
        static readonly IMeasureLong ConnectCount = MeasureLong.Create(ConnectCountName, "StreamingHub connect count.", "num");
        #endregion

        #region DisconnectCount
        static readonly string DisconnectCountName = "MagicOnion/measure/Disconnect";
        static readonly IMeasureLong DisconnectCount = MeasureLong.Create(DisconnectCountName, "StreamingHub disconnect count.", "num");
        #endregion

        #region StreamingRequest
        static readonly string StreamingHubRequestCountName = "MagicOnion/measure/StreamingRequest";
        static readonly IMeasureLong StreamingHubRequestCount = MeasureLong.Create(StreamingHubRequestCountName, "StreamingHub request count.", "num");
        #endregion


        readonly IStatsRecorder statsRecorder;
        readonly ITagger tagger;
        readonly ITagContext defaultTags;

        public OpenTelemetryCollectorLogger(IStatsRecorder statsRecorder, ITagger tagger, ITagContext defaultTags = null)
        {
            this.statsRecorder = statsRecorder;
            this.tagger = tagger;
            // configure defaultTags included as default tag
            this.defaultTags = defaultTags ?? TagContext.Empty;
            if (this.defaultTags != TagContext.Empty)
            {
                foreach (var tag in defaultTags)
                {
                    TagKeys.Add(tag.Key);
                }
            }

            // Create Views
            var buildServiceDefinitionView = View.Create(
                name: ViewName.Create(BuildServiceDefinitionName),
                description: "Build ServiceDefinition elapsed time(ms)",
                measure: BuildServiceDefinition,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var unaryRequestCountView = View.Create(
                name: ViewName.Create(UnaryRequestCountName),
                description: "Request count for Unary request.",
                measure: UnaryRequestCount,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var unaryResponseSizeView = View.Create(
                name: ViewName.Create(UnaryResponseSizeName),
                description: "Response size for Unary response.",
                measure: UnaryResponseSize,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var unaryErrorCountView = View.Create(
                name: ViewName.Create(UnaryErrorCountName),
                description: "Error count for Unary request.",
                measure: UnaryErrorCount,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var unaryElapsedView = View.Create(
                name: ViewName.Create(UnaryElapsedName),
                description: "Elapsed time for Unary request.",
                measure: UnaryElapsed,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var streamingHubErrorCountView = View.Create(
                name: ViewName.Create(StreamingHubErrorCountName),
                description: "Error count for Streaminghub request.",
                measure: StreamingHubErrorCount,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var streamingHubElapsedView = View.Create(
                name: ViewName.Create(StreamingHubElapsedName),
                description: "Elapsed time for Streaminghub request.",
                measure: StreamingHubElapsed,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var streamingHubRequestCountView = View.Create(
                name: ViewName.Create(StreamingHubRequestCountName),
                description: "Request count for Streaminghub request.",
                measure: StreamingHubRequestCount,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var streamingHubResponseSizeView = View.Create(
                name: ViewName.Create(StreamingHubResponseSizeName),
                description: "Response size for Streaminghub request.",
                measure: StreamingHubResponseSize,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var connectCountView = View.Create(
                name: ViewName.Create(ConnectCountName),
                description: "Connect count for Streaminghub request. ConnectCount - DisconnectCount = current connect count. (successfully disconnected)",
                measure: ConnectCount,
                aggregation: Sum.Create(),
                columns: TagKeys);
            var disconnectCountView = View.Create(
                name: ViewName.Create(DisconnectCountName),
                description: "Disconnect count for Streaminghub request. ConnectCount - DisconnectCount = current connect count. (successfully disconnected)",
                measure: DisconnectCount,
                aggregation: Sum.Create(),
                columns: TagKeys);

            // Register Views
            Stats.ViewManager.RegisterView(buildServiceDefinitionView);
            Stats.ViewManager.RegisterView(unaryRequestCountView);
            Stats.ViewManager.RegisterView(unaryResponseSizeView);
            Stats.ViewManager.RegisterView(unaryErrorCountView);
            Stats.ViewManager.RegisterView(unaryElapsedView);
            Stats.ViewManager.RegisterView(connectCountView);
            Stats.ViewManager.RegisterView(disconnectCountView);
            Stats.ViewManager.RegisterView(streamingHubRequestCountView);
            Stats.ViewManager.RegisterView(streamingHubResponseSizeView);
            Stats.ViewManager.RegisterView(streamingHubErrorCountView);
            Stats.ViewManager.RegisterView(streamingHubElapsedView);
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

        public void BeginBuildServiceDefinition()
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            statsRecorder.NewMeasureMap().Put(BuildServiceDefinition, elapsed).Record(CreateTag(nameof(BuildServiceDefinition)));
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                statsRecorder.NewMeasureMap().Put(ConnectCount, 1).Record(CreateTag(context));
            }
            else if (context.MethodType == MethodType.Unary)
            {
                statsRecorder.NewMeasureMap().Put(UnaryRequestCount, 1).Record(CreateTag(context));
            }
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                statsRecorder.NewMeasureMap().Put(DisconnectCount, 1).Record(CreateTag(context));
            }
            else if (context.MethodType == MethodType.Unary)
            {
                var map = statsRecorder.NewMeasureMap();

                map.Put(UnaryElapsed, elapsed);
                map.Put(UnaryResponseSize, response.LongLength);
                if (isErrorOrInterrupted)
                {
                    map.Put(UnaryErrorCount, 1);
                }

                map.Record(CreateTag(context));
            }
        }

        public void BeginInvokeHubMethod(StreamingHubContext context, ArraySegment<byte> request, Type type)
        {
            statsRecorder.NewMeasureMap().Put(StreamingHubRequestCount, 1).Record(CreateTag(context));
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var map = statsRecorder.NewMeasureMap();

            map.Put(StreamingHubElapsed, elapsed);
            map.Put(StreamingHubResponseSize, responseSize);
            if (isErrorOrInterrupted)
            {
                map.Put(StreamingHubErrorCount, 1);
            }

            map.Record(CreateTag(context));
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