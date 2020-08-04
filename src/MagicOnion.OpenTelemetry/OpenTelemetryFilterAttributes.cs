using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace MagicOnion.OpenTelemetry
{
    /// <summary>
    /// Collect OpenTelemetry Tracing for Global filter. Handle Unary and most outside logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryCollectorFilterAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        public MagicOnionFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryCollectorFilter(serviceLocator.GetService<TracerProvider>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryCollectorFilter : MagicOnionFilterAttribute
    {
        readonly TracerProvider tracerProvider;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryCollectorFilter(TracerProvider tracerProvider, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.tracerProvider = tracerProvider;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var tracer = tracerProvider.GetTracer(context.CallContext.Method, telemetryOption.TracerVersion);

            // incoming kind: SERVER
            using (var span = tracer.StartSpan($"{context.CallContext.Method}", SpanKind.Server))
            {
                try
                {
                    span.SetAttribute("grpc.method", "Unary");
                    span.SetAttribute("rpc.service", telemetryOption.ServiceName);
                    span.SetAttribute("rpc.method", context.CallContext.Method);
                    span.SetAttribute("net.peer.ip", context.CallContext.Peer);
                    span.SetAttribute("net.host.name", context.CallContext.Host);
                    span.SetAttribute("message.type", "RECIEVED");
                    span.SetAttribute("message.id", context.ContextId.ToString());
                    span.SetAttribute("message.uncompressed_size", context.GetRawRequest()?.LongLength.ToString() ?? "0");

                    span.SetAttribute("magiconion.method.type", context.MethodType.ToString());
                    span.SetAttribute("magiconion.service.type", context.ServiceType.Name);
                    span.SetAttribute("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                    span.SetAttribute("magiconion.auth.peer.authenticated", context.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                    await next(context);

                    span.SetAttribute("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode);
                }
                catch (Exception ex)
                {
                    span.SetAttribute("exception", ex.ToString());
                    span.SetAttribute("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                    span.SetAttribute("grpc.status_detail", context.CallContext.Status.Detail);
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode);
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Collect OpenTelemetry Tracing for StreamingHub Filter. Handle Streaming Hub logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryHubCollectorFilterAttribute : Attribute, IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public int Order { get; set; }

        public StreamingHubFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryHubCollectorFilter(serviceLocator.GetService<TracerProvider>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryHubCollectorFilter : StreamingHubFilterAttribute
    {
        readonly TracerProvider tracerProvider;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryHubCollectorFilter(TracerProvider tracerProvider, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.tracerProvider = tracerProvider;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            var tracer = tracerProvider.GetTracer($"/{context.Path}", telemetryOption.TracerVersion);

            // incoming kind: SERVER
            using (var span = tracer.StartSpan($"/{context.Path}", SpanKind.Server))
            {
                try
                {
                    span.SetAttribute("grpc.method", "Duplex");
                    span.SetAttribute("rpc.service", telemetryOption.ServiceName);
                    span.SetAttribute("rpc.method", $"/{context.Path}");
                    span.SetAttribute("net.peer.ip", context.ServiceContext.CallContext.Peer);
                    span.SetAttribute("net.host.name", context.ServiceContext.CallContext.Host);
                    span.SetAttribute("message.type", "RECIEVED");
                    span.SetAttribute("message.id", context.ServiceContext.ContextId.ToString());
                    span.SetAttribute("message.uncompressed_size", context.Request.Length.ToString());

                    span.SetAttribute("magiconion.method.type", context.ServiceContext.MethodType.ToString());
                    span.SetAttribute("magiconion.service.type", context.ServiceContext.ServiceType.Name);
                    span.SetAttribute("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                    span.SetAttribute("magiconion.auth.peer.authenticated", context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                    await next(context);

                    span.SetAttribute("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode);
                }
                catch (Exception ex)
                {
                    span.SetAttribute("exception", ex.ToString());
                    span.SetAttribute("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                    span.SetAttribute("grpc.status_detail", context.ServiceContext.CallContext.Status.Detail);
                    span.Status = OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode);
                    throw;
                }
            }
        }
    }
}