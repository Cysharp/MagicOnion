using System.Threading.Channels;

namespace MagicOnion.Client.Tests;

class ChannelClientStreamWriter<T> : IClientStreamWriter<T>
{
    readonly ChannelWriter<T> writer;
    readonly CancellationTokenSource cts = new();

    public WriteOptions? WriteOptions { get; set; }

    public ChannelClientStreamWriter(ChannelWriter<T> writer)
    {
        this.writer = writer;
    }

    public Task CompleteAsync()
    {
        writer.Complete();
        cts.Cancel();
        return Task.CompletedTask;
    }

    public async Task WriteAsync(T message)
    {
        if (cts.IsCancellationRequested)
        {
            // Grpc.Net.Client.Internal.HttpContentClientStreamWriter throws a RpcException.
            throw new RpcException(new Status(StatusCode.Cancelled, "Call canceled by the client.", new OperationCanceledException("The operation was canceled.")));
        }

        writer.TryWrite(message);
        await Task.Delay(100);
        //return Task.CompletedTask;
    }
}
