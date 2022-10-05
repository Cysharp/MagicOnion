using Grpc.Core;
using MagicOnion.Server.Diagnostics;
using MessagePack;

namespace MagicOnion.Server;

public class ClientStreamingContext<TRequest, TResponse> : IAsyncStreamReader<TRequest>, IDisposable
{
    readonly StreamingServiceContext<TRequest, Nil /* Dummy */> context;
    readonly IAsyncStreamReader<TRequest> inner;
    readonly IMagicOnionLogger logger;

    internal ClientStreamingContext(StreamingServiceContext<TRequest, Nil /* Dummy */> context)
    {
        this.context = context;
        this.inner = context.RequestStream!;
        this.logger = context.MagicOnionLogger;
    }

    public ServiceContext ServiceContext => context;

    public TRequest Current { get; private set; } = default!; /* lateinit */

    public async Task<bool> MoveNext(CancellationToken cancellationToken = default(CancellationToken))
    {
        if (await inner.MoveNext(cancellationToken))
        {
            this.Current = inner.Current;
            return true;
        }
        else
        {
            logger.ReadFromStream(context, typeof(TRequest), true);
            return false;
        }
    }

    public void Dispose()
    {
        (inner as IDisposable)?.Dispose();
    }

    public async Task ForEachAsync(Action<TRequest> action)
    {
        while (await MoveNext(CancellationToken.None)) // ClientResponseStream is not supported CancellationToken.
        {
            action(Current);
        }
    }

    public async Task ForEachAsync(Func<TRequest, Task> asyncAction)
    {
        while (await MoveNext(CancellationToken.None))
        {
            await asyncAction(Current);
        }
    }

    public ClientStreamingResult<TRequest, TResponse> Result(TResponse result)
    {
        return new ClientStreamingResult<TRequest, TResponse>(result);
    }

    public ClientStreamingResult<TRequest, TResponse> ReturnStatus(StatusCode statusCode, string detail)
    {
        context.CallContext.Status = new Status(statusCode, detail);

        return default(ClientStreamingResult<TRequest, TResponse>); // dummy
    }
}
