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
    readonly Action<T> onDiscard;
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
    /// <param name="onDiscard">
    /// Releases items that are never passed to the response stream. This callback may run concurrently
    /// on producer and consumer threads. Failures are logged without retrying the callback.
    /// </param>
    public QueuedResponseWriter(IServerStreamWriter<T> responseStream, Func<bool> isDisconnected, ILogger logger, Action<T> onDiscard)
    {
        this.responseStream = responseStream;
        this.isDisconnected = isDisconnected;
        this.logger = logger;
        this.onDiscard = onDiscard;
        channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        });

        Completion = ConsumeQueueAsync();
    }

    /// <summary>
    /// Transfers ownership of an item to the queue, discarding it if the queue rejects it.
    /// </summary>
    /// <remarks>
    /// Once WriteAsync is invoked, the response stream owns the item, even if the write fails.
    /// The queue does not discard items that may already have been released by serialization.
    /// </remarks>
    public void Write(in T value)
    {
        if (!channel.Writer.TryWrite(value))
        {
            Discard(value);
        }
    }

    async Task ConsumeQueueAsync()
    {
        var reader = channel.Reader;
        try
        {
            do
            {
                // Check before removing an item so that unsent items remain available for cleanup.
                while (!isDisconnected() && reader.TryRead(out var item))
                {
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
        finally
        {
            // Reject further writes before draining. Only the consumer reads from the channel.
            channel.Writer.TryComplete();
            while (reader.TryRead(out var item))
            {
                Discard(item);
            }
        }
    }

    void Discard(T item)
    {
        try
        {
            onDiscard(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error occurred on discarding a response.");
        }
    }

    /// <summary>
    /// Stops accepting new items without interrupting an in-flight write.
    /// </summary>
    /// <remarks>
    /// Pending items are sent while connected or discarded when the consumer observes disconnection.
    /// Await <see cref="Completion"/> to wait for the consumer and its cleanup to finish.
    /// </remarks>
    public void Dispose()
    {
        channel.Writer.TryComplete();
    }
}
