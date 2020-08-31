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
            return new OpenTelemetryCollectorFilterAttribute(serviceLocator.GetService<TracerProvider>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryCollectorFilterAttribute : MagicOnionFilterAttribute
    {
        readonly TracerProvider tracerProvider;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        public OpenTelemetryCollectorFilterAttribute(TracerProvider tracerProvider, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.tracerProvider = tracerProvider;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            // TracerName must match with ActivitySource name.
            var tracer = tracerProvider.GetTracer(telemetryOption.ActivitySourceName, telemetryOption.TracerVersion);

            // Client -> Server incoming filter
            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            using (var span = tracer.StartSpan($"{context.MethodType}:{context.CallContext.Method}", SpanKind.Server))
            {
                try
                {
                    span.SetAttribute("grpc.method", context.MethodType.ToString());
                    span.SetAttribute("rpc.system", "grpc");
                    span.SetAttribute("rpc.service", context.ServiceType.Name);
                    span.SetAttribute("rpc.method", context.CallContext.Method);
                    // todo: context.CallContext.Peer/Host format is https://github.com/grpc/grpc/blob/master/doc/naming.md and not uri standard.
                    span.SetAttribute("net.peer.name", context.CallContext.Peer);
                    span.SetAttribute("net.host.name", context.CallContext.Host);
                    span.SetAttribute("message.type", "RECIEVED");
                    span.SetAttribute("message.id", context.ContextId.ToString());
                    span.SetAttribute("message.uncompressed_size", context.GetRawRequest()?.LongLength.ToString() ?? "0");

                    // todo: net.peer.name not report on tracer. use custom tag
                    span.SetAttribute("magiconion.peer.ip", context.CallContext.Peer);
                    span.SetAttribute("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                    span.SetAttribute("magiconion.auth.peer.authenticated", context.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                    await next(context);

                    span.SetAttribute("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                    //span.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode));
                }
                catch (Exception ex)
                {
                    span.SetAttribute("exception", ex.ToString());
                    span.SetAttribute("grpc.status_code", ((long)context.CallContext.Status.StatusCode).ToString());
                    span.SetAttribute("grpc.status_detail", context.CallContext.Status.Detail);
                    //span.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.CallContext.Status.StatusCode));
                    throw;
                }
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
            return new OpenTelemetryHubCollectorFilterAttribute(serviceLocator.GetService<TracerProvider>(), serviceLocator.GetService<MagicOnionOpenTelemetryOptions>());
        }
    }

    internal class OpenTelemetryHubCollectorFilterAttribute : StreamingHubFilterAttribute
    {
        readonly TracerProvider tracerProvider;
        readonly MagicOnionOpenTelemetryOptions telemetryOption;

        private static readonly ConcurrentDictionary<Guid, SpanContext> _contextState = new ConcurrentDictionary<Guid, SpanContext>();

        public OpenTelemetryHubCollectorFilterAttribute(TracerProvider tracerProvider, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.tracerProvider = tracerProvider;
            this.telemetryOption = telemetryOption;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

            await NextTracer(context, next);
            //await NextSource(context, next);
        }

        private async ValueTask NextSource(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            using var source = new ActivitySource(telemetryOption.ActivitySourceName);
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

        private async ValueTask NextTracer(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            Console.WriteLine($"{context.ConnectionId} / {context.ServiceContext.ContextId} : {context.Path}");

            // TracerName must match with ActivitySource name.
            var tracer = tracerProvider.GetTracer(telemetryOption.ActivitySourceName, telemetryOption.TracerVersion);

            // Client -> Server incoming filter
            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            if (_contextState.TryGetValue(context.ServiceContext.ContextId, out var parentSpan))
            {
                using var span = tracer.StartSpan($"{context.ServiceContext.MethodType}:/{context.Path}", SpanKind.Server, parentSpan);
                await SpanExec(context, next, span);
            }
            else
            {
                using var span = tracer.StartSpan($"{context.ServiceContext.MethodType}:/{context.Path}", SpanKind.Server);
                //_contextState.TryAdd(context.ServiceContext.ContextId, span.Context);
                await SpanExec(context, next, span);
            }
        }

        private static async Task SpanExec(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next, TelemetrySpan span)
        {
            try
            {
                span.SetAttribute("grpc.method", context.ServiceContext.MethodType.ToString());
                span.SetAttribute("rpc.system", "grpc");
                span.SetAttribute("rpc.service", context.ServiceContext.ServiceType.Name);
                span.SetAttribute("rpc.method", $"/{context.Path}");
                // todo: context.CallContext.Peer/Host format is https://github.com/grpc/grpc/blob/master/doc/naming.md and not uri standard.
                span.SetAttribute("net.peer.ip", context.ServiceContext.CallContext.Peer);
                span.SetAttribute("net.host.name", context.ServiceContext.CallContext.Host);
                span.SetAttribute("message.type", "RECIEVED");
                span.SetAttribute("message.id", context.ServiceContext.ContextId.ToString());
                span.SetAttribute("message.uncompressed_size", context.Request.Length.ToString());

                // todo: net.peer.name not report on tracer. use custom tag
                span.SetAttribute("magiconion.peer.ip", context.ServiceContext.CallContext.Peer);
                span.SetAttribute("magiconion.auth.enabled", (!string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                span.SetAttribute("magiconion.auth.peer.authenticated", context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);

                span.SetAttribute("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                //span.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                span.SetAttribute("exception", ex.ToString());
                span.SetAttribute("grpc.status_code", ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                span.SetAttribute("grpc.status_detail", context.ServiceContext.CallContext.Status.Detail);
                //span.SetStatus(OpenTelemetrygRpcStatusHelper.ConvertStatus(context.ServiceContext.CallContext.Status.StatusCode));
                throw;
            }
            finally
            {
                //_contextState.TryRemove(context.ServiceContext.ContextId, out var _);
            }
        }
    }
}