using MagicOnion.Server.Hubs;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using Grpc.Core;
using System.Collections.Generic;
using System.Linq;
using OpenTelemetry;

// ReSharper disable once CheckNamespace

namespace MagicOnion.Server.OpenTelemetry
{
    internal static class PropagatorExtensions
    {
        /// <summary>
        /// Injects the context into a carrier
        /// </summary>
        /// <param name="propagator"></param>
        /// <param name="context"></param>
        /// <param name="carrier"></param>
        public static void Inject(this TextMapPropagator propagator, PropagationContext context, CallOptions carrier)
        {
            static void SetMetadata(Metadata metadata, string key, string value) => metadata.Add(new Metadata.Entry(key, value));
            propagator.Inject(context, carrier.Headers, SetMetadata);
        }

        /// <summary>
        /// Extract the context from a carrier
        /// </summary>
        /// <param name="propagator"></param>
        /// <param name="activityContext"></param>
        /// <param name="carrier"></param>
        /// <returns></returns>
        public static PropagationContext Extract(this TextMapPropagator propagator, ActivityContext? activityContext, Metadata carrier)
        {
            static IEnumerable<string> GetMetadata(Metadata metadata, string key)
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
            }
            return propagator.Extract(new PropagationContext(activityContext ?? default, Baggage.Current), carrier, GetMetadata);
        }
    }

    public static class ServiceContextTelemetryExtensions
    {
        /// <summary>
        /// Set the trace context with this service context. This allows user to add their span directly to this context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activityContext"></param>
        internal static void SetTraceContext(this ServiceContext context, ActivityContext activityContext)
        {
            context.Items[MagicOnionTelemetry.ServiceContextItemKeyTrace] = activityContext;
        }

        /// <summary>
        /// Gets the trace context associated with this service context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ActivityContext GetTraceContext(this ServiceContext context)
        {
            if (context.Items.TryGetValue(MagicOnionTelemetry.ServiceContextItemKeyTrace, out var activityContext))
            {
                return (ActivityContext)activityContext;
            }
            return default;
        }
    }

    public static class StreamingHubContextTelemetryExtensions
    {
        /// <summary>
        /// Set the trace context with this streaming hub context. This allows user to add their span directly to this context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activityContext"></param>
        internal static void SetTraceContext(this StreamingHubContext context, ActivityContext activityContext)
        {
            context.Items[MagicOnionTelemetry.ServiceContextItemKeyTrace] = activityContext;
        }

        /// <summary>
        /// Gets the trace context associated with this streaming hub context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ActivityContext GetTraceContext(this StreamingHubContext context)
        {
            if (context.Items.TryGetValue(MagicOnionTelemetry.ServiceContextItemKeyTrace, out var activityContext))
            {
                return (ActivityContext)activityContext;
            }
            return default;
        }
    }
}