using System.Collections.Concurrent;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public abstract class StreamingHubHeartbeatTestBase
{
    protected ServerFixture Fixture { get; }

    public StreamingHubHeartbeatTestBase(ServerFixture fixture)
    {
        this.Fixture = fixture;
    }

    [Fact]
    public async Task EnableByAttribute()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubHeartbeatTestHub_EnableByAttribute, IStreamingHubHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Equal(2, receivedHeartbeatMetadata.Count); // The client must receive a heartbeat every 300ms from the server.
    }

    [Fact]
    public async Task DisableByAttribute()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubHeartbeatTestHub_DisableByAttribute, IStreamingHubHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Empty(receivedHeartbeatMetadata);
    }

    [Fact]
    public async Task Override_Interval()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Single(receivedHeartbeatMetadata); // The client must receive a heartbeat every 500ms from the server.
    }

    [Fact]
    public async Task Timeout()
    {
        // Arrange
        var heartbeatReceived = new TaskCompletionSource();
        var receiver = Substitute.For<IStreamingHubHeartbeatTestHubReceiver>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithHeartbeatReceived(x =>
        {
            heartbeatReceived.SetResult();
            Thread.Sleep(200); // Block consuming message loop.
        });

        // We need to consume message inline. Avoid post continuations to the synchronization context.
        SynchronizationContext.SetSynchronizationContext(null);

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubHeartbeatTestHub_TimeoutBehavior, IStreamingHubHeartbeatTestHubReceiver>(receiver, options);

        // Wait for receiving a heartbeat from the server.
        // The client must receive a heartbeat every 200ms from the server.
        await heartbeatReceived.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Timeout at 100 ms after receiving a heartbeat.
        await Task.Delay(500);

        // Assert
        Assert.True((bool)Fixture.Items.GetValueOrDefault("Disconnected"));
        Assert.True(client.WaitForDisconnect().IsCompleted);
    }
}


public class StreamingHubHeartbeatTest_DisabledByDefault : StreamingHubHeartbeatTestBase, IClassFixture<StreamingHubHeartbeatTest_DisabledByDefault.StreamingHubHeartbeatTestServerFixture>
{
    public StreamingHubHeartbeatTest_DisabledByDefault(StreamingHubHeartbeatTestServerFixture fixture)
        : base(fixture)
    {
    }

    public class StreamingHubHeartbeatTestServerFixture : ServerFixture<
        StreamingHubHeartbeatTestHub,
        StreamingHubHeartbeatTestHub_EnableByAttribute,
        StreamingHubHeartbeatTestHub_DisableByAttribute,
        StreamingHubHeartbeatTestHub_CustomIntervalAndTimeout,
        StreamingHubHeartbeatTestHub_TimeoutBehavior
    >
    {
        protected override void ConfigureMagicOnion(MagicOnionOptions options)
        {
            options.StreamingHubHeartbeatInterval = TimeSpan.FromMilliseconds(300);
            options.StreamingHubHeartbeatTimeout = TimeSpan.FromMilliseconds(200);
            options.EnableStreamingHubHeartbeat = false; // Disabled by default.
        }
    }

    [Fact]
    public async Task Default_Disable()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubHeartbeatTestHub, IStreamingHubHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Empty(receivedHeartbeatMetadata);
    }
}


public class StreamingHubHeartbeatTest_EnabledByDefault : StreamingHubHeartbeatTestBase, IClassFixture<StreamingHubHeartbeatTest_EnabledByDefault.StreamingHubHeartbeatTestServerFixture>
{
    public StreamingHubHeartbeatTest_EnabledByDefault(StreamingHubHeartbeatTestServerFixture fixture)
        : base(fixture)
    {
    }

    public class StreamingHubHeartbeatTestServerFixture : ServerFixture<
        StreamingHubHeartbeatTestHub,
        StreamingHubHeartbeatTestHub_EnableByAttribute,
        StreamingHubHeartbeatTestHub_DisableByAttribute,
        StreamingHubHeartbeatTestHub_CustomIntervalAndTimeout,
        StreamingHubHeartbeatTestHub_TimeoutBehavior
    >
    {
        protected override void ConfigureMagicOnion(MagicOnionOptions options)
        {
            options.StreamingHubHeartbeatInterval = TimeSpan.FromMilliseconds(300);
            options.StreamingHubHeartbeatTimeout = TimeSpan.FromMilliseconds(200);
            options.EnableStreamingHubHeartbeat = true; // Enabled by default.
        }
    }

    [Fact]
    public async Task Default_Enable()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubHeartbeatTestHub, IStreamingHubHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Equal(2, receivedHeartbeatMetadata.Count);
    }
}

public interface IStreamingHubHeartbeatTestHub : IStreamingHub<IStreamingHubHeartbeatTestHub, IStreamingHubHeartbeatTestHubReceiver>;
public interface IStreamingHubHeartbeatTestHub_EnableByAttribute : IStreamingHub<IStreamingHubHeartbeatTestHub_EnableByAttribute, IStreamingHubHeartbeatTestHubReceiver>;
public interface IStreamingHubHeartbeatTestHub_DisableByAttribute : IStreamingHub<IStreamingHubHeartbeatTestHub_DisableByAttribute, IStreamingHubHeartbeatTestHubReceiver>;
public interface IStreamingHubHeartbeatTestHub_CustomIntervalAndTimeout : IStreamingHub<IStreamingHubHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubHeartbeatTestHubReceiver>;
public interface IStreamingHubHeartbeatTestHub_TimeoutBehavior : IStreamingHub<IStreamingHubHeartbeatTestHub_TimeoutBehavior, IStreamingHubHeartbeatTestHubReceiver>;
public interface IStreamingHubHeartbeatTestHubReceiver;

// Implementations

// This streaming hub has no `Heartbeat` attribute.
public class StreamingHubHeartbeatTestHub()
    : StreamingHubBase<IStreamingHubHeartbeatTestHub, IStreamingHubHeartbeatTestHubReceiver>, IStreamingHubHeartbeatTestHub;

[Heartbeat]
public class StreamingHubHeartbeatTestHub_EnableByAttribute()
    : StreamingHubBase<IStreamingHubHeartbeatTestHub_EnableByAttribute, IStreamingHubHeartbeatTestHubReceiver>, IStreamingHubHeartbeatTestHub_EnableByAttribute;

[Heartbeat(Enable = false)]
public class StreamingHubHeartbeatTestHub_DisableByAttribute()
    : StreamingHubBase<IStreamingHubHeartbeatTestHub_DisableByAttribute, IStreamingHubHeartbeatTestHubReceiver>, IStreamingHubHeartbeatTestHub_DisableByAttribute;

[Heartbeat(Interval = 500, Timeout = 100)]
public class StreamingHubHeartbeatTestHub_CustomIntervalAndTimeout()
    : StreamingHubBase<IStreamingHubHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubHeartbeatTestHubReceiver>, IStreamingHubHeartbeatTestHub_CustomIntervalAndTimeout;

[Heartbeat(Enable = true, Interval = 200, Timeout = 100)]
public class StreamingHubHeartbeatTestHub_TimeoutBehavior([FromKeyedServices(ServerFixture.ItemsServiceKey)] ConcurrentDictionary<string, object> items)
    : StreamingHubBase<IStreamingHubHeartbeatTestHub_TimeoutBehavior, IStreamingHubHeartbeatTestHubReceiver>, IStreamingHubHeartbeatTestHub_TimeoutBehavior
{
    protected override ValueTask OnDisconnected()
    {
        items["Disconnected"] = true;
        return base.OnDisconnected();
    }
}
