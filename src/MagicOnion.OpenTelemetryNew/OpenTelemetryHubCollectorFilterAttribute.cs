using System;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace MagicOnion.OpenTelemetry
{
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
}