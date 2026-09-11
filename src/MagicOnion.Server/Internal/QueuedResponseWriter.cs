using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace MagicOnion.Server.Internal;

// Grpc's ResponseStream(IAsyncStreamWriter) does not allow multithread call.
// IGroup is sometimes called from many caller(multithread) and invoke ResponseStream.Write
// So requires queueing.
internal class QueuedResponseWriter<T> : IDisposable
{
    readonly IServerStreamWriter<T> responseStream;
    readonly Func<bool> isDisconnected;
    readonly ILogger logger;
    readonly Channel<T> channel;

    /// <summary>
    /// Gets a task that completes when the response queue consumer has stopped.
    /// </summary>
    public Task Completion { get; }

    /// <summary>
    /// Creates a queue that serializes writes to the response stream.
    /// </summary>
    /// <param name="responseStream">The destination for queued responses.</param>
    /// <param name="isDisconnected">A callback that reports the current disconnection state.</param>
    /// <param name="logger">The logger used to report response write failures.</param>
    public QueuedResponseWriter(IServerStreamWriter<T> responseStream, Func<bool> isDisconnected, ILogger logger)
    {
        this.responseStream = responseStream;
        this.isDisconnected = isDisconnected;
        this.logger = logger;
        channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        });

        Completion = ConsumeQueueAsync();
    }

    public void Write(in T value)
    {
        channel.Writer.TryWrite(value);
    }

    async Task ConsumeQueueAsync()
    {
        var reader = channel.Reader;
        do
        {
            while (reader.TryRead(out var item))
            {
                if (isDisconnected()) break;
                try
                {
                    await responseStream.WriteAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "error occurred on write to client.");
                }
            }
            if (isDisconnected()) break;
        } while (await reader.WaitToReadAsync().ConfigureAwait(false));

    }

    public void Dispose()
    {
        channel.Writer.TryComplete();
    }
}
