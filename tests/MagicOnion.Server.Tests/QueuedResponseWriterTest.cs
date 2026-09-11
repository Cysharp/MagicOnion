using System.Collections.Concurrent;
using Grpc.Core;
using MagicOnion.Server.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;

namespace MagicOnion.Server.Tests;

/// <summary>
/// Tests response queue completion, disconnection, write failures, and ownership of unsent items.
/// </summary>
public class QueuedResponseWriterTest
{
    /// <summary>
    /// Closing the queue must not signal completion before an in-flight write has finished.
    /// </summary>
    [Fact]
    public async Task Completion_WaitsForInFlightWriteAfterDispose()
    {
        var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWrite = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            writeStarted.TrySetResult();
            return releaseWrite.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, discarded.Add, static _ => 0, static () => { });

        try
        {
            writer.Write(1);
            await writeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            writer.Write(2);

            writer.Dispose();

            Assert.False(writer.Completion.IsCompleted);
        }
        finally
        {
            writer.Dispose();
            releaseWrite.TrySetResult();
            // Finish cleanup even when the test cancellation token has been canceled.
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(new[] { 1, 2 }, sent);
        Assert.Empty(discarded);
    }

    /// <summary>
    /// Closing an empty queue must wake the consumer and complete its task.
    /// </summary>
    [Fact]
    public async Task Completion_DisposeWithoutMessages_Completes()
    {
        var stream = new ResponseStream(_ => Task.CompletedTask);
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, static _ => { }, static _ => 0, static () => { });

