using MagicOnion.Server.Hubs;
using MagicOnion.Server.OpenTelemetry.Internal;

namespace MagicOnion.Server.OpenTelemetry
{
    public static class ServiceContextTelemetryExtensions
    {
        /// <summary>
        /// Set the trace scope with this service context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scope"></param>
        internal static void SetTraceScope(this StreamingHubContext context, IRpcScope scope)
        {
            context.ServiceContext.Items[MagicOnionTelemetryConstants.ServiceContextItemKeyTrace + "." +  context.Path] = scope;
        }

        /// <summary>
        /// Set the trace scope with this service context. This allows user to add their tag directly to this activity.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scope"></param>
        internal static void SetTraceScope(this ServiceContext context, IRpcScope scope)
        {
            context.Items[MagicOnionTelemetryConstants.ServiceContextItemKeyTrace] = scope;
        }

        /// <summary>
        /// Gets the trace scope associated with this service context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IRpcScope GetTraceScope(this ServiceContext context)
        {
            if (context.Items.TryGetValue(MagicOnionTelemetryConstants.ServiceContextItemKeyTrace, out var scope))
            {
                return (IRpcScope)scope;
            }
            return default;
        }
        /// <summary>
        /// Gets the trace scope associated with this service context.
        /// </summary>
        /// <remarks>Add custom tag directly to this activity.</remarks>
        /// <param name="context"></param>
        /// <param name="hubPath">IHubClass/MethodName</param>
        /// <returns></returns>
        public static IRpcScope GetTraceScope(this ServiceContext context, string hubPath)
        {
            if (context.Items.TryGetValue(MagicOnionTelemetryConstants.ServiceContextItemKeyTrace + "." + hubPath, out var scope))
            {
                return (IRpcScope)scope;
            }
            return default;
        }
    }
}
