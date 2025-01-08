using System.Collections.Concurrent;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server.Features;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public abstract class StreamingHubServerHeartbeatTestBase
{
    protected StreamingHubHeartbeatTestServerFixtureBase Fixture { get; }

    public abstract class StreamingHubHeartbeatTestServerFixtureBase : ServerFixture<
        StreamingHubServerHeartbeatTestHub,
        StreamingHubServerHeartbeatTestHub_EnableByAttribute,
        StreamingHubServerHeartbeatTestHub_DisableByAttribute,
        StreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout,
        StreamingHubServerHeartbeatTestHub_TimeoutBehavior,
        StreamingHubServerHeartbeatTestHub_Conditional
    >
    {
        public FakeTimeProvider FakeTimeProvider { get; } = new();

        protected sealed override void ConfigureMagicOnion(MagicOnionOptions options)
        {
            options.TimeProvider = FakeTimeProvider;
            options.StreamingHubHeartbeatInterval = TimeSpan.FromMilliseconds(300);
            options.StreamingHubHeartbeatTimeout = TimeSpan.FromMilliseconds(200);
            ConfigureMagicOnionCore(options);
        }

        protected virtual void ConfigureMagicOnionCore(MagicOnionOptions options){}
    }

    public StreamingHubServerHeartbeatTestBase(StreamingHubHeartbeatTestServerFixtureBase fixture)
    {
        this.Fixture = fixture;
    }

    [Fact]
    public async Task EnableByAttribute()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.Metadata.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_EnableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
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
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.Metadata.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_DisableByAttribute, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
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
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.Metadata.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_CustomIntervalAndTimeout, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
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
            Thread.Sleep(100); // Block consuming message loop.
        });

        // We need to consume message inline. Avoid post continuations to the synchronization context.
        SynchronizationContext.SetSynchronizationContext(null);

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_TimeoutBehavior, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);

        // Send a heartbeat to the client.
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);

        // Wait for receiving a heartbeat from the server.
        // The client must receive a heartbeat every 200ms from the server.
        await heartbeatReceived.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        // Timeout at 200 ms after receiving a heartbeat.
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(200));

        await Task.Delay(150, TestContext.Current.CancellationToken); // Wait for unblocking and disconnection.

        // Assert
        Assert.True((bool)Fixture.Items.GetValueOrDefault("Disconnected"));
        Assert.True((bool)Fixture.Items.GetValueOrDefault("Heartbeat/TimeoutToken/IsCancellationRequested"));
        Assert.True(client.WaitForDisconnect().IsCompleted);
    }


    [Fact]
    public async Task Disabled_Unregister_Manually()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var metadata = new Metadata()
        {
            {"x-mo-test-disable-heartbeat", "true"},
        };
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.Metadata.ToArray())).WithCallOptions(new CallOptions(metadata));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub_Conditional, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        await client.DisposeAsync();

        // Assert
        Assert.Empty(receivedHeartbeatMetadata);
    }
}


public class StreamingHubServerHeartbeatTest_DisabledByDefault : StreamingHubServerHeartbeatTestBase, IClassFixture<StreamingHubServerHeartbeatTest_DisabledByDefault.StreamingHubHeartbeatTestServerFixture>
{
    public StreamingHubServerHeartbeatTest_DisabledByDefault(StreamingHubHeartbeatTestServerFixture fixture)
        : base(fixture)
    {
    }

    public class StreamingHubHeartbeatTestServerFixture : StreamingHubHeartbeatTestServerFixtureBase
    {
        protected override void ConfigureMagicOnionCore(MagicOnionOptions options)
        {
            options.EnableStreamingHubHeartbeat = false; // Disabled by default.
        }
    }

    [Fact]
    public async Task Default_Disable()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.Metadata.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
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

    public class StreamingHubHeartbeatTestServerFixture : StreamingHubHeartbeatTestServerFixtureBase
    {
        protected override void ConfigureMagicOnionCore(MagicOnionOptions options)
        {
            options.EnableStreamingHubHeartbeat = true; // Enabled by default.
        }
    }

    [Fact]
    public async Task Default_Enable()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubServerHeartbeatTestHubReceiver>();
        var receivedHeartbeatMetadata = new List<byte[]>();
        var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x => receivedHeartbeatMetadata.Add(x.Metadata.ToArray()));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubServerHeartbeatTestHub, IStreamingHubServerHeartbeatTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await Task.Delay(15, TestContext.Current.CancellationToken);
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
public interface IStreamingHubServerHeartbeatTestHub_Conditional : IStreamingHub<IStreamingHubServerHeartbeatTestHub_Conditional, IStreamingHubServerHeartbeatTestHubReceiver>;
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
        var httpContext = Context.CallContext.GetHttpContext();
        var heartbeatFeature = httpContext.Features.GetRequiredFeature<IMagicOnionHeartbeatFeature>();

        items["Disconnected"] = true;
        items["Heartbeat/TimeoutToken/IsCancellationRequested"] = heartbeatFeature.TimeoutToken.IsCancellationRequested;
        return base.OnDisconnected();
    }
}

[Heartbeat(Enable = true, Interval = 200, Timeout = 100)]
public class StreamingHubServerHeartbeatTestHub_Conditional
    : StreamingHubBase<IStreamingHubServerHeartbeatTestHub_Conditional, IStreamingHubServerHeartbeatTestHubReceiver>, IStreamingHubServerHeartbeatTestHub_Conditional
{
    protected override ValueTask OnConnected()
    {
        var httpContext = Context.CallContext.GetHttpContext();
        if (httpContext.Request.Headers.TryGetValue("x-mo-test-disable-heartbeat", out var disableHeartbeatHeader))
        {
            if (disableHeartbeatHeader == "true")
            {
                httpContext.Features.GetRequiredFeature<IMagicOnionHeartbeatFeature>().Unregister();
            }
        }

        return base.OnConnected();
    }
}

