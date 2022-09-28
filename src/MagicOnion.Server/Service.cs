using Grpc.Core;
using MessagePack;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server;

public abstract class ServiceBase<TServiceInterface> : IService<TServiceInterface>
    where TServiceInterface : IServiceMarker
{
    public ServiceContext Context { get; set; }

    public ServiceBase()
    {
        this.Context = default!;
    }

    internal ServiceBase(ServiceContext context)
    {
        this.Context = context;
    }

    // Helpers

    protected UnaryResult<TResponse> UnaryResult<TResponse>(TResponse result)
    {
        return new MagicOnion.UnaryResult<TResponse>(result);
    }

    protected UnaryResult<Nil> ReturnNil()
    {
        return new MagicOnion.UnaryResult<Nil>(Nil.Default);
    }

    protected TResponse ReturnStatusCode<TResponse>(int statusCode, string detail)
    {
        Context.CallContext.Status = new Status((StatusCode)statusCode, detail ?? "");
#pragma warning disable CS8603 // Possible null reference return.
        return default;
#pragma warning restore CS8603 // Possible null reference return.
    }

    protected UnaryResult<TResponse> ReturnStatus<TResponse>(StatusCode statusCode, string detail)
    {
        Context.CallContext.Status = new Status(statusCode, detail ?? "");
        return default(UnaryResult<TResponse>); // dummy
    }

    [Ignore]
    public ClientStreamingContext<TRequest, TResponse> GetClientStreamingContext<TRequest, TResponse>()
    {
        return new ClientStreamingContext<TRequest, TResponse>(Context);
    }

    [Ignore]
    public ServerStreamingContext<TResponse> GetServerStreamingContext<TResponse>()
    {
        return new ServerStreamingContext<TResponse>(Context);
    }

    [Ignore]
    public DuplexStreamingContext<TRequest, TResponse> GetDuplexStreamingContext<TRequest, TResponse>()
    {
        return new DuplexStreamingContext<TRequest, TResponse>(Context);
    }

    // Interface methods for Client

    TServiceInterface IService<TServiceInterface>.WithOptions(CallOptions option)
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }

    TServiceInterface IService<TServiceInterface>.WithHeaders(Metadata headers)
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }

    TServiceInterface IService<TServiceInterface>.WithDeadline(DateTime deadline)
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }

    TServiceInterface IService<TServiceInterface>.WithCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }

    TServiceInterface IService<TServiceInterface>.WithHost(string host)
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }
}
