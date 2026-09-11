using Grpc.Core;
using MagicOnion.Server.Internal;
using NSubstitute;

namespace MagicOnion.Server.Tests;

/// <summary>
/// Tests response queue consumer completion.
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
        var stream = Substitute.For<IServerStreamWriter<int>>();
#pragma warning disable xUnit1051 // Configure the same WriteAsync overload used by the production consumer.
        stream.WriteAsync(1).Returns(_ =>
        {
            writeStarted.TrySetResult();
            return releaseWrite.Task;
        });
#pragma warning restore xUnit1051
        var context = Substitute.For<IServiceContextWithResponseStream<int>>();
        context.ResponseStream.Returns(stream);
        using var writer = new QueuedResponseWriter<int>(context);

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
        var context = Substitute.For<IServiceContextWithResponseStream<int>>();
        context.ResponseStream.Returns(Substitute.For<IServerStreamWriter<int>>());
        using var writer = new QueuedResponseWriter<int>(context);

        writer.Dispose();

        await writer.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
