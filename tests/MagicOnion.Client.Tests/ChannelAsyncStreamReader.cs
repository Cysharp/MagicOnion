using System.Threading.Channels;

namespace MagicOnion.Client.Tests;

class ChannelAsyncStreamReader<T> : IAsyncStreamReader<T>
{
    readonly ChannelReader<T> reader;

    public T Current { get; private set; } = default!;

    public ChannelAsyncStreamReader(Channel<T> channel)
    {
        reader = channel.Reader;
    }

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        try
        {
            if (await reader.WaitToReadAsync(cancellationToken))
            {
                if (reader.TryRead(out var item))
                {
                    Current = item;
                    return true;
                }
            }
        }
        catch (OperationCanceledException e)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, e.Message, e));
        }
        catch (Exception e)
        {
            throw new RpcException(new Status(StatusCode.Unknown, e.Message, e));
        }

        return false;
    }
}
