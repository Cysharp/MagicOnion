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
            return new OpenTelemetryCollectorFilter(serviceLocator.GetService<ActivitySource>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryCollectorFilter : MagicOnionFilterAttribute
    {
        readonly ActivitySource activitySource;
        readonly string serviceName;

        public OpenTelemetryCollectorFilter(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.activitySource = activitySource;
            this.serviceName = telemetryOption.ServiceName;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // incoming kind: SERVER
            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            using (var activity = activitySource.StartActivity($"{context.CallContext.Method}", ActivityKind.Server))
            {
                try
                {
                    //todo: method 確認
                    activity.SetTag("grpc.method", context.CallContext.Method);
                    activity.SetTag("rpc.service", serviceName);
                    activity.SetTag("net.peer.ip", context.CallContext.Peer);
                    activity.SetTag("net.host.name", context.CallContext.Host);
                    activity.SetTag("message.type", "RECIEVED");
                    activity.SetTag("message.id", context.ContextId.ToString());
                    activity.SetTag("message.uncompressed_size", context.GetRawRequest()?.LongLength ?? 0);

                    activity.SetTag("magiconion.method.type", context.MethodType.ToString());
                    activity.SetTag("magiconion.service.type", context.ServiceType.Name);
                    activity.SetTag("magiconion.auth.enabled", !string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName));
                    activity.SetTag("magiconion.auth.peer.authenticated", context.CallContext.AuthContext.IsPeerAuthenticated);

                    await next(context);

                    activity.SetTag("grpc.status_code", (long)context.CallContext.Status.StatusCode);
                    activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode));
                }
                catch (Exception ex)
                {
                    activity.SetTag("exception", ex.ToString());
                    activity.SetTag("grpc.status_code", (long)context.CallContext.Status.StatusCode);
                    activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode));
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
            return new OpenTelemetryHubCollectorFilter(serviceLocator.GetService<ActivitySource>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryHubCollectorFilter : StreamingHubFilterAttribute
    {
        readonly ActivitySource activitySource;
        readonly string serviceName;

        public OpenTelemetryHubCollectorFilter(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.activitySource = activitySource;
            this.serviceName = telemetryOption.ServiceName;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // incoming kind: SERVER
            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            using (var activity = activitySource.StartActivity($"/{context.Path}", ActivityKind.Server))
            {
                try
                {
                    //todo: method 確認
                    activity.SetTag("grpc.method", context.ServiceContext.CallContext.Method);
                    activity.SetTag("rpc.service", serviceName);
                    activity.SetTag("net.peer.ip", context.ServiceContext.CallContext.Peer);
                    activity.SetTag("net.host.name", context.ServiceContext.CallContext.Host);
                    activity.SetTag("message.type", "RECIEVED");
                    activity.SetTag("message.id", context.ServiceContext.ContextId.ToString());
                    activity.SetTag("message.uncompressed_size", context.Request.Length);

                    activity.SetTag("magiconion.method.type", context.ServiceContext.MethodType.ToString());
                    activity.SetTag("magiconion.service.type", context.ServiceContext.ServiceType.Name);
                    activity.SetTag("magiconion.auth.enabled", !string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName));
                    activity.SetTag("magiconion.auth.peer.authenticated", context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated);

                    await next(context);

                    activity.SetTag("grpc.status_code", (long)context.ServiceContext.CallContext.Status.StatusCode);
                    activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode));
                }
                catch (Exception ex)
                {
                    activity.SetTag("exception", ex.ToString());
                    activity.SetTag("grpc.status_code", (long)context.ServiceContext.CallContext.Status.StatusCode);
                    activity.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode));
                    throw;
                }
            }
        }
    }
}