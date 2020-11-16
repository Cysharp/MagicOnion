using Grpc.Core;

namespace MagicOnion.Server.OpenTelemetry
{
    public static class OpenTelemetryHelper
    {
        /// <summary>
        /// Convert gRPC StatusCode to OpenTelemetry Status.
        /// </summary>
        /// <remarks>spec: https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/api.md#set-status</remarks>
        /// <param name="code"></param>
        /// <returns></returns>
        public static global::OpenTelemetry.Trace.Status GrpcToOpenTelemetryStatus(StatusCode code)
        {
            switch (code)
            {
                case StatusCode.OK:
                    return global::OpenTelemetry.Trace.Status.Ok;
                default:
                    return global::OpenTelemetry.Trace.Status.Error;
            }
        }
    }
}