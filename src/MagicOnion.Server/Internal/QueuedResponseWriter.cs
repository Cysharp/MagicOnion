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
    readonly Func<T, int> getSize;
    readonly Action onMaxLengthOrSizeExceeded;
    readonly int maxQueueLength;
    readonly long maxQueueSize;
    readonly Channel<(T Value, int Size)> channel;
    readonly object gate = new();
    int queuedCount;
    long queuedSize;
    bool completed;
    volatile bool overflowed;

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
    /// <param name="getSize">Gets an item's retained data size in bytes before ownership is transferred.</param>
    /// <param name="onMaxLengthOrSizeExceeded">Aborts the response stream when the maximum queue length or size is exceeded. Called once, outside the queue lock.</param>
    /// <param name="maxQueueLength">The maximum number of waiting items, or null for no limit.</param>
    /// <param name="maxQueueSize">The maximum total size of waiting items in bytes, or null for no limit.</param>
    public QueuedResponseWriter(IServerStreamWriter<T> responseStream, Func<bool> isDisconnected, ILogger logger, Action<T> onDiscard,
        Func<T, int> getSize, Action onMaxLengthOrSizeExceeded, int? maxQueueLength = null, long? maxQueueSize = null)
    {
        if (maxQueueLength is <= 0) throw new ArgumentOutOfRangeException(nameof(maxQueueLength));
        if (maxQueueSize is <= 0) throw new ArgumentOutOfRangeException(nameof(maxQueueSize));
        this.responseStream = responseStream;
        this.isDisconnected = isDisconnected;
        this.logger = logger;
        this.onDiscard = onDiscard;
        this.getSize = getSize;
        this.onMaxLengthOrSizeExceeded = onMaxLengthOrSizeExceeded;
        this.maxQueueLength = maxQueueLength ?? int.MaxValue;
        this.maxQueueSize = maxQueueSize ?? long.MaxValue;
        // Admission and accounting are atomic under gate, including when producers race.
        channel = Channel.CreateUnbounded<(T, int)>(new UnboundedChannelOptions
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
        var size = getSize(value);
        var notifyOverflow = false;
        lock (gate)
        {
            if (!completed)
            {
                if (queuedCount >= maxQueueLength || size > maxQueueSize - queuedSize)
                {
                    overflowed = true;
                    completed = true;
                    channel.Writer.TryComplete();
                    notifyOverflow = true;
                }
                else if (channel.Writer.TryWrite((value, size)))
                {
                    queuedCount++;
                    queuedSize += size;
                    return;
                }
            }
        }

        Discard(value);
        if (notifyOverflow)
        {
            onMaxLengthOrSizeExceeded();
            logger.LogWarning("The StreamingHub response queue limit was exceeded. Maximum length: {MaxQueueLength}, maximum size: {MaxQueueSize} bytes.",
                maxQueueLength, maxQueueSize);
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
                while (!isDisconnected() && TryRead(out var item))
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
                if (isDisconnected() || overflowed) break;
            } while (await reader.WaitToReadAsync().ConfigureAwait(false));
        }
        finally
        {
            // Reject further writes before draining. Only the consumer reads from the channel.
            Dispose();
            while (TryRead(out var item, discard: true))
            {
                Discard(item);
            }
        }
    }

    bool TryRead(out T item, bool discard = false)
    {
        lock (gate)
        {
            if ((discard || !overflowed) && channel.Reader.TryRead(out var entry))
            {
                queuedCount--;
                queuedSize -= entry.Size;
                // Cache sizes on enqueue: serialization may return the item to its pool.
                item = entry.Value;
                return true;
            }
        }
        item = default!;
        return false;
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
        lock (gate)
        {
            completed = true;
            channel.Writer.TryComplete();
        }
    }
}