        writer.Dispose();

        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// The consumer must observe a change in the disconnection state and stop sending queued messages.
    /// </summary>
    [Fact]
    public async Task Consumer_DisconnectedAfterWrite_StopsSending()
    {
        var disconnected = false;
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            disconnected = true;
            return Task.CompletedTask;
        });
        using var writer = new QueuedResponseWriter<int>(stream, () => disconnected, NullLogger.Instance, discarded.Add, static _ => 0, static () => { });

        writer.Write(1);
        writer.Write(2);
        writer.Dispose();
        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(new[] { 1 }, sent);
        Assert.Equal(new[] { 2 }, discarded);
    }

    /// <summary>
    /// Write failures must be reported to the supplied logger without preventing subsequent writes.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Consumer_WriteFails_LogsErrorAndContinuesSending(bool synchronousFailure)
    {
        var error = new InvalidOperationException("Write failed.");
        var logger = new FakeLogger<QueuedResponseWriter<int>>();
        var attempted = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var stream = new ResponseStream(message =>
        {
            attempted.Add(message);
            if (message != 1) return Task.CompletedTask;
            if (synchronousFailure) throw error;
            return Task.FromException(error);
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, logger, discarded.Add, static _ => 0, static () => { });

        writer.Write(1);
        writer.Write(2);
        writer.Dispose();
        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(new[] { 1, 2 }, attempted);
        // Invocation transfers ownership even if the transport fails before returning a task.
        Assert.Empty(discarded);
        var log = Assert.Single(logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Same(error, log.Exception);
    }

    /// <summary>
    /// An item rejected by a closed queue must be discarded once by the producer.
    /// </summary>
    [Fact]
    public async Task Write_AfterDispose_DiscardsRejectedItemOnce()
    {
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            return Task.CompletedTask;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, discarded.Add, static _ => 0, static () => { });
        writer.Dispose();
        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        writer.Write(1);
        writer.Dispose();

        Assert.Equal(new[] { 1 }, discarded);
        Assert.Empty(sent);
    }

    /// <summary>
    /// Disconnection must release pending and rejected items without discarding the in-flight item.
    /// </summary>
    [Fact]
    public async Task Consumer_DisconnectedDuringWrite_DiscardsPendingItemsOnce()
    {
        var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWrite = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var disconnected = false;
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            writeStarted.TrySetResult();
            return releaseWrite.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, () => disconnected, NullLogger.Instance, discarded.Add, static _ => 0, static () => { });

        try
        {
            writer.Write(1);
            await writeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            writer.Write(2);
            writer.Write(3);
            disconnected = true;
            writer.Dispose();
            writer.Write(4);

            Assert.False(writer.Completion.IsCompleted);
            Assert.Equal(new[] { 4 }, discarded);
        }
        finally
        {
            disconnected = true;
            writer.Dispose();
            releaseWrite.TrySetResult();
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(new[] { 1 }, sent);
        Assert.Equal(new[] { 2, 3, 4 }, discarded.Order());
    }

    /// <summary>
    /// One failing discard callback must not prevent cleanup of the remaining queued items.
    /// </summary>
    [Fact]
    public async Task Consumer_DiscardFails_ContinuesCleanup()
    {
        var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWrite = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var disconnected = false;
        var attempted = new ConcurrentBag<int>();
        var error = new InvalidOperationException("Discard failed.");
        var logger = new FakeLogger<QueuedResponseWriter<int>>();
        var stream = new ResponseStream(_ =>
        {
            writeStarted.TrySetResult();
            return releaseWrite.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, () => disconnected, logger, message =>
        {
            attempted.Add(message);
            if (message == 1) throw error;
        }, static _ => 0, static () => { });

        try
        {
            writer.Write(0);
            await writeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            writer.Write(1);
            writer.Write(2);
        }
        finally
        {
            disconnected = true;
            writer.Dispose();
            releaseWrite.TrySetResult();
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(new[] { 1, 2 }, attempted.Order());
        var log = Assert.Single(logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Same(error, log.Exception);
    }

    /// <summary>
    /// Producers racing with queue closure must discard every unsent item exactly once.
    /// </summary>
    [Fact]
    public async Task Write_RacesWithDispose_DiscardsEachUnsentItemOnce()
    {
        var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWrite = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var startRace = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var disconnected = false;
        var discarded = new ConcurrentBag<int>();
        var stream = new ResponseStream(_ =>
        {
            writeStarted.TrySetResult();
            return releaseWrite.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, () => disconnected, NullLogger.Instance, discarded.Add, static _ => 0, static () => { });

        try
        {
            writer.Write(0);
            await writeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            var writes = Enumerable.Range(1, 16).Select(async message =>
            {
                await startRace.Task;
                writer.Write(message);
            });
            var producers = Task.WhenAll(writes.Append(CloseAsync()));
            startRace.SetResult();
            await producers.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }
        finally
        {
            disconnected = true;
            writer.Dispose();
            releaseWrite.TrySetResult();
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(Enumerable.Range(1, 16), discarded.Order());

        async Task CloseAsync()
        {
            await startRace.Task;
            disconnected = true;
            writer.Dispose();
        }
    }

    /// <summary>
    /// Each limit accepts its exact boundary and closes the queue on the next oversized admission.
    /// </summary>
    [Theory]
    [InlineData(2, null)]
    [InlineData(null, 5L)]
    [InlineData(2, 5L)]
    public async Task Write_ExceedsLimit_StopsSendingAndDiscardsAllUnsentItems(int? maxLength, long? maxSize)
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var overflows = 0;
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            started.TrySetResult();
            return release.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, discarded.Add,
            static value => value, () => Interlocked.Increment(ref overflows), maxLength, maxSize);

        try
        {
            writer.Write(0);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            writer.Write(2);
            writer.Write(3);
            Assert.Equal(0, overflows);
            Assert.Empty(discarded);

            writer.Write(4);
            writer.Write(5);
            Assert.Equal(1, overflows);
            Assert.Equal(new[] { 4, 5 }, discarded.Order());
        }
        finally
        {
            writer.Dispose();
            release.TrySetResult();
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(new[] { 0 }, sent);
        Assert.Equal(new[] { 2, 3, 4, 5 }, discarded.Order());
    }

    /// <summary>
    /// A single payload larger than the size limit is rejected even when the queue is empty.
    /// </summary>
    [Fact]
    public async Task Write_SingleOversizedItem_AbortsWithoutSending()
    {
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var overflows = 0;
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            return Task.CompletedTask;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, discarded.Add,
            static value => value, () => Interlocked.Increment(ref overflows), maxQueueSize: 5);

        writer.Write(6);
        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(1, overflows);
        Assert.Empty(sent);
        Assert.Equal(new[] { 6 }, discarded);
    }

    /// <summary>
    /// Dequeuing releases both budgets before awaiting the transport, allowing sustained sending.
    /// </summary>
    [Fact]
    public async Task Consumer_Dequeue_ReleasesCapacityForLaterResponses()
    {
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sent = new List<int>();
        var discarded = new ConcurrentBag<int>();
        var overflows = 0;
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            if (message == 1)
            {
                firstStarted.TrySetResult();
                return releaseFirst.Task;
            }
            if (message == 2)
            {
                secondStarted.TrySetResult();
                return releaseSecond.Task;
            }
            return Task.CompletedTask;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, discarded.Add,
            static _ => 3, () => Interlocked.Increment(ref overflows), maxQueueLength: 1, maxQueueSize: 3);

        try
        {
            writer.Write(1);
            await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            writer.Write(2);
            releaseFirst.TrySetResult();
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            writer.Write(3);
        }
        finally
        {
            writer.Dispose();
            releaseFirst.TrySetResult();
            releaseSecond.TrySetResult();
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(0, overflows);
        Assert.Empty(discarded);
        Assert.Equal(new[] { 1, 2, 3 }, sent);
    }

    /// <summary>
    /// Concurrent producers cannot overbook either budget or notify overflow more than once.
    /// </summary>
    [Theory]
    [InlineData(8, null)]
    [InlineData(null, 8L)]
    public async Task Write_ConcurrentOverflow_EnforcesLimitAndDiscardsEachItemOnce(int? maxLength, long? maxSize)
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var startRace = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var discarded = new ConcurrentBag<int>();
        var overflows = 0;
        var stream = new ResponseStream(_ =>
        {
            started.TrySetResult();
            return release.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance, discarded.Add,
            static _ => 1, () => Interlocked.Increment(ref overflows), maxLength, maxSize);

        try
        {
            writer.Write(0);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            var producers = Task.WhenAll(Enumerable.Range(1, 64).Select(async message =>
            {
                await startRace.Task;
                writer.Write(message);
            }));
            startRace.SetResult();
            await producers.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(1, overflows);
            Assert.Equal(64 - 8, discarded.Count);
        }
        finally
        {
            writer.Dispose();
            release.TrySetResult();
            await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        Assert.Equal(Enumerable.Range(1, 64), discarded.Order());
    }

    sealed class ResponseStream(Func<int, Task> write) : IServerStreamWriter<int>
    {
        public WriteOptions WriteOptions { get; set; }

        public Task WriteAsync(int message) => write(message);
    }
}
