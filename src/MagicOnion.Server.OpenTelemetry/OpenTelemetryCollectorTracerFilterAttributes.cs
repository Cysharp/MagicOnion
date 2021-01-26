using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// Collect OpenTelemetry Tracer with Unary filter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryCollectorTracerFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            var activitySource = serviceProvider.GetService<MagicOnionActivitySources>();
            var options = serviceProvider.GetService<MagicOnionOpenTelemetryOptions>();
            return new OpenTelemetryCollectorTracerFilterAttribute(activitySource.Current, options);
        }
    }

    internal class OpenTelemetryCollectorTracerFilterAttribute : MagicOnionFilterAttribute
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryCollectorTracerFilterAttribute(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.source = activitySource;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            using var activity = source.StartActivity($"{context.MethodType}:{context.CallContext.Method}", ActivityKind.Server);

            // activity may be null if "no one is listening" or "all listener returns ActivitySamplingResult.None in Sample or SampleUsingParentId callback".
            if (activity == null)
            {
                await next(context);
                return;
            }

            // add trace context to service context. it allows user to add their span directly to this context.
            context.SetTraceContext(activity.Context);

            try
            {
                // request
                activity.SetTag("grpc.method", context.MethodType.ToString());
                activity.SetTag("rpc.system", "grpc");
                activity.SetTag("rpc.service", context.ServiceType.Name);
                activity.SetTag("rpc.method", context.CallContext.Method);
                activity.SetTag("net.peer.name", context.CallContext.Peer);
                activity.SetTag("http.host", context.CallContext.Host);
                activity.SetTag("http.useragent", context.CallContext.RequestHeaders.GetValue("user-agent"));
                activity.SetTag("message.type", "RECIEVED");
                activity.SetTag("message.id", context.ContextId.ToString());
                activity.SetTag("message.uncompressed_size", context.GetRawRequest()?.LongLength.ToString() ?? "0");

                activity.SetTag("magiconion.peer.ip", context.CallContext.Peer);
                activity.SetTag("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                activity.SetTag("magiconion.auth.peer.authenticated", context.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);
                // response
                activity.SetTag("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                activity.SetTag("exception", ex.ToString());
                activity.SetTag("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                activity.SetTag("grpc.status_detail", context.CallContext.Status.Detail);
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.CallContext.Status.StatusCode));
                throw;
            }
        }
    }

    /// <summary>
    /// Collect OpenTelemetry Tracer with StreamingHub Filter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class OpenTelemetryHubCollectorTracerFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public int Order { get; set; }

        StreamingHubFilterAttribute IMagicOnionFilterFactory<StreamingHubFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            var activitySource = serviceProvider.GetService<MagicOnionActivitySources>();
            var options = serviceProvider.GetService<MagicOnionOpenTelemetryOptions>();
            return new OpenTelemetryHubCollectorTracerFilterAttribute(activitySource.Current, options);
        }
    }

    internal class OpenTelemetryHubCollectorTracerFilterAttribute : StreamingHubFilterAttribute
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryHubCollectorTracerFilterAttribute(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.source = activitySource;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            using var activity = source.StartActivity($"{context.ServiceContext.MethodType}:/{context.Path}", ActivityKind.Server);

            // activity may be null if "no one is listening" or "all listener returns ActivitySamplingResult.None in Sample or SampleUsingParentId callback".
            if (activity == null)
            {
                await next(context);
                return;
            }

            // add trace context to service context. it allows user to add their span directly to this hub
            context.SetTraceContext(activity.Context);

            try
            {
                // request
                activity.SetTag("grpc.method", context.ServiceContext.MethodType.ToString());
                activity.SetTag("rpc.system", "grpc");
                activity.SetTag("rpc.service", context.ServiceContext.ServiceType.Name);
                activity.SetTag("rpc.method", $"/{context.Path}");
                activity.SetTag("net.peer.ip", context.ServiceContext.CallContext.Peer);
                activity.SetTag("http.host", context.ServiceContext.CallContext.Host);
                activity.SetTag("http.useragent", context.ServiceContext.CallContext.RequestHeaders.GetValue("user-agent"));
                activity.SetTag("message.type", "RECIEVED");
                activity.SetTag("message.id", context.ServiceContext.ContextId.ToString());
                activity.SetTag("message.uncompressed_size", context.Request.Length.ToString());

                activity.SetTag("magiconion.peer.ip", context.ServiceContext.CallContext.Peer);
                activity.SetTag("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                activity.SetTag("magiconion.auth.peer.authenticated", context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);
                // response
                activity.SetTag("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.ServiceContext.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                activity.SetTag("exception", ex.ToString());
                activity.SetTag("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                activity.SetTag("grpc.status_detail", context.ServiceContext.CallContext.Status.Detail);
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.ServiceContext.CallContext.Status.StatusCode));
                throw;
            }
        }
    }
}