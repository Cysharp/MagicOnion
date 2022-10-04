using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace MagicOnion.Server;

// Grpc's ResponseStream(IAsyncStreamWriter) does not allow multithread call.
// IGroup is sometimes called from many caller(multithread) and invoke ResponseStream.Write
// So requires queueing.
internal class QueuedResponseWriter<T> : IDisposable
{
    IStreamingServiceContext serviceContext;
    Channel<T> channel;

    public QueuedResponseWriter(IStreamingServiceContext serviceContext)
    {
        this.serviceContext = serviceContext;
        channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        });

        ConsumeQueueAsync();
    }

    public void Write(in T value)
    {
        channel.Writer.TryWrite(value);
    }

    async void ConsumeQueueAsync()
    {
        var reader = channel.Reader;
        var stream = ((IServiceContextWithResponseStream<T>)serviceContext).ResponseStream!;
        do
        {
            while (reader.TryRead(out var item))
            {
                if (serviceContext.IsDisconnected) break;
                try
                {
                    await stream.WriteAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    MagicOnionServerInternalLogger.Current.LogError(ex, "error occurred on write to client.");
                }
            }
            if (serviceContext.IsDisconnected) break;
        } while (await reader.WaitToReadAsync().ConfigureAwait(false));

    }

    public void Dispose()
    {
        channel.Writer.TryComplete();
    }
}
