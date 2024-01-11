using System.Threading.Channels;

namespace MagicOnion.Client.Tests;

class ChannelClientStreamWriter<T> : IClientStreamWriter<T>
{
    readonly ChannelWriter<T> writer;

    public WriteOptions? WriteOptions { get; set; }

    public ChannelClientStreamWriter(ChannelWriter<T> writer)
    {
        this.writer = writer;
    }

    public Task CompleteAsync()
    {
        writer.Complete();
        return Task.CompletedTask;
    }

    public Task WriteAsync(T message)
    {
        writer.TryWrite(message);
        return Task.CompletedTask;
    }
}
