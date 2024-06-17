using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Grpc.Core;

namespace Microbenchmark.Client;

class ChannelAsyncStreamReader<T> : IAsyncStreamReader<T>
{
    readonly ChannelReader<T> reader;

    public T Current { get; private set; } = default!;

    public ChannelAsyncStreamReader(Channel<T> channel)
    {
        reader = channel.Reader;
    }

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return MoveNextCore(cancellationToken).AsTask();
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    async ValueTask<bool> MoveNextCore(CancellationToken cancellationToken)
    {
        if (await reader.WaitToReadAsync())
        {
            if (reader.TryRead(out var item))
            {
                Current = item;
                return true;
            }
        }

        return false;
    }
}
