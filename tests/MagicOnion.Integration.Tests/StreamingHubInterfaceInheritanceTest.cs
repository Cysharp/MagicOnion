using Grpc.Net.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Integration.Tests;

public class StreamingHubInterfaceInheritanceTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubInheritanceTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubInheritanceTestHub> factory;

    public StreamingHubInterfaceInheritanceTest(MagicOnionApplicationFactory<StreamingHubInheritanceTestHub> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new [] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
        yield return new [] { new TestStreamingHubClientFactory("Static", MagicOnionGeneratedClientInitializer.StreamingHubClientFactoryProvider)};
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task InterfaceInheritance(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubInheritanceTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubInheritanceTestHub, IStreamingHubInheritanceTestHubReceiver>(channel, receiver);

        // Act
        await client.MethodA();
        await client.MethodB();
        await client.MethodC();
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_MethodA();
        receiver.Received().Receiver_MethodB();
        receiver.Received().Receiver_MethodC();
    }
}

public class StreamingHubInheritanceTestHub : StreamingHubBase<IStreamingHubInheritanceTestHub, IStreamingHubInheritanceTestHubReceiver>, IStreamingHubInheritanceTestHub
{
    IGroup group = default!;

    protected override async ValueTask OnConnecting()
    {
        group = await Group.AddAsync(ConnectionId.ToString());
    }

    public Task MethodA()
    {
        Broadcast(group).Receiver_MethodA();
        return Task.CompletedTask;
    }

    public Task MethodB()
    {
        Broadcast(group).Receiver_MethodB();
        return Task.CompletedTask;
    }

    public Task MethodC()
    {
        Broadcast(group).Receiver_MethodC();
        return Task.CompletedTask;
    }
}

public interface IStreamingHubInheritanceTestHubReceiver : IStreamingHubInheritanceTestHubReceiverEx2
{
    void Receiver_MethodA();
}

public interface IStreamingHubInheritanceTestHubReceiverEx2 : IStreamingHubInheritanceTestHubReceiverEx
{
    void Receiver_MethodC();
}

public interface IStreamingHubInheritanceTestHubReceiverEx
{
    void Receiver_MethodB();
}

public interface IStreamingHubInheritanceTestHub : IStreamingHub<IStreamingHubInheritanceTestHub, IStreamingHubInheritanceTestHubReceiver>, IStreamingHubInheritanceTestHubEx2
{
    Task MethodA();
}

public interface IStreamingHubInheritanceTestHubEx
{
    Task MethodB();
}

public interface IStreamingHubInheritanceTestHubEx2 : IStreamingHubInheritanceTestHubEx
{
    Task MethodC();
}
