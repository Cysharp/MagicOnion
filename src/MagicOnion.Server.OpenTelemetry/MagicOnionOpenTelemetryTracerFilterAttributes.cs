using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.OpenTelemetry.Internal;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
            using var rpcScope = new ServerRpcScope(context.ServiceType.Name, context.CallContext.Method.TrimStart('/'), context.CallContext, source, options);

            if (options.ExposeRpcScope)
            {
                context.SetTraceScope(rpcScope);
            }

            rpcScope.SetTags(options.TracingTags);
            rpcScope.SetTags(new Dictionary<string, string>
            {
                { SemanticConventions.AttributeRpcGrpcMethod, context.MethodType.ToString() },
                { SemanticConventions.AttributeHttpHost, context.CallContext.Host},
                { SemanticConventions.AttributeHttpUrl, context.CallContext.Host + context.CallContext.Method },
                { SemanticConventions.AttributeHttpUserAgent, context.CallContext.RequestHeaders.GetValue("user-agent")},
                { SemanticConventions.AttributeMessageId, context.ContextId.ToString()},
                { SemanticConventions.AttributeMessageUncompressedSize, context.GetRawRequest()?.LongLength.ToString() ?? "0"},
                { SemanticConventions.AttributeMagicOnionPeerName, context.CallContext.Peer},
                { SemanticConventions.AttributeMagicOnionAuthEnabled, (!string.IsNullOrEmpty(context.CallContext.AuthContext.PeerIdentityPropertyName)).ToString()},
                { SemanticConventions.AttributeMagicOnionAuthPeerAuthenticated, context.CallContext.AuthContext.IsPeerAuthenticated.ToString()},
            });

            try
            {
                await next(context);

                OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.CallContext.Status.StatusCode);
                rpcScope.Complete(context.CallContext.Status.StatusCode);
            }
            catch (Exception ex)
            {
                rpcScope.SetTags(new Dictionary<string, string>
                {
                    { SemanticConventions.AttributeRpcGrpcStatusCode, ((long)context.CallContext.Status.StatusCode).ToString()},
                    { SemanticConventions.AttributeRpcGrpcStatusDetail, context.CallContext.Status.Detail},
                });
                rpcScope.CompleteWithException(ex);
                throw;
            }
            finally
            {
                rpcScope.RestoreParentActivity();
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
            using var rpcScope = new ServerRpcScope(context.ServiceContext.ServiceType.Name, context.Path, context.ServiceContext.CallContext, source, options);

            if (options.ExposeRpcScope)
            {
                context.SetTraceScope(rpcScope);
            }

            rpcScope.SetTags(options.TracingTags);
            rpcScope.SetTags(new Dictionary<string, string>
            {
                { SemanticConventions.AttributeRpcGrpcMethod, context.ServiceContext.MethodType.ToString() },
                { SemanticConventions.AttributeHttpHost, context.ServiceContext.CallContext.Host},
                { SemanticConventions.AttributeHttpUrl, context.ServiceContext.CallContext.Host + "/" + context.Path },
                { SemanticConventions.AttributeHttpUserAgent, context.ServiceContext.CallContext.RequestHeaders.GetValue("user-agent")},
                { SemanticConventions.AttributeMessageId, context.ServiceContext.ContextId.ToString()},
                { SemanticConventions.AttributeMessageUncompressedSize, context.Request.Length.ToString()},
                { SemanticConventions.AttributeMagicOnionPeerName, context.ServiceContext.CallContext.Peer},
                { SemanticConventions.AttributeMagicOnionAuthEnabled, (!string.IsNullOrEmpty(context.ServiceContext.CallContext.AuthContext.PeerIdentityPropertyName)).ToString()},
                { SemanticConventions.AttributeMagicOnionAuthPeerAuthenticated, context.ServiceContext.CallContext.AuthContext.IsPeerAuthenticated.ToString()},
            });

            try
            {
                await next(context);

                OpenTelemetryHelper.GrpcToOpenTelemetryStatus(context.ServiceContext.CallContext.Status.StatusCode);
                rpcScope.Complete(context.ServiceContext.CallContext.Status.StatusCode);
            }
            catch (Exception ex)
            {
                rpcScope.SetTags(new Dictionary<string, string>
                    {
                        { SemanticConventions.AttributeRpcGrpcStatusCode, ((long)context.ServiceContext.CallContext.Status.StatusCode).ToString()},
                        { SemanticConventions.AttributeRpcGrpcStatusDetail, context.ServiceContext.CallContext.Status.Detail},
                    });
                rpcScope.CompleteWithException(ex);
                throw;
            }
            finally
            {
                rpcScope.RestoreParentActivity();
            }
        }
    }

    internal class ServerRpcScope : RpcScope
    {
        public ServerRpcScope(string rpcService, string rpcMethod, ServerCallContext context, ActivitySource source, MagicOnionOpenTelemetryOptions options) 
            : base(rpcService, rpcMethod, options.ServiceName)
        {
            // activity may be null if "no one is listening" or "all listener returns ActivitySamplingResult.None in Sample or SampleUsingParentId callback".
            if (!source.HasListeners())
                return;

            var currentContext = Activity.Current?.Context;

            // Extract the SpanContext, if any from the headers
            var metadata = context.RequestHeaders;
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

            // span name should be `$package.$service/$method` but MagicOnion has no $package.
            var rpcActivity = source.StartActivity(
                rpcMethod,
                ActivityKind.Server,
                currentContext ?? default);

            SetActivity(rpcActivity);
        }

        /// <summary>
        /// Restores the parent activity.
        /// </summary>
        public void RestoreParentActivity()
        {
            Activity.Current = this.ParentActivity;
        }
    }
}