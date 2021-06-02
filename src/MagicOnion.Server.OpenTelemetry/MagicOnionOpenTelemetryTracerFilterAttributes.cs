using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// Collect OpenTelemetry Tracer with Server filter (Unary).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class MagicOnionOpenTelemetryTracerFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            var activitySource = serviceProvider.GetService<MagicOnionActivitySources>();
            var options = serviceProvider.GetService<MagicOnionOpenTelemetryOptions>();
            return new MagicOnionOpenTelemetryTracerFilterAttribute(activitySource.Current, options);
        }
    }

    internal class MagicOnionOpenTelemetryTracerFilterAttribute : MagicOnionFilterAttribute
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions options;

        public MagicOnionOpenTelemetryTracerFilterAttribute(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.source = activitySource;
            this.options = telemetryOption;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            // short-circuit current activity
            if (!source.HasListeners())
            {
                await next(context);
                return;
            }

            var currentContext = Activity.Current?.Context;
            
            // Extract the SpanContext, if any from the headers
            var metadata = context.CallContext.RequestHeaders;
            if (metadata != null)
            {
                var propagationContext = Propagators.DefaultTextMapPropagator.Extract(currentContext, metadata);
                if (propagationContext.ActivityContext.IsValid())
                {
                    currentContext = propagationContext.ActivityContext;
                }
                if (propagationContext.Baggage != default)
                {
                    Baggage.Current = propagationContext.Baggage;
                }
            }

            // span name must be `$package.$service/$method` but MagicOnion has no $package.
            using var activity = source.StartActivity($"{context.MethodType}:{context.CallContext.Method}", ActivityKind.Server, currentContext ?? default);

            // activity may be null if "no one is listening" or "all listener returns ActivitySamplingResult.None in Sample or SampleUsingParentId callback".
            if (activity == null)
            {
                await next(context);
                return;
            }

            // todo: propagate で消せるはず?
            // add trace context to service context. 
            context.SetTraceContext(activity.Context);

            try
            {
                // tag spec: https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

                // application tags
                foreach (var tag in options.TracingTags)
                    activity.SetTag(tag.Key, tag.Value);

                // request
                activity.SetTag(SemanticConventions.AttributeRpcGrpcMethod, context.MethodType.ToString());
                activity.SetTag(SemanticConventions.AttributeRpcSystem, "grpc");
                activity.SetTag(SemanticConventions.AttributeRpcService, context.ServiceType.Name);
                activity.SetTag(SemanticConventions.AttributeRpcMethod, context.CallContext.Method);
                activity.SetTag(SemanticConventions.AttributeHttpHost, context.CallContext.Host);
                activity.SetTag(SemanticConventions.AttributeHttpUrl, context.CallContext.Host + context.CallContext.Method); 
                activity.SetTag(SemanticConventions.AttributeHttpUserAgent, context.CallContext.RequestHeaders.GetValue("user-agent"));
                activity.SetTag(SemanticConventions.AttributeMessageType, "RECIEVED");
                activity.SetTag(SemanticConventions.AttributeMessageId, context.ContextId.ToString());
                activity.SetTag(SemanticConventions.AttributeMessageUncompressedSize, context.GetRawRequest()?.LongLength.ToString() ?? "0");

                activity.SetTag(SemanticConventions.AttributeMagicOnionPeerName, context.CallContext.Peer);
                activity.SetTag(SemanticConventions.AttributeMagicOnionAuthEnabled, (!string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                activity.SetTag(SemanticConventions.AttributeMagicOnionAuthPeerAuthenticated, context.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);

                // response
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusCode, ((long)context.CallContext.Status.StatusCode).ToString());
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                activity.SetTag(SemanticConventions.AttributeException, ex.ToString());
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusCode, ((long)context.CallContext.Status.StatusCode).ToString());
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusDetail, context.CallContext.Status.Detail);
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(Grpc.Core.StatusCode.Internal));
                throw;
            }
        }
    }

    /// <summary>
    /// Collect OpenTelemetry Tracer with Server filter (StreamingHub).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class MagicOnionOpenTelemetryStreamingTracerFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public int Order { get; set; }

        StreamingHubFilterAttribute IMagicOnionFilterFactory<StreamingHubFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            var activitySource = serviceProvider.GetService<MagicOnionActivitySources>();
            var options = serviceProvider.GetService<MagicOnionOpenTelemetryOptions>();
            return new MagicOnionOpenTelemetryStreamingTracerFilterAttribute(activitySource.Current, options);
        }
    }

    internal class MagicOnionOpenTelemetryStreamingTracerFilterAttribute : StreamingHubFilterAttribute
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions options;

        public MagicOnionOpenTelemetryStreamingTracerFilterAttribute(ActivitySource activitySource, MagicOnionOpenTelemetryOptions telemetryOption)
        {
            this.source = activitySource;
            this.options = telemetryOption;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            // short-circuit current activity
            if (!source.HasListeners())
            {
                await next(context);
                return;
            }

            var currentContext = Activity.Current?.Context;

            // Extract the SpanContext, if any from the headers
            var metadata = context.ServiceContext.CallContext.RequestHeaders;
            if (metadata != null)
            {
                var propagationContext = Propagators.DefaultTextMapPropagator.Extract(currentContext, metadata);
                if (propagationContext.ActivityContext.IsValid())
                {
                    currentContext = propagationContext.ActivityContext;
                }
                if (propagationContext.Baggage != default)
                {
                    Baggage.Current = propagationContext.Baggage;
                }
            }

            using var activity = source.StartActivity($"{context.ServiceContext.MethodType}:/{context.Path}", ActivityKind.Server, currentContext ?? default);

            // activity may be null if "no one is listening" or "all listener returns ActivitySamplingResult.None in Sample or SampleUsingParentId callback".
            if (activity == null)
            {
                await next(context);
                return;
            }

            // todo: propagate で消せるはず?
            // add trace context to service context. 
            context.SetTraceContext(activity.Context);

            try
            {
                // tag spec: https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc

                // application tags
                foreach (var tag in options.TracingTags)
                    activity.SetTag(tag.Key, tag.Value);

                // request
                activity.SetTag(SemanticConventions.AttributeRpcGrpcMethod, context.ServiceContext.MethodType.ToString());
                activity.SetTag(SemanticConventions.AttributeRpcSystem, "grpc");
                activity.SetTag(SemanticConventions.AttributeRpcService, context.ServiceContext.ServiceType.Name);
                activity.SetTag(SemanticConventions.AttributeRpcMethod, $"/{context.Path}");
                activity.SetTag(SemanticConventions.AttributeHttpHost, context.ServiceContext.CallContext.Host);
                activity.SetTag(SemanticConventions.AttributeHttpUrl, context.ServiceContext.CallContext.Host + $"/{context.Path}");
                activity.SetTag(SemanticConventions.AttributeHttpUserAgent, context.ServiceContext.CallContext.RequestHeaders.GetValue("user-agent"));
                activity.SetTag(SemanticConventions.AttributeMessageType, "RECIEVED");
                activity.SetTag(SemanticConventions.AttributeMessageId, context.ServiceContext.ContextId.ToString());
                activity.SetTag(SemanticConventions.AttributeMessageUncompressedSize, context.Request.Length.ToString());

                activity.SetTag(SemanticConventions.AttributeMagicOnionPeerName, context.ServiceContext.CallContext.Peer);
                activity.SetTag(SemanticConventions.AttributeMagicOnionAuthEnabled, (!string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName)).ToString());
                activity.SetTag(SemanticConventions.AttributeMagicOnionAuthPeerAuthenticated, context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated.ToString());

                await next(context);

                // response
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusCode, ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.ServiceContext.CallContext.Status.StatusCode));
            }
            catch (Exception ex)
            {
                activity.SetTag(SemanticConventions.AttributeException, ex.ToString());
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusCode, ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString());
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusDetail, context.ServiceContext.CallContext.Status.Detail);
                activity.SetStatus(OpenTelemetryHelper.GrpcToOpenTelemetryStatus(Grpc.Core.StatusCode.Internal));
                throw;
            }
        }

        private static readonly Func<Metadata, string, IEnumerable<string>> MetadataGetter = (metadata, key) =>
        {
            for (var i = 0; i < metadata.Count; i++)
            {
                var entry = metadata[i];
                if (entry.Key.Equals(key))
                {
                    return new string[1] { entry.Value };
                }
            }

            return Enumerable.Empty<string>();
        };
    }
}