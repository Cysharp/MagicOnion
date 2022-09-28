using Grpc.Core;

namespace MagicOnion;

public class ReturnStatusException : Exception
{
    public StatusCode StatusCode { get; }
    public string Detail { get; }

    public ReturnStatusException(StatusCode statusCode, string detail)
        : base($"The method has returned the status code '{statusCode}'." + (string.IsNullOrWhiteSpace(detail) ? "" : $" (Detail={detail})"))
    {
        this.StatusCode = statusCode;
        this.Detail = detail;
    }

    public Status ToStatus()
    {
        return new Status(StatusCode, Detail ?? "");
    }
}
