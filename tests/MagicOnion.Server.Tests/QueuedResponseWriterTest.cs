using Grpc.Core;
using MagicOnion.Server.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;

namespace MagicOnion.Server.Tests;

/// <summary>
/// Tests response queue completion, disconnection, and write failures.
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
        var stream = new ResponseStream(_ =>
        {
            writeStarted.TrySetResult();
            return releaseWrite.Task;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance);

        try
        {
            writer.Write(1);
            await writeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

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
    }

    /// <summary>
    /// Closing an empty queue must wake the consumer and complete its task.
    /// </summary>
    [Fact]
    public async Task Completion_DisposeWithoutMessages_Completes()
    {
        var stream = new ResponseStream(_ => Task.CompletedTask);
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, NullLogger.Instance);

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
        var stream = new ResponseStream(message =>
        {
            sent.Add(message);
            disconnected = true;
            return Task.CompletedTask;
        });
        using var writer = new QueuedResponseWriter<int>(stream, () => disconnected, NullLogger.Instance);

        writer.Write(1);
        writer.Write(2);
        writer.Dispose();
        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(new[] { 1 }, sent);
    }

    /// <summary>
    /// Write failures must be reported to the supplied logger without preventing subsequent writes.
    /// </summary>
    [Fact]
    public async Task Consumer_WriteFails_LogsErrorAndContinuesSending()
    {
        var error = new InvalidOperationException("Write failed.");
        var logger = new FakeLogger<QueuedResponseWriter<int>>();
        var attempted = new List<int>();
        var stream = new ResponseStream(message =>
        {
            attempted.Add(message);
            return message == 1 ? Task.FromException(error) : Task.CompletedTask;
        });
        using var writer = new QueuedResponseWriter<int>(stream, static () => false, logger);

        writer.Write(1);
        writer.Write(2);
        writer.Dispose();
        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(new[] { 1, 2 }, attempted);
        var log = Assert.Single(logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Same(error, log.Exception);
    }

    sealed class ResponseStream(Func<int, Task> write) : IServerStreamWriter<int>
    {
        public WriteOptions WriteOptions { get; set; }

        public Task WriteAsync(int message) => write(message);
    }
}
