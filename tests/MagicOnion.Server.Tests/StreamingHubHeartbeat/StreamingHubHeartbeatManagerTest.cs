using System.Buffers;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public class StreamingHubHeartbeatManagerTest
{
    static readonly byte[] HeartbeatMessageHeader = [0x95, 0x7f, 0xc0, 0xc0, 0xc0]; // [127, Nil, Nil, Nil, <Extra>
    static readonly byte[] HeartbeatMessageNoExtra = [0x95, 0x7f, 0xc0, 0xc0, 0xc0, 0xc0]; // [127, Nil, Nil, Nil, Nil]

    [Fact]
    public void Register()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var manager = new StreamingHubHeartbeatManager(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, null, logger);
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
        var manager = new StreamingHubHeartbeatManager(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, null, logger);
        var context = CreateFakeStreamingServiceContext();
        var handle = manager.Register(context);

        // Act
        handle.Dispose();
    }

    [Fact]
    public async Task Interval_Disable_Timeout()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan, null, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        await Task.Delay(310);

        // Assert
        Assert.Equal(3, context1.Responses.Count);
        Assert.Equal(HeartbeatMessageNoExtra, context1.Responses[0].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context1.Responses[1].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context1.Responses[2].Memory.ToArray());
        Assert.Equal(3, context2.Responses.Count);
        Assert.Equal(HeartbeatMessageNoExtra, context2.Responses[0].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context2.Responses[1].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context2.Responses[2].Memory.ToArray());
        Assert.Equal(3, context3.Responses.Count);
        Assert.Equal(HeartbeatMessageNoExtra, context3.Responses[0].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context3.Responses[1].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context3.Responses[2].Memory.ToArray());
    }

    [Fact]
    public async Task Interval_Keep()
    {
        // Arrange
        var collector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<StreamingHubHeartbeatManager>(collector);
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        await Task.Delay(350);
        var isCanceled1 = handle1.TimeoutToken.IsCancellationRequested;
        var isCanceled2 = handle2.TimeoutToken.IsCancellationRequested;
        var isCanceled3 = handle3.TimeoutToken.IsCancellationRequested;
        // Simulate to send heartbeat responses from clients.
        handle1.Ack();
        handle2.Ack();
        handle3.Ack();
        await Task.Delay(250);

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
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(200), null, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        await Task.Delay(350);
        var isCanceled1 = handle1.TimeoutToken.IsCancellationRequested;
        var isCanceled2 = handle2.TimeoutToken.IsCancellationRequested;
        var isCanceled3 = handle3.TimeoutToken.IsCancellationRequested;
        await Task.Delay(250); // No responses from clients and timeouts are reached.

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
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan, null, logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();

        // Act
        var handle1 = manager.Register(context1);
        var handle2 = manager.Register(context2);
        var handle3 = manager.Register(context3);
        await Task.Delay(310);
        handle1.Dispose();
        handle2.Dispose();
        handle3.Dispose();
        await Task.Delay(300);

        // Assert
        Assert.Equal(3, context1.Responses.Count);
        Assert.Equal(HeartbeatMessageNoExtra, context1.Responses[0].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context1.Responses[1].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context1.Responses[2].Memory.ToArray());
        Assert.Equal(3, context2.Responses.Count);
        Assert.Equal(HeartbeatMessageNoExtra, context2.Responses[0].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context2.Responses[1].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context2.Responses[2].Memory.ToArray());
        Assert.Equal(3, context3.Responses.Count);
        Assert.Equal(HeartbeatMessageNoExtra, context3.Responses[0].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context3.Responses[1].Memory.ToArray());
        Assert.Equal(HeartbeatMessageNoExtra, context3.Responses[2].Memory.ToArray());
    }

    [Fact]
    public async Task CustomMetadataProvider()
    {
        // Arrange
        var logger = new FakeLogger<StreamingHubHeartbeatManager>();
        var manager = new StreamingHubHeartbeatManager(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan, new CustomHeartbeatMetadataProvider(), logger);
        var context1 = CreateFakeStreamingServiceContext();
        var context2 = CreateFakeStreamingServiceContext();
        var context3 = CreateFakeStreamingServiceContext();
        byte[] expectedHeartbeatMessage = [.. HeartbeatMessageHeader, .. "Hello"u8];

        // Act
        using var handle1 = manager.Register(context1);
        using var handle2 = manager.Register(context2);
        using var handle3 = manager.Register(context3);
        await Task.Delay(310);

        // Assert
        Assert.Equal(3, context1.Responses.Count);
        Assert.Equal(expectedHeartbeatMessage, context1.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage, context1.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage, context1.Responses[2].Memory.ToArray());
        Assert.Equal(3, context2.Responses.Count);
        Assert.Equal(expectedHeartbeatMessage, context2.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage, context2.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage, context2.Responses[2].Memory.ToArray());
        Assert.Equal(3, context3.Responses.Count);
        Assert.Equal(expectedHeartbeatMessage, context3.Responses[0].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage, context3.Responses[1].Memory.ToArray());
        Assert.Equal(expectedHeartbeatMessage, context3.Responses[2].Memory.ToArray());
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
