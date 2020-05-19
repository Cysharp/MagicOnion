using System;
using System.Threading.Tasks;
using MagicOnion.Server;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace MagicOnion.OpenTelemetry
{
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
}