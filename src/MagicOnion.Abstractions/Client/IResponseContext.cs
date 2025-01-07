using Grpc.Core;

namespace MagicOnion.Client;

public interface IResponseContext : IDisposable
{
    Task<Metadata> ResponseHeadersAsync { get; }
    Status GetStatus();
    Metadata GetTrailers();
    Type ResponseType { get; }
}

public interface IResponseContext<T> : IResponseContext
{
    Task<T> ResponseAsync { get; }
}
