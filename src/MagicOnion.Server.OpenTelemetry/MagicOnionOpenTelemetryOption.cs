using System.Reflection;

namespace MagicOnion.Server.OpenTelemetry
{
    public class MagicOnionOpenTelemetryOptions
    {
        /// <summary>
        /// Tracer ServiceName use as ActivitySource
        /// </summary>
        public string MagicOnionActivityName { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
    }
}