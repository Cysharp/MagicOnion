using System.Collections.Concurrent;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public abstract class StreamingHubServerHeartbeatTestBase
{
    protected ServerFixture Fixture { get; }

    public StreamingHubServerHeartbeatTestBase(ServerFixture fixture)
    {
        this.Fixture = fixture;
    }

    [Fact]
    public async Task EnableByAttribute()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_EnableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Equal(2, receivedHeartbeatMetadata.Count); // The client must receive a heartbeat every 300ms from the server.
    }

    [Fact]
    public async Task DisableByAttribute()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_DisableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Empty(receivedHeartbeatMetadata);
    }

    [Fact]
    public async Task Override_Interval()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
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
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
        {
            heartbeatReceived.SetResult();
            Thread.Sleep(200); // Block consuming message loop.
        });

        // We need to consume message inline. Avoid post continuations to the synchronization context.
        SynchronizationContext.SetSynchronizationContext(null);

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_TimeoutBehavior, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);

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


public class StreamingHubServerHeartbeatTest_DisabledByDefault : StreamingHubServerHeartbeatTestBase, IClassFixture<StreamingHubServerHeartbeatTest_DisabledByDefault.StreamingHubHeartbeatTestServerFixture>
{
    public StreamingHubServerHeartbeatTest_DisabledByDefault(StreamingHubHeartbeatTestServerFixture fixture)
        : base(fixture)
    {
    }

    public class StreamingHubHeartbeatTestServerFixture : ServerFixture<
        StreamingHubServerHeartbeatTestHub,
        StreamingHubServerHeartbeatTestHub_EnableByAttribute,
        StreamingHubServerHeartbeatTestHub_DisableByAttribute,
        StreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout,
        StreamingHubServerHeartbeatTestHub_TimeoutBehavior
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
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Empty(receivedHeartbeatMetadata);
    }
}


public class StreamingHubServerHeartbeatTest_EnabledByDefault : StreamingHubServerHeartbeatTestBase, IClassFixture<StreamingHubServerHeartbeatTest_EnabledByDefault.StreamingHubHeartbeatTestServerFixture>
{
    public StreamingHubServerHeartbeatTest_EnabledByDefault(StreamingHubHeartbeatTestServerFixture fixture)
        : base(fixture)
    {
    }

    public class StreamingHubHeartbeatTestServerFixture : ServerFixture<
        StreamingHubServerHeartbeatTestHub,
        StreamingHubServerHeartbeatTestHub_EnableByAttribute,
        StreamingHubServerHeartbeatTestHub_DisableByAttribute,
        StreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout,
        StreamingHubServerHeartbeatTestHub_TimeoutBehavior
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
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        await Task.Delay(650);
        await client.DisposeAsync();

        // Assert
        Assert.Equal(2, receivedHeartbeatMetadata.Count);
    }
}

public interface IStreamingHubServerHeartbeatTestHub : IStreamingHub<IStreamingHubServerHeartbeatTestHub, IStreamingHubServerHeartbeatTestHubReceiver>;
public interface IStreamingHubServerHeartbeatTestHub_EnableByAttribute : IStreamingHub<IStreamingHubServerHeartbeatTestHub_EnableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>;
public interface IStreamingHubServerHeartbeatTestHub_DisableByAttribute : IStreamingHub<IStreamingHubServerHeartbeatTestHub_DisableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>;
public interface IStreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout : IStreamingHub<IStreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubServerHeartbeatTestHubReceiver>;
public interface IStreamingHubServerHeartbeatTestHub_TimeoutBehavior : IStreamingHub<IStreamingHubServerHeartbeatTestHub_TimeoutBehavior, IStreamingHubServerHeartbeatTestHubReceiver>;
public interface IStreamingHubServerHeartbeatTestHubReceiver;

// Implementations

// This streaming hub has no `Heartbeat` attribute.
public class StreamingHubServerHeartbeatTestHub()
    : StreamingHubBase<IStreamingHubServerHeartbeatTestHub, IStreamingHubServerHeartbeatTestHubReceiver>, IStreamingHubServerHeartbeatTestHub;

[Heartbeat]
public class StreamingHubServerHeartbeatTestHub_EnableByAttribute()
    : StreamingHubBase<IStreamingHubServerHeartbeatTestHub_EnableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>, IStreamingHubServerHeartbeatTestHub_EnableByAttribute;

[Heartbeat(Enable = false)]
public class StreamingHubServerHeartbeatTestHub_DisableByAttribute()
    : StreamingHubBase<IStreamingHubServerHeartbeatTestHub_DisableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>, IStreamingHubServerHeartbeatTestHub_DisableByAttribute;

[Heartbeat(Interval = 500, Timeout = 100)]
public class StreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout()
    : StreamingHubBase<IStreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubServerHeartbeatTestHubReceiver>, IStreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout;

[Heartbeat(Enable = true, Interval = 200, Timeout = 100)]
public class StreamingHubServerHeartbeatTestHub_TimeoutBehavior([FromKeyedServices(ServerFixture.ItemsServiceKey)] ConcurrentDictionary<string, object> items)
    : StreamingHubBase<IStreamingHubServerHeartbeatTestHub_TimeoutBehavior, IStreamingHubServerHeartbeatTestHubReceiver>, IStreamingHubServerHeartbeatTestHub_TimeoutBehavior
{
    protected override ValueTask OnDisconnected()
    {
        items["Disconnected"] = true;
        return base.OnDisconnected();
    }
}
