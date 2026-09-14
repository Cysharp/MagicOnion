using System.Buffers;
using System.Threading.Channels;
using Grpc.Core;
using MagicOnion.Internal;
using MessagePack;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public partial class StreamingHubPayloadReturnTest
{
    // Small configured budgets keep the overflow tests independent of production defaults.
    // Count and bytes measure retained payloads; the fake serializes the in-flight response
    // before blocking, just as a transport can stall after the marshaller returns its buffer.
    const int ResponseQueueCountBudget = 16;
    const long ResponseQueueByteBudget = 64 * 1024;
    static readonly TimeSpan ResponseQueueTestTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// A stopped response stream must not retain more heartbeat responses than the count budget.
    /// </summary>
    [Fact]
    public async Task ResponseQueue_BlockedWriter_EnforcesCountBudgetAndDisconnects()
    {
        await using var harness = new ResponseQueueHarness();
        await harness.StartBlockedWriteAsync();

        // At most 18 tiny messages including the first, already serialized response.
        await harness.SendUntilDisconnectedAsync(ResponseQueueCountBudget + 1, extraSize: 0);

        var pending = harness.Inner.OutstandingPayloads;
        Assert.True(pending.Count <= ResponseQueueCountBudget,
            $"The stopped writer retained {pending.Count} heartbeat responses ({pending.Bytes} bytes); the count budget is {ResponseQueueCountBudget}.");
        Assert.True(harness.Inner.Context.IsDisconnected, "Exceeding the response count budget must disconnect the slow client.");
        harness.Inner.RequestLifetime.Received(1).Abort();
        await harness.Inner.Context.ResponseWriterCompletion.WaitAsync(ResponseQueueTestTimeout, TestContext.Current.CancellationToken);
        harness.Inner.AssertAllReturnedOnce();
    }

    /// <summary>
    /// A few larger heartbeat responses must trigger the byte budget independently of the count budget.
    /// </summary>
    [Fact]
    public async Task ResponseQueue_BlockedWriter_EnforcesByteBudgetAndDisconnects()
    {
        await using var harness = new ResponseQueueHarness();
        await harness.StartBlockedWriteAsync();

        // Three 24 KiB extras exceed 64 KiB while remaining well below the count budget.
        await harness.SendUntilDisconnectedAsync(3, extraSize: 24 * 1024);

        var pending = harness.Inner.OutstandingPayloads;
        Assert.True(pending.Count <= ResponseQueueCountBudget);
        Assert.True(pending.Bytes <= ResponseQueueByteBudget,
            $"The stopped writer retained {pending.Bytes} bytes in {pending.Count} heartbeat responses; the byte budget is {ResponseQueueByteBudget}.");
        Assert.True(harness.Inner.Context.IsDisconnected, "Exceeding the response byte budget must disconnect the slow client.");
        harness.Inner.RequestLifetime.Received(1).Abort();
        await harness.Inner.Context.ResponseWriterCompletion.WaitAsync(ResponseQueueTestTimeout, TestContext.Current.CancellationToken);
        harness.Inner.AssertAllReturnedOnce();
    }

    /// <summary>
    /// Disconnecting with queued heartbeat responses must return every input and output buffer once.
    /// </summary>
    [Fact]
    public async Task ResponseQueue_DisconnectWithPendingHeartbeats_ReturnsAllPayloadsOnce()
    {
        await using var harness = new ResponseQueueHarness();
        await harness.StartBlockedWriteAsync();
        await harness.SendUntilDisconnectedAsync(4, extraSize: 128);
        Assert.Equal(4, harness.Inner.OutstandingPayloads.Count);

        await harness.DisconnectAsync();

        Assert.Equal(0, harness.Inner.OutstandingPayloads.Count);
        harness.Inner.AssertAllReturnedOnce();
    }

    /// <summary>
    /// A producer racing with disconnection must return an output rejected by the completed queue.
    /// </summary>
    [Fact]
    public async Task ResponseQueue_WriteAfterDisconnect_ReturnsRejectedPayloadOnce()
    {
        await using var harness = new ResponseQueueHarness();
        await harness.StartBlockedWriteAsync();
        await harness.DisconnectAsync();
        var response = harness.Inner.Rent([0x95, 0x7e, 0x01, 0x02, 0xc0, 0xc0]);

        harness.Inner.Context.QueueResponseStreamWrite(response);

        harness.Inner.AssertReturnedOnce(response);
        harness.Inner.AssertAllReturnedOnce();
    }

    /// <summary>
    /// A burst below both budgets must preserve heartbeat responses and release their buffers when sending resumes.
    /// </summary>
    [Fact]
    public async Task ResponseQueue_BelowBudgets_ResumesInOrderAndReturnsAllPayloadsOnce()
    {
        await using var harness = new ResponseQueueHarness();
        await harness.StartBlockedWriteAsync();
        await harness.SendUntilDisconnectedAsync(3, extraSize: 128);
        Assert.False(harness.Inner.Context.IsDisconnected);
        Assert.Equal(3, harness.Inner.OutstandingPayloads.Count);

        harness.Stream.Release();
        for (short sequence = 0; sequence < 4; sequence++)
        {
            var response = await harness.Stream.Responses.Reader.ReadAsync(TestContext.Current.CancellationToken)
                .AsTask().WaitAsync(ResponseQueueTestTimeout, TestContext.Current.CancellationToken);
            var reader = new StreamingHubClientMessageReader(response);
            Assert.Equal(StreamingHubMessageType.ClientHeartbeatResponse, reader.ReadMessageType());
            Assert.Equal((sequence, 2L), reader.ReadClientHeartbeatResponse());

            // Preserve the current echo behavior, including its reserved fields.
            var expected = new ArrayBufferWriter<byte>();
            StreamingHubMessageWriter.WriteClientHeartbeatMessageResponse(expected, sequence, 2);
            var requestReader = new StreamingHubServerMessageReader(BuildHeartbeat(sequence, sequence == 0 ? 0 : 128));
            Assert.Equal(StreamingHubMessageType.ClientHeartbeat, requestReader.ReadMessageType());
            expected.Write(requestReader.ReadClientHeartbeat().Extra.Span);
            Assert.Equal(expected.WrittenSpan.ToArray(), response);
        }

        await harness.DisconnectAsync();
        harness.Inner.AssertAllReturnedOnce();
    }

    static byte[] BuildHeartbeat(short sequence, int extraSize)
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(4);
        writer.Write(0x7e);
        writer.Write(sequence);
        writer.Write(2L);
        if (extraSize == 0)
        {
            writer.WriteNil();
        }
        else
        {
            writer.Write(new byte[extraSize]);
        }
        writer.Flush();
        return buffer.WrittenSpan.ToArray();
    }

    sealed class ResponseQueueHarness : IAsyncDisposable
    {
        short sequence;
        bool disconnected;

        public BlockingResponseStream Stream { get; } = new();
        public Harness Inner { get; }

        public ResponseQueueHarness()
        {
            // The existing harness uses NopStreamingHubHeartbeatManager: queue protection
            // must work with server heartbeats and their timeout disabled.
            Inner = new Harness(Stream, new MagicOnionOptions
            {
                StreamingHubResponseQueueMaxLength = ResponseQueueCountBudget,
                StreamingHubResponseQueueMaxSize = ResponseQueueByteBudget,
            });
            // Model the transport releasing its pending write when the request is aborted.
            Inner.RequestLifetime.When(x => x.Abort()).Do(_ => Stream.Release());
        }

        public async Task StartBlockedWriteAsync()
        {
            await SendHeartbeatAsync(extraSize: 0);
            await Stream.FirstWriteStarted.Task.WaitAsync(ResponseQueueTestTimeout, TestContext.Current.CancellationToken);
        }

        public async Task SendUntilDisconnectedAsync(int count, int extraSize)
        {
            for (var i = 0; i < count && !Inner.Context.IsDisconnected; i++)
            {
                await SendHeartbeatAsync(extraSize);
            }
        }

        async Task SendHeartbeatAsync(int extraSize)
        {
            var payload = Inner.Rent(BuildHeartbeat(sequence++, extraSize));
            await Inner.ProcessAsync(payload, TestContext.Current.CancellationToken)
                .AsTask().WaitAsync(ResponseQueueTestTimeout, TestContext.Current.CancellationToken);
            Inner.AssertReturnedOnce(payload);
        }

        public async Task DisconnectAsync()
        {
            if (disconnected) return;
            if (!Inner.Context.IsDisconnected) Inner.Context.CompleteStreamingHub();
            Stream.Release();
            // Cleanup must also run if the test cancellation token has been canceled.
            await Inner.Context.ResponseWriterCompletion.WaitAsync(ResponseQueueTestTimeout);
            disconnected = true;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            // TrackingPool.Dispose releases unreturned buffers after the consumer stops.
            // Assertions above therefore observe production behavior before fallback cleanup.
            await Inner.DisposeAsync();
        }
    }

    sealed class BlockingResponseStream : IServerStreamWriter<StreamingHubPayload>
    {
        readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public WriteOptions WriteOptions { get; set; }
        public TaskCompletionSource FirstWriteStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Channel<byte[]> Responses { get; } = Channel.CreateUnbounded<byte[]>();

        public Task WriteAsync(StreamingHubPayload message)
        {
            var context = new RecordingSerializationContext();
            MagicOnionMarshallers.StreamingHubMarshaller.ContextualSerializer(message, context);
            Responses.Writer.TryWrite(context.Buffer.WrittenSpan.ToArray());
            FirstWriteStarted.TrySetResult();
            return release.Task;
        }

        public void Release() => release.TrySetResult();
    }

    sealed class RecordingSerializationContext : SerializationContext
    {
        public ArrayBufferWriter<byte> Buffer { get; } = new();

        public override IBufferWriter<byte> GetBufferWriter() => Buffer;
        public override void SetPayloadLength(int payloadLength) { }
        public override void Complete() { }
    }
}
