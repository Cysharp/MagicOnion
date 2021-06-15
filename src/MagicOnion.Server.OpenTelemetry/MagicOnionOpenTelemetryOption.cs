using System.Collections.Generic;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// OpenTelemetry Options to inject Application Information
    /// </summary>
    public class MagicOnionOpenTelemetryOptions
    {
        /// <summary>
        /// ServiceName for Tracer. Especially Zipkin use service.name tag to identify service name.
        /// </summary>
        /// <remarks>input to tag `service.name`</remarks>
        public string ServiceName { get; set; }

        /// <summary>
        /// Expose RpsScope to the ServiceContext.Items. RpsScope key begin with .TraceContext
        /// </summary>
        public bool ExposeRpcScope { get; set; } = true;

        /// <summary>
        /// Application specific OpenTelemetry Tracing tags
        /// </summary>
        public Dictionary<string, string> TracingTags { get; set; } = new Dictionary<string, string>();
    }
}