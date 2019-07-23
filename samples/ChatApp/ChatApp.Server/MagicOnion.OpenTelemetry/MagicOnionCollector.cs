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
        static readonly List<TagKey> MethodKeys = new List<TagKey>() { MethodKey };

        #region BuildServiceDefinition
        static readonly string BuildServiceDefinitionName = "MagicOnion/measure/BuildServiceDefinition";
        static readonly IMeasureDouble BuildServiceDefinition = MeasureDouble.Create(BuildServiceDefinitionName, "Service build time.", "ms");
        private static readonly IView BuildServiceDefinitionView = View.Create(
            name: ViewName.Create(BuildServiceDefinitionName),
            description: string.Empty,
            measure: BuildServiceDefinition,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region UnaryResponseSize
        static readonly string UnaryResponseSizeName = "MagicOnion/measure/UnaryResponseSize";
        static readonly IMeasureLong UnaryResponseSize = MeasureLong.Create(UnaryResponseSizeName, "Unary API response size.", "bytes");
        private static readonly IView UnaryResponseSizeView = View.Create(
            name: ViewName.Create(UnaryResponseSizeName),
            description: string.Empty,
            measure: UnaryResponseSize,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region UnaryErrorCount
        static readonly string UnaryErrorCountName = "MagicOnion/measure/UnaryErrorCount";
        static readonly IMeasureLong UnaryErrorCount = MeasureLong.Create(UnaryErrorCountName, "Unary API error Count.", "num");
        private static readonly IView UnaryErrorCountView = View.Create(
            name: ViewName.Create(UnaryErrorCountName),
            description: string.Empty,
            measure: UnaryErrorCount,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region UnaryElapsed
        static readonly string UnaryElapsedName = "MagicOnion/measure/UnaryElapsed";
        static readonly IMeasureDouble UnaryElapsed = MeasureDouble.Create(UnaryElapsedName, "Unary API elapsed time.", "ms");
        private static readonly IView UnaryElapsedView = View.Create(
            name: ViewName.Create(UnaryElapsedName),
            description: string.Empty,
            measure: UnaryElapsed,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region StreamingHubErrorCount
        static readonly string StreamingHubErrorCountName = "MagicOnion/measure/StreamingHubErrorCount";
        static readonly IMeasureLong StreamingHubErrorCount = MeasureLong.Create(StreamingHubErrorCountName, "StreamingHub API error Count.", "num");
        private static readonly IView StreamingHubErrorCountView = View.Create(
            name: ViewName.Create(StreamingHubErrorCountName),
            description: string.Empty,
            measure: StreamingHubErrorCount,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region StreamingHubElapsed
        static readonly string StreamingHubElapsedName = "MagicOnion/measure/StreamingHubElapsed";
        static readonly IMeasureDouble StreamingHubElapsed = MeasureDouble.Create(StreamingHubElapsedName, "StreamingHub API elapsed time.", "ms");
        private static readonly IView StreamingHubElapsedView = View.Create(
            name: ViewName.Create(StreamingHubElapsedName),
            description: string.Empty,
            measure: StreamingHubElapsed,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region StreamingHubResponseSize
        static readonly string StreamingHubResponseSizeName = "MagicOnion/measure/StreamingHubResponseSize";
        static readonly IMeasureLong StreamingHubResponseSize = MeasureLong.Create(StreamingHubResponseSizeName, "StreamingHub API response size.", "bytes");
        private static readonly IView StreamingHubResponseSizeView = View.Create(
            name: ViewName.Create(StreamingHubResponseSizeName),
            description: string.Empty,
            measure: StreamingHubResponseSize,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region ConnectCount
        static readonly string ConnectCountName = "MagicOnion/measure/Connect";
        static readonly IMeasureLong ConnectCount = MeasureLong.Create(ConnectCountName, "StreamingHub connect count.", "num");
        private static readonly IView ConnectCountView = View.Create(
            name: ViewName.Create(ConnectCountName),
            description: string.Empty,
            measure: ConnectCount,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region DisconnectCount
        static readonly string DisconnectCountName = "MagicOnion/measure/Disconnect";
        static readonly IMeasureLong DisconnectCount = MeasureLong.Create(DisconnectCountName, "StreamingHub disconnect count.", "num");
        private static readonly IView DisconnectCountView = View.Create(
            name: ViewName.Create(DisconnectCountName),
            description: string.Empty,
            measure: DisconnectCount,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region StreamingRequest
        static readonly string StreamingRequestCountName = "MagicOnion/measure/StreamingRequest";
        static readonly IMeasureLong StreamingRequestCount = MeasureLong.Create(StreamingRequestCountName, "StreamingHub request count.", "num");
        private static readonly IView StreamingRequestCountView = View.Create(
            name: ViewName.Create(StreamingRequestCountName),
            description: string.Empty,
            measure: StreamingRequestCount,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        #region UnaryRequest
        static readonly string UnaryRequestCountName = "MagicOnion/measure/UnaryRequest";
        static readonly IMeasureLong UnaryRequestCount = MeasureLong.Create(UnaryRequestCountName, "Unary request count.", "num");
        private static readonly IView UnaryRequestCountView = View.Create(
            name: ViewName.Create(UnaryRequestCountName),
            description: string.Empty,
            measure: UnaryRequestCount,
            aggregation: Sum.Create(),
            columns: MethodKeys);
        #endregion

        readonly IStatsRecorder statsRecorder;
        readonly ITagger tagger;
        readonly ITagContext defaultTags;

        public OpenTelemetryCollectorLogger(IStatsRecorder statsRecorder, ITagger tagger, ITagContext defaultTags = null)
        {
            this.statsRecorder = statsRecorder;
            this.tagger = tagger;
            this.defaultTags = defaultTags ?? TagContext.Empty;

            Stats.ViewManager.RegisterView(BuildServiceDefinitionView);
            Stats.ViewManager.RegisterView(UnaryErrorCountView);
            Stats.ViewManager.RegisterView(UnaryElapsedView);
            Stats.ViewManager.RegisterView(UnaryResponseSizeView);
            Stats.ViewManager.RegisterView(ConnectCountView);
            Stats.ViewManager.RegisterView(DisconnectCountView);
            Stats.ViewManager.RegisterView(StreamingRequestCountView);
            Stats.ViewManager.RegisterView(UnaryRequestCountView);
            Stats.ViewManager.RegisterView(StreamingHubErrorCountView);
            Stats.ViewManager.RegisterView(StreamingHubElapsedView);
            Stats.ViewManager.RegisterView(StreamingHubResponseSizeView);
        }

        ITagContext CreateTag(ServiceContext context)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(context.CallContext.Method)).Build();
        }

        ITagContext CreateTag(StreamingHubContext context)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(context.Path)).Build();
        }

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
            statsRecorder.NewMeasureMap().Put(StreamingRequestCount, 1).Record(CreateTag(context));
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
    /// global filter. handle Unary and most outside logging.
    /// </summary>
    public class OpenTelemetryCollectorFilter : MagicOnionFilterAttribute
    {
        public OpenTelemetryCollectorFilter(Func<ServiceContext, ValueTask> next) :
            base(next)
        {
        }

        public override async ValueTask Invoke(ServiceContext context)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/semantic-conventions.md#grpc

            var tracer = context.ServiceLocator.GetService<ITracer>();
            var sampler = context.ServiceLocator.GetService<ISampler>();

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var spanBuilder = tracer.SpanBuilder(context.CallContext.Method, SpanKind.Server);
            if (sampler != null)
            {
                spanBuilder.SetSampler(sampler);
            }

            using (spanBuilder.StartScopedSpan(out var span))
            {
                try
                {
                    span.SetAttribute("component", "grpc");
                    //span.SetAttribute("request.size", context.GetRawRequest().LongLength);

                    await Next(context);

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
            }
        }
    }

    /// <summary>
    /// streamingHub Filter. handle Streaming Hub logging.
    /// </summary>
    public class OpenTelemetryHubCollectorFilter : StreamingHubFilterAttribute
    {
        public OpenTelemetryHubCollectorFilter(Func<StreamingHubContext, ValueTask> next) : base(next)
        {
        }

        public override async ValueTask Invoke(StreamingHubContext context)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/semantic-conventions.md#grpc

            var tracer = context.ServiceContext.ServiceLocator.GetService<ITracer>();
            var sampler = context.ServiceContext.ServiceLocator.GetService<ISampler>();

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var spanBuilder = tracer.SpanBuilder(context.ServiceContext.CallContext.Method, SpanKind.Server);
            if (sampler != null)
            {
                spanBuilder.SetSampler(sampler);
            }

            using (spanBuilder.StartScopedSpan(out var span))
            {
                try
                {
                    span.SetAttribute("component", "grpc");
                    //span.SetAttribute("request.size", context.GetRawRequest().LongLength);

                    await Next(context);

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
            }
        }
    }

    internal static class OpenTelemetrygRpcStatusHelper
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