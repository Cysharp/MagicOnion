using System.Buffers;
using System.Diagnostics;
using System.Threading.Channels;
using MagicOnion.Client.Internal;
using MagicOnion.Internal;
using Microsoft.Extensions.Time.Testing;

namespace MagicOnion.Client.Tests;

public class StreamingHubClientHeartbeatManagerTest
{
    [Fact]
    public async Task Interval_TimeoutDisabled()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });
        var interval = TimeSpan.FromSeconds(1);
        var timeout = Timeout.InfiniteTimeSpan;
        var serverHeartbeatReceived = new List<ReadOnlyMemory<byte>>();
        var clientHeartbeatResponseReceived = new List<ClientHeartbeatEvent>();
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        await using var manager = new StreamingHubClientHeartbeatManager(
            channel.Writer,
            interval,
            timeout,
            onServerHeartbeatReceived: x => serverHeartbeatReceived.Add(x.Metadata.ToArray()),
            onClientHeartbeatResponseReceived: x => clientHeartbeatResponseReceived.Add(x),
            synchronizationContext: null,
            timeProvider
        );

        // Act
        manager.StartClientHeartbeatLoop();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(10, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(channel.Reader.TryRead(out var heartbeat1));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(TimeSpan.FromSeconds(1)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat1.Memory.ToArray());
        Assert.True(channel.Reader.TryRead(out var heartbeat2));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x1 /* Sequence(1) */, .. ToMessagePackBytes(TimeSpan.FromSeconds(2)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat2.Memory.ToArray());
        Assert.True(channel.Reader.TryRead(out var heartbeat3));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x2 /* Sequence(2) */, .. ToMessagePackBytes(TimeSpan.FromSeconds(3)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat3.Memory.ToArray());

        Assert.False(manager.TimeoutToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Elapsed_RoundTripTime()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });
        var interval = TimeSpan.FromSeconds(1);
        var timeout = Timeout.InfiniteTimeSpan;
        var serverHeartbeatReceived = new List<ReadOnlyMemory<byte>>();
        var clientHeartbeatResponseReceived = new List<ClientHeartbeatEvent>();
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        await using var manager = new StreamingHubClientHeartbeatManager(
            channel.Writer,
            interval,
            timeout,
            onServerHeartbeatReceived: x => serverHeartbeatReceived.Add(x.Metadata.ToArray()),
            onClientHeartbeatResponseReceived: x => clientHeartbeatResponseReceived.Add(x),
            synchronizationContext: null,
            timeProvider
        );

        // Act
        manager.StartClientHeartbeatLoop();
        timeProvider.Advance(TimeSpan.FromMilliseconds(1000)); // Send
        await Task.Delay(10, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(1000)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));

        timeProvider.Advance(TimeSpan.FromMilliseconds(900)); // Send
        await Task.Delay(10, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x1 /* Sequence(1) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(2000)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));

        timeProvider.Advance(TimeSpan.FromMilliseconds(900)); // Send
        await Task.Delay(10, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x2 /* Sequence(2) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(3000)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));

        await Task.Delay(10, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, clientHeartbeatResponseReceived.Count);
        Assert.True(channel.Reader.TryRead(out var heartbeat1));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(1000)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat1.Memory.ToArray());
        Assert.Equal(TimeSpan.FromMilliseconds(100), clientHeartbeatResponseReceived[0].RoundTripTime);
        Assert.True(channel.Reader.TryRead(out var heartbeat2));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x1 /* Sequence(1) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(2000)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat2.Memory.ToArray());
        Assert.Equal(TimeSpan.FromMilliseconds(100), clientHeartbeatResponseReceived[1].RoundTripTime);
        Assert.True(channel.Reader.TryRead(out var heartbeat3));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x2 /* Sequence(2) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(3000)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat3.Memory.ToArray());
        Assert.Equal(TimeSpan.FromMilliseconds(100), clientHeartbeatResponseReceived[2].RoundTripTime);

        Assert.False(manager.TimeoutToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Timeout_Not_Responding()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });
        var interval = TimeSpan.FromSeconds(1);
        var timeout = TimeSpan.FromMilliseconds(500);
        var serverHeartbeatReceived = new List<ReadOnlyMemory<byte>>();
        var clientHeartbeatResponseReceived = new List<ClientHeartbeatEvent>();
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        await using var manager = new StreamingHubClientHeartbeatManager(
            channel.Writer,
            interval,
            timeout,
            onServerHeartbeatReceived: x => serverHeartbeatReceived.Add(x.Metadata.ToArray()),
            onClientHeartbeatResponseReceived: x => clientHeartbeatResponseReceived.Add(x),
            synchronizationContext: null,
            timeProvider
        );

        // Act
        manager.StartClientHeartbeatLoop();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(10, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(channel.Reader.TryRead(out var heartbeat1));
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(TimeSpan.FromSeconds(1)) /* ClientSentAt */, 0xc0 /* Nil */], heartbeat1.Memory.ToArray());
        Assert.False(channel.Reader.TryRead(out var heartbeat2));

        Assert.True(manager.TimeoutToken.IsCancellationRequested);
    }


    [Fact]
    public async Task Timeout_Keep()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });
        var interval = TimeSpan.FromSeconds(1);
        var timeout = TimeSpan.FromMilliseconds(500);
        var serverHeartbeatReceived = new List<ReadOnlyMemory<byte>>();
        var clientHeartbeatResponseReceived = new List<ClientHeartbeatEvent>();
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        await using var manager = new StreamingHubClientHeartbeatManager(
            channel.Writer,
            interval,
            timeout,
            onServerHeartbeatReceived: x => serverHeartbeatReceived.Add(x.Metadata.ToArray()),
            onClientHeartbeatResponseReceived: x => clientHeartbeatResponseReceived.Add(x),
            synchronizationContext: null,
            timeProvider
        );

        // Act && Assert
        manager.StartClientHeartbeatLoop();

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // Send a client heartbeat message
        await Task.Delay(10, TestContext.Current.CancellationToken);

        Assert.True(channel.Reader.TryRead(out var heartbeat1));

        timeProvider.Advance(TimeSpan.FromMilliseconds(250));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        // Received a response message from the server.
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(origin.AddSeconds(1)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));

        timeProvider.Advance(TimeSpan.FromMilliseconds(250));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromMilliseconds(500));
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        Assert.True(channel.Reader.TryRead(out var heartbeat2));
    }


    [Fact]
    public async Task Timeout_IntervalLongerThanTimeout_Not_Responding()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });
        var interval = TimeSpan.FromSeconds(1);
        var timeout = TimeSpan.FromSeconds(3);
        var serverHeartbeatReceived = new List<ReadOnlyMemory<byte>>();
        var clientHeartbeatResponseReceived = new List<ClientHeartbeatEvent>();
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        await using var manager = new StreamingHubClientHeartbeatManager(
            channel.Writer,
            interval,
            timeout,
            onServerHeartbeatReceived: x => serverHeartbeatReceived.Add(x.Metadata.ToArray()),
            onClientHeartbeatResponseReceived: x => clientHeartbeatResponseReceived.Add(x),
            synchronizationContext: null,
            timeProvider
        );

        // Act & Assert
        manager.StartClientHeartbeatLoop();
        timeProvider.Advance(TimeSpan.FromSeconds(1)); // Send a client heartbeat message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // 1s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // 2s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromMilliseconds(900)); // 2.9s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        // Respond to the first message. but it does not respond to subsequent messages.
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(origin.AddSeconds(1)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));

        timeProvider.Advance(TimeSpan.FromMilliseconds(100)); // 3s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.True(manager.TimeoutToken.IsCancellationRequested);
    }


    [Fact]
    public async Task Timeout_IntervalLongerThanTimeout_Keep()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });
        var interval = TimeSpan.FromSeconds(1);
        var timeout = TimeSpan.FromSeconds(3);
        var serverHeartbeatReceived = new List<ReadOnlyMemory<byte>>();
        var clientHeartbeatResponseReceived = new List<ClientHeartbeatEvent>();
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        await using var manager = new StreamingHubClientHeartbeatManager(
            channel.Writer,
            interval,
            timeout,
            onServerHeartbeatReceived: x => serverHeartbeatReceived.Add(x.Metadata.ToArray()),
            onClientHeartbeatResponseReceived: x => clientHeartbeatResponseReceived.Add(x),
            synchronizationContext: null,
            timeProvider
        );

        // Act & Assert
        manager.StartClientHeartbeatLoop();
        timeProvider.Advance(TimeSpan.FromSeconds(1)); // Send a client heartbeat message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // 1s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // 2s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromMilliseconds(900)); // 2.9s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);

        // Respond to the first message. but it does not respond to subsequent messages.
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x0 /* Sequence(0) */, .. ToMessagePackBytes(origin.AddSeconds(1)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x1 /* Sequence(1) */, .. ToMessagePackBytes(origin.AddSeconds(2)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));
        manager.ProcessClientHeartbeatResponse(StreamingHubPayloadPool.Shared.RentOrCreate([0x95 /* Array(5) */, 0x7e /* 0x7e(127) */, 0x2 /* Sequence(2) */, .. ToMessagePackBytes(origin.AddSeconds(3)) /* ClientSentAt */, 0xc0 /* Nil */, 0xc0 /* Nil */]));

        timeProvider.Advance(TimeSpan.FromMilliseconds(100)); // 3s has elapsed since the first message.
        await Task.Delay(10, TestContext.Current.CancellationToken);
        Assert.False(manager.TimeoutToken.IsCancellationRequested);
    }

    static byte[] ToMessagePackBytes(DateTimeOffset dt)
    {
        var ms = dt.ToUnixTimeMilliseconds();

        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);
        writer.Write(ms);
        writer.Flush();
        return arrayBufferWriter.WrittenMemory.ToArray();
    }

    static byte[] ToMessagePackBytes(TimeSpan ts)
    {
        var ms = (long)ts.TotalMilliseconds;

        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);
        writer.Write(ms);
        writer.Flush();
        return arrayBufferWriter.WrittenMemory.ToArray();
    }
}
