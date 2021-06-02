using System;
using System.Collections.Generic;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// OpenTelemetry Options to inject Application Information
    /// </summary>
    public class MagicOnionOpenTelemetryOptions
    {
        /// <summary>
        /// Application specific OpenTelemetry Tracing tags
        /// </summary>
        public Dictionary<string, string> TracingTags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Expose RpsScope to the ServiceContext.Items. RpsScope key begin with .TraceContext
        /// </summary>
        public bool ExposeRpcScope { get; set; } = true;
    }
}