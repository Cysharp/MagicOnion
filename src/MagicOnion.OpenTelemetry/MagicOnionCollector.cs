using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Stats;
using OpenTelemetry.Stats.Measures;
using OpenTelemetry.Tags;
using OpenTelemetry.Trace;
using System;
using System.Threading.Tasks;

namespace MagicOnion.OpenTelemetry
{
    public class OpenTelemetryCollectorLogger : IMagicOnionLogger
    {
        static readonly IMeasureDouble BuildServiceDefinition = MeasureDouble.Create("MagicOnion/measure/BuildServiceDefinition", "Service build time.", "ms");

        static readonly IMeasureDouble UnaryElapsed = MeasureDouble.Create("MagicOnion/measure/UnaryElapsed", "Unary API elapsed time.", "ms");
        static readonly IMeasureLong UnaryResponseSize = MeasureLong.Create("MagicOnion/measure/UnaryResponseSize", "Unary API response size.", "bytes");
        static readonly IMeasureLong UnaryErrorCount = MeasureLong.Create("MagicOnion/measure/UnaryErrorCount", "Unary API error Count.", "num");

        static readonly IMeasureDouble StreamingHubElapsed = MeasureDouble.Create("MagicOnion/measure/StreamingHubElapsed", "StreamingHub API elapsed time.", "ms");
        static readonly IMeasureLong StreamingHubResponseSize = MeasureLong.Create("MagicOnion/measure/StreamingHubResponseSize", "StreamingHub API response size.", "bytes");
        static readonly IMeasureLong StreamingHubErrorCount = MeasureLong.Create("MagicOnion/measure/StreamingHubErrorCount", "StreamingHub API error Count.", "num");

        static readonly IMeasureLong ConnectCount = MeasureLong.Create("MagicOnion/measure/Connect", "StreamingHub connect count.", "num");
        static readonly IMeasureLong DisconnectCount = MeasureLong.Create("MagicOnion/measure/Disconnect", "StreamingHub disconnect count.", "num");

        static readonly TagKey MethodKey = TagKey.Create("MagicOnion/keys/Method");

        readonly IStatsRecorder statsRecorder;
        readonly ITagger tagger;
        readonly ITagContext defaultTags;

        public OpenTelemetryCollectorLogger(IStatsRecorder statsRecorder, ITagger tagger, ITagContext defaultTags = null)
        {
            this.statsRecorder = statsRecorder;
            this.tagger = tagger;
            this.defaultTags = defaultTags ?? TagContext.Empty;
        }

        ITagContext CreateTag(ServiceContext context)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(context.CallContext.Method)).Build();
        }

        ITagContext CreateTag(StreamingHubContext context)
        {
            return tagger.ToBuilder(defaultTags).Put(MethodKey, TagValue.Create(context.Path)).Build();
        }

        public void BeginBuildServiceDefinition()
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            statsRecorder.NewMeasureMap().Put(BuildServiceDefinition, elapsed).Record(defaultTags);
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                statsRecorder.NewMeasureMap().Put(ConnectCount, 1).Record(CreateTag(context));
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
                    span.SetAttribute("request.size", context.GetRawRequest().LongLength);

                    await Next(context);

                    span.SetAttribute("response.size", context.GetRawResponse().LongLength);
                    span.SetAttribute("status_code", (long)context.CallContext.Status.StatusCode);
                    span.Status = ConvertStatus(context.CallContext.Status.StatusCode).WithDescription(context.CallContext.Status.Detail);
                }
                catch (Exception ex)
                {
                    span.SetAttribute("exception", ex.ToString());

                    span.SetAttribute("status_code", (long)context.CallContext.Status.StatusCode);
                    span.Status = ConvertStatus(context.CallContext.Status.StatusCode).WithDescription(context.CallContext.Status.Detail);
                }
            }
        }

        // gRPC StatusCode and OpenTelemetry.CanonicalCode is same.
        static global::OpenTelemetry.Trace.Status ConvertStatus(StatusCode code)
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
