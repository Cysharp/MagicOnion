using System;
using System.Buffers;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server.Hubs;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public class StreamingHubHeartbeatManagerTest
{
    [Fact]
    public void Register()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act
        var handle = manager.Register(context);

        // Assert
        Assert.NotNull(handle);
        Assert.Equal(context, handle.ServiceContext);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);
    }

    [Fact]
    public void Handle_Dispose()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();
        var handle = manager.Register(context);

        // Act
        handle.Dispose();
    }

    [Fact]
    public async Task Latency()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act
        using var handle = manager.Register(context);
        timeProvider.Advance(TimeSpan.FromMilliseconds(350));
        await Task.Delay(16);

        // Simulate to send heartbeat responses from clients.
        timeProvider.Advance(TimeSpan.FromMilliseconds(50));
        handle.Ack(0);
        await Task.Delay(16);

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(50), handle.Latency);
    }

    [Fact]
    public void Latency_Before_Send()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act
        using var handle = manager.Register(context);

        // Assert
        Assert.Equal(TimeSpan.Zero, handle.Latency);
    }

    [Fact]
    public async Task Latency_Before_Ack()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act
        using var handle = manager.Register(context);
        timeProvider.Advance(TimeSpan.FromMilliseconds(350));
        await Task.Delay(16);

        // Assert
        Assert.Equal(TimeSpan.Zero, handle.Latency);
    }

    [Fact]
    public async Task Interval_Disable_Timeout()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan, null, timeProvider, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();
        byte[] expectedHeartbeatMessageNoExtra1 = BuildMessage(0, timeProvider.GetUtcNow().AddMilliseconds(100));
        byte[] expectedHeartbeatMessageNoExtra2 = BuildMessage(1, timeProvider.GetUtcNow().AddMilliseconds(200));
        byte[] expectedHeartbeatMessageNoExtra3 = BuildMessage(2, timeProvider.GetUtcNow().AddMilliseconds(300));

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);

        // Assert
        Assert.Equal(3, context1.Responses.Count);
        Assert.Equal(expectedHeartbeatMessageNoExtra1, context1.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra2, context1.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra3, context1.Responses[2].Memory.ToArray());
        Assert.Equal(3, context2.Responses.Count);
        Assert.Equal(expectedHeartbeatMessageNoExtra1, context2.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra2, context2.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra3, context2.Responses[2].Memory.ToArray());
        Assert.Equal(3, context3.Responses.Count);
        Assert.Equal(expectedHeartbeatMessageNoExtra1, context3.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra2, context3.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra3, context3.Responses[2].Memory.ToArray());
    }

    [Fact]
    public async Task Interval_Keep()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        timeProvider.Advance(TimeSpan.FromMilliseconds(350));
        await Task.Delay(16);
        var isCanceled1 = handle1.TimeoutToken.IsCancellationRequested;
        var isCanceled2 = handle2.TimeoutToken.IsCancellationRequested;
        var isCanceled3 = handle3.TimeoutToken.IsCancellationRequested;
        // Simulate to send heartbeat responses from clients.
        handle1.Ack(0);
        handle2.Ack(0);
        handle3.Ack(0);
        timeProvider.Advance(TimeSpan.FromMilliseconds(250));
        await Task.Delay(16);

        // Assert
        Assert.False(isCanceled1);
        Assert.False(isCanceled2);
        Assert.False(isCanceled3);
        Assert.False(handle1.TimeoutToken.IsCancellationRequested);
        Assert.False(handle2.TimeoutToken.IsCancellationRequested);
        Assert.False(handle3.TimeoutToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Interval_With_Timeout()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        timeProvider.Advance(TimeSpan.FromMilliseconds(350));
        await Task.Delay(16);
        var isCanceled1 = handle1.TimeoutToken.IsCancellationRequested;
        var isCanceled2 = handle2.TimeoutToken.IsCancellationRequested;
        var isCanceled3 = handle3.TimeoutToken.IsCancellationRequested;
        timeProvider.Advance(TimeSpan.FromMilliseconds(250)); // No responses from clients and timeouts are reached.
        await Task.Delay(16);

        // Assert
        Assert.False(isCanceled1);
        Assert.False(isCanceled2);
        Assert.False(isCanceled3);
        Assert.True(handle1.TimeoutToken.IsCancellationRequested);
        Assert.True(handle2.TimeoutToken.IsCancellationRequested);
        Assert.True(handle3.TimeoutToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Interval_Stop_After_HandleDisposed()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan, null, timeProvider, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();
        byte[] expectedHeartbeatMessageNoExtra1 = BuildMessage(0, timeProvider.GetUtcNow().AddMilliseconds(100));
        byte[] expectedHeartbeatMessageNoExtra2 = BuildMessage(1, timeProvider.GetUtcNow().AddMilliseconds(200));
        byte[] expectedHeartbeatMessageNoExtra3 = BuildMessage(2, timeProvider.GetUtcNow().AddMilliseconds(300));

        // Act
        var handle1 = manager.Register(context1);
        var handle2 = manager.Register(context2);
        var handle3 = manager.Register(context3);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        handle1.Dispose();
        handle2.Dispose();
        handle3.Dispose();
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);

        // Assert
        Assert.Equal(3, context1.Responses.Count);
        Assert.Equal(expectedHeartbeatMessageNoExtra1, context1.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra2, context1.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra3, context1.Responses[2].Memory.ToArray());
        Assert.Equal(3, context2.Responses.Count);
        Assert.Equal(expectedHeartbeatMessageNoExtra1, context2.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra2, context2.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra3, context2.Responses[2].Memory.ToArray());
        Assert.Equal(3, context3.Responses.Count);
        Assert.Equal(expectedHeartbeatMessageNoExtra1, context3.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra2, context3.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessageNoExtra3, context3.Responses[2].Memory.ToArray());
    }

    [Fact]
    public async Task CustomMetadataProvider()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan, new CustomHeartbeatMetadataProvider(), timeProvider, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();
        byte[] expectedHeartbeatMessage1 = [.. BuildMessageHeader(0, timeProvider.GetUtcNow().AddMilliseconds(100)), .. "Hello"u8];
        byte[] expectedHeartbeatMessage2 = [.. BuildMessageHeader(1, timeProvider.GetUtcNow().AddMilliseconds(200)), .. "Hello"u8];
        byte[] expectedHeartbeatMessage3 = [.. BuildMessageHeader(2, timeProvider.GetUtcNow().AddMilliseconds(300)), .. "Hello"u8];

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);

        // Assert
        Assert.Equal(3, context1.Responses.Count);
        Assert.Equal(expectedHeartbeatMessage1, context1.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage2, context1.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage3, context1.Responses[2].Memory.ToArray());
        Assert.Equal(3, context2.Responses.Count);
        Assert.Equal(expectedHeartbeatMessage1, context2.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage2, context2.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage3, context2.Responses[2].Memory.ToArray());
        Assert.Equal(3, context3.Responses.Count);
        Assert.Equal(expectedHeartbeatMessage1, context3.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage2, context3.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage3, context3.Responses[2].Memory.ToArray());
    }

    [Fact]
    public async Task Timeout_Longer_Than_Interval_Keep()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act & Assert
        using var handle = manager.Register(context);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);
        Assert.Single(context.Responses);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);
        Assert.Equal(2, context.Responses.Count);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromMilliseconds(900));
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);

        handle.Ack(0);
        handle.Ack(1);
        handle.Ack(2);

        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);
        Assert.Equal(4, context.Responses.Count);
    }

    [Fact]
    public async Task Timeout_Longer_Than_Interval_Lost()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act & Assert
        using var handle = manager.Register(context);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);
        Assert.Single(context.Responses);

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // 1s has elapsed since the first message.
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);
        Assert.Equal(2, context.Responses.Count);

        timeProvider.Advance(TimeSpan.FromSeconds(1)); // 2s has elapsed since the first message.
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);

        timeProvider.Advance(TimeSpan.FromMilliseconds(900)); // 2.9s has elapsed since the first message.
        await Task.Delay(16);
        Assert.False(handle.TimeoutToken.IsCancellationRequested);

        // Only returns a response to the first message.
        handle.Ack(0);
        //handle.Ack(1);
        //handle.Ack(2);

        timeProvider.Advance(TimeSpan.FromMilliseconds(100)); // 3s has elapsed since the first message.
        await Task.Delay(16);
        Assert.True(handle.TimeoutToken.IsCancellationRequested); // The client should be disconnected.
        Assert.Equal(4, context.Responses.Count);
    }

    [Fact]
    public async Task Sequence()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context1 = CreateFakeStreamingServiceContext();

        // Act
        using var handle1 = manager.Register(context1);
        timeProvider.Advance(TimeSpan.FromMilliseconds(350));
        await Task.Delay(16);
        // Simulate to send heartbeat responses from clients.
        handle1.Ack(0);

        timeProvider.Advance(TimeSpan.FromMilliseconds(350));
        await Task.Delay(16);
        handle1.Ack(1);

        // Assert
        Assert.Equal(BuildMessage(0, origin.AddMilliseconds(350)), context1.Responses[0].Memory.ToArray());
        Assert.Equal(BuildMessage(1, origin.AddMilliseconds(700)), context1.Responses[1].Memory.ToArray());
    }

    [Fact]
    public void Writer_WriteServerHeartbeatMessageHeader()
    {
        // Arrange
        var bufferWriter1 = new ArrayBufferWriter<byte>();
        var bufferWriter2 = new ArrayBufferWriter<byte>();

        // Act
        StreamingHubMessageWriter.WriteServerHeartbeatMessageHeader(bufferWriter1, 0, DateTimeOffset.FromUnixTimeMilliseconds(123456));
        StreamingHubMessageWriter.WriteServerHeartbeatMessageHeader(bufferWriter2, 1, DateTimeOffset.FromUnixTimeMilliseconds(456789));

        // Assert
        Assert.Equal(BuildMessageHeader(0, DateTimeOffset.FromUnixTimeMilliseconds(123456)), bufferWriter1.WrittenSpan.ToArray());
        Assert.Equal(BuildMessageHeader(1, DateTimeOffset.FromUnixTimeMilliseconds(456789)), bufferWriter2.WrittenSpan.ToArray());
    }

    [Fact]
    public async Task AckCallback()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var timeProvider = new FakeTimeProvider();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, timeProvider, logger);
        var context = CreateFakeStreamingServiceContext();

        // Act
        var ackCalled = default(TimeSpan?);
        using var handle = manager.Register(context);
        handle.SetAckCallback(x => ackCalled = x);
        timeProvider.Advance(TimeSpan.FromMilliseconds(350)); // SentAt = 00:00.0350
        await Task.Delay(16);

        // Simulate to send heartbeat responses from clients.
        timeProvider.Advance(TimeSpan.FromMilliseconds(50)); // ReceivedAt = 00:00.0400
        handle.Ack(0);
        await Task.Delay(16);

        // Assert
        Assert.True(ackCalled.HasValue);
        Assert.Equal(TimeSpan.FromMilliseconds(50), ackCalled.Value);
    }

    static byte[] BuildMessageHeader(byte sequence, DateTimeOffset serverSentAt)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(5);
        {
            messagePackWriter.Write(127);                                   // 0: 0x7f / 127: ServerHeartbeat
            messagePackWriter.Write(sequence);                              // 1: Sequence
            messagePackWriter.Write(serverSentAt.ToUnixTimeMilliseconds()); // 2: ServerSentAt
            messagePackWriter.WriteNil();                                   // 3: Dummy
        }
        messagePackWriter.Flush();

        return bufferWriter.WrittenSpan.ToArray();
    }

    static byte[] BuildMessage(byte sequence, long serverSentAt)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(5);
        {
            messagePackWriter.Write(127);          // 0: 0x7f / 127: ServerHeartbeat
            messagePackWriter.Write(sequence);     // 1: Sequence
            messagePackWriter.Write(serverSentAt); // 2: ServerSentAt
            messagePackWriter.WriteNil();          // 3: Dummy
            messagePackWriter.WriteNil();          // 4: Dummy
        }
        messagePackWriter.Flush();

        return bufferWriter.WrittenSpan.ToArray();
    }

    static byte[] BuildMessage(byte sequence, DateTimeOffset serverSentAt)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(5);
        {
            messagePackWriter.Write(127);                                   // 0: 0x7f / 127: ServerHeartbeat
            messagePackWriter.Write(sequence);                              // 1: Sequence
            messagePackWriter.Write(serverSentAt.ToUnixTimeMilliseconds()); // 2: ServerSentAt
            messagePackWriter.WriteNil();                                   // 3: Dummy
            messagePackWriter.WriteNil();                                   // 4: Dummy

        }
        messagePackWriter.Flush();

        return bufferWriter.WrittenSpan.ToArray();
    }

    class CustomHeartbeatMetadataProvider : IStreamingHubHeartbeatMetadataProvider
    {
        public bool TryWriteMetadata(IBufferWriter<byte> writer)
        {
            writer.Write("Hello"u8);
            return true;
        }
    }

    static FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> CreateFakeStreamingServiceContext()
    {
        var hubType = typeof(ITestHub);
        var hubMethod = hubType.GetMethod(nameof(ITestHub.OneArgument))!;
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethod, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);
        return fakeStreamingHubContext;
    }
}
