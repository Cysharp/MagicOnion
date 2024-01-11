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
        if (await reader.WaitToReadAsync(cancellationToken))
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
