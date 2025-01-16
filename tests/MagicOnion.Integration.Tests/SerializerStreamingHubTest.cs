using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Hubs;
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
        yield return new [] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
        yield return new [] { new TestStreamingHubClientFactory("Static", MagicOnionGeneratedClientInitializer.StreamingHubClientFactoryProvider)};
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameterless(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync<ISerializerTestHub, ISerializerTestHubReceiver>(channel, receiver, serializerProvider: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.MethodParameterless();

        // Assert
        Assert.Equal(123, result);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync<ISerializerTestHub, ISerializerTestHubReceiver>(channel, receiver, serializerProvider: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.MethodParameter_One(12345);

        // Assert
        Assert.Equal(123 + 12345, result);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync<ISerializerTestHub, ISerializerTestHubReceiver>(channel, receiver, serializerProvider: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.MethodParameter_Many(12345, "6789");

        // Assert
        Assert.Equal(123 + 12345 + 6789, result);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Callback(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync<ISerializerTestHub, ISerializerTestHubReceiver>(channel, receiver, serializerProvider: XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.Callback(12345, "6789");
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var result2  = await client.Callback(98765, "43210");
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(123, result);
        Assert.Equal(123, result2);
        Assert.Equal(2, receiver.Received.Count());
        Assert.Contains((12345, "6789"), receiver.Received);
        Assert.Contains((98765, "43210"), receiver.Received);
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
    IGroup<ISerializerTestHubReceiver>? group;

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
        group!.All.OnMessage(arg0, arg1);
        return Task.FromResult(123);
    }
}

