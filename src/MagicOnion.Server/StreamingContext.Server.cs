using Grpc.Core;
using MagicOnion.Server.Diagnostics;
using MessagePack;

namespace MagicOnion.Server;


public class ServerStreamingContext<TResponse> : IAsyncStreamWriter<TResponse>
{
    readonly StreamingServiceContext<Nil /* Dummy */, TResponse> context;
    readonly IAsyncStreamWriter<TResponse> inner;
    readonly IMagicOnionLogger logger;

    internal ServerStreamingContext(StreamingServiceContext<Nil /* Dummy */, TResponse> context)
    {
        this.context = context;
        this.inner = context.ResponseStream!;
        this.logger = context.MagicOnionLogger;
    }

    public ServiceContext ServiceContext => context;

    public WriteOptions WriteOptions
    {
        get => inner.WriteOptions;
        set => inner.WriteOptions = value;
    }

    public Task WriteAsync(TResponse message)
    {
        logger.WriteToStream(context, typeof(TResponse));
        return inner.WriteAsync(message);
    }

    public ServerStreamingResult<TResponse> Result()
    {
        return default(ServerStreamingResult<TResponse>); // dummy
    }

    public ServerStreamingResult<TResponse> ReturnStatus(StatusCode statusCode, string detail)
    {
        context.CallContext.Status = new Status(statusCode, detail);

        return default(ServerStreamingResult<TResponse>); // dummy
    }
}
