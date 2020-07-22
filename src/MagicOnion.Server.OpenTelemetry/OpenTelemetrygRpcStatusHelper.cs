using Grpc.Core;

namespace MagicOnion.Server.OpenTelemetry
{
    public static class OpenTelemetrygRpcStatusHelper
    {
        // gRPC StatusCode and OpenTelemetry.CanonicalCode is same.
        public static global::OpenTelemetry.Trace.Status ConvertStatus(StatusCode code)
        {
            switch (code)
            {
                case StatusCode.OK:
                    return global::OpenTelemetry.Trace.Status.Ok;
                case StatusCode.Cancelled:
                    return global::OpenTelemetry.Trace.Status.Cancelled;
                case StatusCode.Unknown:
                    return global::OpenTelemetry.Trace.Status.Unknown;
                case StatusCode.InvalidArgument:
                    return global::OpenTelemetry.Trace.Status.InvalidArgument;
                case StatusCode.DeadlineExceeded:
                    return global::OpenTelemetry.Trace.Status.DeadlineExceeded;
                case StatusCode.NotFound:
                    return global::OpenTelemetry.Trace.Status.NotFound;
                case StatusCode.AlreadyExists:
                    return global::OpenTelemetry.Trace.Status.AlreadyExists;
                case StatusCode.PermissionDenied:
                    return global::OpenTelemetry.Trace.Status.PermissionDenied;
                case StatusCode.Unauthenticated:
                    return global::OpenTelemetry.Trace.Status.Unauthenticated;
                case StatusCode.ResourceExhausted:
                    return global::OpenTelemetry.Trace.Status.ResourceExhausted;
                case StatusCode.FailedPrecondition:
                    return global::OpenTelemetry.Trace.Status.FailedPrecondition;
                case StatusCode.Aborted:
                    return global::OpenTelemetry.Trace.Status.Aborted;
                case StatusCode.OutOfRange:
                    return global::OpenTelemetry.Trace.Status.OutOfRange;
                case StatusCode.Unimplemented:
                    return global::OpenTelemetry.Trace.Status.Unimplemented;
                case StatusCode.Internal:
                    return global::OpenTelemetry.Trace.Status.Internal;
                case StatusCode.Unavailable:
                    return global::OpenTelemetry.Trace.Status.Unavailable;
                case StatusCode.DataLoss:
                    return global::OpenTelemetry.Trace.Status.DataLoss;
                default:
                    // custom status code? use Unknown.
                    return global::OpenTelemetry.Trace.Status.Unknown;
            }
        }
    }
}