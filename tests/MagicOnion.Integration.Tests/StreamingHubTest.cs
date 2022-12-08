using System.Collections.Concurrent;
using System.Diagnostics;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Integration.Tests;

public class StreamingHubTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubTestHub> factory;

    public StreamingHubTest(MagicOnionApplicationFactory<StreamingHubTestHub> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new [] { new TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver>("Dynamic", (callInvoker, receiver, messageSerializer) => StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(callInvoker, receiver, messageSerializer: messageSerializer)) };
        yield return new [] { new TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver>("Static", async (callInvoker, receiver, messageSerializer) =>
        {
            var client = new StreamingHubTestHubClient(callInvoker, string.Empty, new CallOptions(), messageSerializer ?? MagicOnionMessageSerializerProvider.Default, NullMagicOnionClientLogger.Instance);
            await client.__ConnectAndSubscribeAsync(receiver, default);
            return client;
        })};
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NoReturn_Parameter_Zero(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act & Assert
        await client.NoReturn_Parameter_Zero();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NoReturn_Parameter_One(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act & Assert
        await client.NoReturn_Parameter_One(12345);
    }

    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NoReturn_Parameter_Many(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act & Assert
        await client.NoReturn_Parameter_Many(12345, "Hello✨", true);
    }
    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Zero(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act
        var result = await client.Parameter_Zero();

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_One(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act
        var result = await client.Parameter_One(12345);

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Many(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act
        var result = await client.Parameter_Many(12345, "Hello✨", true);

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_Parameter_Zero(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act
        await client.CallReceiver_Parameter_Zero();
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Verify(x => x.Receiver_Parameter_Zero());
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_Parameter_One(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act
        await client.CallReceiver_Parameter_One(12345);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Verify(x => x.Receiver_Parameter_One(12345));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_Parameter_Many(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);

        // Act
        await client.CallReceiver_Parameter_Many(12345, "Hello✨", true);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Verify(x => x.Receiver_Parameter_Many(12345, "Hello✨", true));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Forget_NoReturnValue(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);
        client = client.FireAndForget(); // Use FireAndForget client

        // Act
        await client.Never();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Forget_WithReturnValue(TestStreamingHubClientFactory<IStreamingHubTestHub, IStreamingHubTestHubReceiver> clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = new Mock<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver.Object);
        client = client.FireAndForget(); // Use FireAndForget client

        // Act
        var result = await client.Never_With_Return();

        // Assert
        result.Should().Be(default(int));
    }
}

public class StreamingHubTestHub : StreamingHubBase<IStreamingHubTestHub, IStreamingHubTestHubReceiver>, IStreamingHubTestHub
{
    IGroup group = default!;

    protected override async ValueTask OnConnecting()
    {
        group = await Group.AddAsync(ConnectionId.ToString());
    }

    public Task NoReturn_Parameter_Zero()
    {
        return Task.CompletedTask;
    }

    public Task NoReturn_Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        return Task.CompletedTask;
    }

    public Task NoReturn_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        return Task.CompletedTask;
    }

    public Task<int> Parameter_Zero()
    {
        return Task.FromResult(67890);
    }

    public Task<int> Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        return Task.FromResult(67890);
    }

    public Task<int> Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        return Task.FromResult(67890);
    }

    public Task CallReceiver_Parameter_Zero()
    {
        Broadcast(group).Receiver_Parameter_Zero();
        return Task.CompletedTask;
    }

    public Task CallReceiver_Parameter_One(int arg0)
    {
        Broadcast(group).Receiver_Parameter_One(12345);
        return Task.CompletedTask;
    }

    public Task CallReceiver_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Broadcast(group).Receiver_Parameter_Many(12345, "Hello✨", true);
        return Task.CompletedTask;
    }

    public Task Never()
    {
        return new TaskCompletionSource().Task.WaitAsync(TimeSpan.FromMilliseconds(100));
    }
    
    public Task<int> Never_With_Return()
    {
        return new TaskCompletionSource<int>().Task.WaitAsync(TimeSpan.FromMilliseconds(100));
    }

}

public interface IStreamingHubTestHubReceiver
{
    void Receiver_Parameter_Zero();
    void Receiver_Parameter_One(int arg0);
    void Receiver_Parameter_Many(int arg0, string arg1, bool arg2);
}

public interface IStreamingHubTestHub : IStreamingHub<IStreamingHubTestHub, IStreamingHubTestHubReceiver>
{
    Task NoReturn_Parameter_Zero();
    Task NoReturn_Parameter_One(int arg0);
    Task NoReturn_Parameter_Many(int arg0, string arg1, bool arg2);

    Task<int> Parameter_Zero();
    Task<int> Parameter_One(int arg0);
    Task<int> Parameter_Many(int arg0, string arg1, bool arg2);

    Task CallReceiver_Parameter_Zero();
    Task CallReceiver_Parameter_One(int arg0);
    Task CallReceiver_Parameter_Many(int arg0, string arg1, bool arg2);

    Task Never();
    Task<int> Never_With_Return();
}
