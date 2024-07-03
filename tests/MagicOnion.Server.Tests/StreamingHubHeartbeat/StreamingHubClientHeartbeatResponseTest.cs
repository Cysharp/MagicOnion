using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using NSubstitute;
using static MagicOnion.Server.Tests.StreamingHubHeartbeat.StreamingHubClientHeartbeatResponseTest;

namespace MagicOnion.Server.Tests.StreamingHubHeartbeat;

public class StreamingHubClientHeartbeatResponseTest(StreamingHubClientHeartbeatResponseTestServerFixture fixture) : IClassFixture<StreamingHubClientHeartbeatResponseTestServerFixture>
{
    protected ServerFixture Fixture { get; } = fixture;

    public class StreamingHubClientHeartbeatResponseTestServerFixture : ServerFixture<
        StreamingHubClientHeartbeatResponseTestHub
    >
    {
        protected override void ConfigureMagicOnion(MagicOnionOptions options)
        {
            options.EnableStreamingHubHeartbeat = false; // Server Heartbeat is disabled on this test.
        }
    }

    [Fact]
    public async Task Interval()
    {
        // Arrange
        var receiver = Substitute.For<IStreamingHubClientHeartbeatResponseTestHubReceiver>();
        var receivedHeartbeatEvents = new List<ClientHeartbeatEvent>();
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithClientHeartbeatInterval(TimeSpan.FromMilliseconds(100))
            .WithClientHeartbeatResponseReceived(x => receivedHeartbeatEvents.Add(x));

        // Act
        var client = await Fixture.CreateStreamingHubClientAsync<IStreamingHubClientHeartbeatResponseTestHub, IStreamingHubClientHeartbeatResponseTestHubReceiver>(receiver, options);
        await Task.Delay(250);
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
