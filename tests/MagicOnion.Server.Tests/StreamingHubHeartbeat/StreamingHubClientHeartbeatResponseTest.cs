using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using static MagicOnion.Server.Tests.StreamingHubHeartbeat.StreamingHubClientHeartbeatResponseTest;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public class StreamingHubClientHeartbeatResponseTest(StreamingHubClientHeartbeatResponseTestServerFixture fixture) : IClassFixture<StreamingHubClientHeartbeatResponseTestServerFixture>
{
    protected StreamingHubClientHeartbeatResponseTestServerFixture Fixture { get; } = fixture;

    public class StreamingHubClientHeartbeatResponseTestServerFixture : ServerFixture<
        StreamingHubClientHeartbeatResponseTestHub
    >
    {
        public FakeTimeProvider FakeTimeProvider { get; } = new();

        protected override void ConfigureMagicOnion(MagicOnionOptions options)
        {
            options.EnableStreamingHubHeartbeat = false; // Server Heartbeat is disabled on this test.
            options.TimeProvider = FakeTimeProvider;
        }
    }

    [Fact]
    public async Task Interval()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubClientHeartbeatResponseTestHubReceiver>();
        var receivedHeartbeatEvents = new List<ClientHeartbeatEvent>();
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithTimeProvider(Fixture.FakeTimeProvider)
            .WithClientHeartbeatInterval(TimeSpan.FromMilliseconds(100))
            .WithClientHeartbeatResponseReceived(x => receivedHeartbeatEvents.Add(x));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubClientHeartbeatResponseTestHub, IStreamingHubClientHeartbeatResponseTestHubReceiver>(receiver, options);
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100)); // 100ms
        await Task.Delay(50); // Client -> Server -> Client
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100)); // 200ms
        await Task.Delay(50); // Client -> Server -> Client
        Fixture.FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(50)); // 250ms
        await client.DisposeAsync();

        // Assert
        Assert.Equal(2, receivedHeartbeatEvents.Count);
    }

}

public interface IStreamingHubClientHeartbeatResponseTestHub : IStreamingHub<IStreamingHubClientHeartbeatResponseTestHub, IStreamingHubClientHeartbeatResponseTestHubReceiver>;
public interface IStreamingHubClientHeartbeatResponseTestHubReceiver;

public class StreamingHubClientHeartbeatResponseTestHub : StreamingHubBase<IStreamingHubClientHeartbeatResponseTestHub, IStreamingHubClientHeartbeatResponseTestHubReceiver>, IStreamingHubClientHeartbeatResponseTestHub
{
}
