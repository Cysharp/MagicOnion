using MagicOnion.Server.OpenTelemetry;
using MagicOnion.Server.Hubs;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace MagicOnion.Server.OpenTelemetry
{
    public static class ServiceContextTelemetryExtensions
    {
        /// <summary>
        /// Set the trace context with this service context
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
            return (ActivityContext)context.Items[MagicOnionTelemetry.ServiceContextItemKeyTrace];
        }
    }

    public static class StreamingHubContextTelemetryExtensions
    {
        /// <summary>
        /// Set the trace context with this streaming hub context
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
            return (ActivityContext)context.Items[MagicOnionTelemetry.ServiceContextItemKeyTrace];
        }
    }
}