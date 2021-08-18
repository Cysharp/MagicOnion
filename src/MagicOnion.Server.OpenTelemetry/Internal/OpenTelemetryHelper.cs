using Grpc.Core;

namespace MagicOnion.Server.OpenTelemetry.Internal
{
    internal static class OpenTelemetryHelper
    {
        /// <summary>
        /// Convert gRPC StatusCode to OpenTelemetry Status.
        /// </summary>
        /// <remarks>spec: https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/api.md#set-status</remarks>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static global::OpenTelemetry.Trace.Status GrpcToOpenTelemetryStatus(StatusCode code)
        {
            return code switch
            {
                StatusCode.OK => global::OpenTelemetry.Trace.Status.Ok,
                _ => global::OpenTelemetry.Trace.Status.Error,
            };
        }
    }
}