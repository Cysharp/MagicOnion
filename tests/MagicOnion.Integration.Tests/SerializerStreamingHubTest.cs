using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Server.Hubs;
using MagicOnionTestServer;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Integration.Tests;

public class SerializerStreamingHubTest : IClassFixture<MagicOnionApplicationFactory<SerializerTestHub>>
{
    readonly WebApplicationFactory<Program> factory;

    public SerializerStreamingHubTest(MagicOnionApplicationFactory<SerializerTestHub> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = XorMessagePackMagicOnionSerializerProvider.Instance;
        });
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new [] { new TestStreamingHubClientFactory<ISerializerTestHub, ISerializerTestHubReceiver>("Dynamic", (callInvoker, receiver, messageSerializer) => StreamingHubClient.ConnectAsync<ISerializerTestHub, ISerializerTestHubReceiver>(callInvoker, receiver, messageSerializer: messageSerializer)) };
        yield return new [] { new TestStreamingHubClientFactory<ISerializerTestHub, ISerializerTestHubReceiver>("Static", async (callInvoker, receiver, messageSerializer) =>
        {
            var client = new SerializerTestHubClient(callInvoker, string.Empty, new CallOptions(), messageSerializer ?? MagicOnionSerializerProvider.Default, NullMagicOnionClientLogger.Instance);
            await client.__ConnectAndSubscribeAsync(receiver, default);
            return client;
        })};
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameterless(TestStreamingHubClientFactory<ISerializerTestHub, ISerializerTestHubReceiver> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver, messageSerializer: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.MethodParameterless();

        // Assert
        result.Should().Be(123);
    }
    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameter_One(TestStreamingHubClientFactory<ISerializerTestHub, ISerializerTestHubReceiver> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver, messageSerializer: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.MethodParameter_One(12345);

        // Assert
        result.Should().Be(123 + 12345);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameter_Many(TestStreamingHubClientFactory<ISerializerTestHub, ISerializerTestHubReceiver> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver, messageSerializer: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.MethodParameter_Many(12345, "6789");

        // Assert
        result.Should().Be(123 + 12345 + 6789);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Callback(TestStreamingHubClientFactory<ISerializerTestHub, ISerializerTestHubReceiver> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver, messageSerializer: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.Callback(12345, "6789");
        await Task.Delay(100);
        var result2  = await client.Callback(98765, "43210");
        await Task.Delay(100);

        // Assert
        result.Should().Be(123);
        result2.Should().Be(123);
        receiver.Received.Should().HaveCount(2);
        receiver.Received.Should().Contain((12345, "6789"));
        receiver.Received.Should().Contain((98765, "43210"));
    }

    class Receiver : ISerializerTestHubReceiver
    {
        public List<(int Arg0, string Arg1)> Received { get; } = new List<(int Arg0, string Arg1)>();
        public void OnMessage(int arg0, string arg1)
        {
            Received.Add((arg0, arg1));
        }
    }
}


public interface ISerializerTestHubReceiver
{
    void OnMessage(int arg0, string arg1);
}

public interface ISerializerTestHub : IStreamingHub<ISerializerTestHub, ISerializerTestHubReceiver>
{
    Task MethodReturnWithoutValue();
    Task<int> MethodParameterless();
    Task<int> MethodParameter_One(int arg0);
    Task<int> MethodParameter_Many(int arg0, string arg1);
    Task<int> Callback(int arg0, string arg1);
}

public class SerializerTestHub : StreamingHubBase<ISerializerTestHub, ISerializerTestHubReceiver>, ISerializerTestHub
{
    IGroup? group;

    protected override async ValueTask OnConnecting()
    {
        group = await Group.AddAsync(ConnectionId.ToString());
    }

    public Task MethodReturnWithoutValue() => Task.CompletedTask;
    public Task<int> MethodParameterless() => Task.FromResult(123);
    public Task<int> MethodParameter_One(int arg0) => Task.FromResult(arg0 + 123);
    public Task<int> MethodParameter_Many(int arg0, string arg1) => Task.FromResult(arg0 + int.Parse(arg1) + 123);

    public Task<int> Callback(int arg0, string arg1)
    {
        Broadcast(group!).OnMessage(arg0, arg1);
        return Task.FromResult(123);
    }
}

