using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Trace;

namespace MagicOnion.OpenTelemetry
{
    /// <summary>
    /// Collect OpenTelemetry Tracing for Global filter. Handle Unary and most outside logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryCollectorFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryCollectorFilterAttribute(serviceLocator.GetService<ActivitySource>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryCollectorFilterAttribute : MagicOnionFilterAttribute
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryCollectorFilterAttribute(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.source = activitySource;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // Client -> Server incoming filter
            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            using var activity = source.StartActivity($"{context.MethodType}:{context.CallContext.Method}", ActivityKind.Server);

            try
            {
                activity.SetTag("grpc.method", context.MethodType.ToString());
                activity.SetTag("rpc.system", "grpc");
                activity.SetTag("rpc.service", context.ServiceType.Name);
                activity.SetTag("rpc.method", context.CallContext.Method);
                // todo: context.CallContext.Peer/Host format is https://github.com/grpc/grpc/blob/master/doc/naming.md and not uri standard.
                activity.SetTag("net.peer.name", context.CallContext.Peer);
                activity.SetTag("net.host.name", context.CallContext.Host);
                activity.SetTag("message.type", "RECIEVED");
                activity.SetTag("message.id", context.ContextId.ToString());
                activity.SetTag("message.uncompressed_size", context.GetRawRequest()?.LongLength.ToString() ?? "0");

                // todo: net.peer.name not report on tracer. use custom tag
                activity.SetTag("magiconion.peer.ip", context.CallContext.Peer);
                activity.SetTag("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                activity.SetTag("magiconion.auth.peer.authenticated", context.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);

                activity.SetTag("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                activity.SetTag("exception", ex.ToString());
                activity.SetTag("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                activity.SetTag("grpc.status_detail", context.CallContext.Status.Detail);
                activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode));
                throw;
            }
        }
    }

    /// <summary>
    /// Collect OpenTelemetry Tracing for StreamingHub Filter. Handle Streaming Hub logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryHubCollectorFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public int Order { get; set; }

        StreamingHubFilterAttribute IMagicOnionFilterFactory<StreamingHubFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            return new OpenTelemetryHubCollectorFilterAttribute(serviceLocator.GetService<ActivitySource>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryHubCollectorFilterAttribute : StreamingHubFilterAttribute
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryHubCollectorFilterAttribute(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.source = activitySource;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            using var activity = source.StartActivity($"{context.ServiceContext.MethodType}:/{context.Path}", ActivityKind.Server);

            try
            {
                activity.SetTag("grpc.method", context.ServiceContext.MethodType.ToString());
                activity.SetTag("rpc.system", "grpc");
                activity.SetTag("rpc.service", context.ServiceContext.ServiceType.Name);
                activity.SetTag("rpc.method", $"/{context.Path}");
                // todo: context.CallContext.Peer/Host format is https://github.com/grpc/grpc/blob/master/doc/naming.md and not uri standard.
                activity.SetTag("net.peer.ip", context.ServiceContext.CallContext.Peer);
                activity.SetTag("net.host.name", context.ServiceContext.CallContext.Host);
                activity.SetTag("message.type", "RECIEVED");
                activity.SetTag("message.id", context.ServiceContext.ContextId.ToString());
                activity.SetTag("message.uncompressed_size", context.Request.Length.ToString());

                // todo: net.peer.name not report on tracer. use custom tag
                activity.SetTag("magiconion.peer.ip", context.ServiceContext.CallContext.Peer);
                activity.SetTag("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                activity.SetTag("magiconion.auth.peer.authenticated", context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);

                activity.SetTag("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                activity.SetTag("exception", ex.ToString());
                activity.SetTag("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                activity.SetTag("grpc.status_detail", context.ServiceContext.CallContext.Status.Detail);
                activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode));
                throw;
            }
        }
   }
}