using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using MagicOnionTestServer;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Serialization.MemoryPack.Tests;

public class MemoryPackSerializerStreamingHubTest : IClassFixture<MagicOnionApplicationFactory<MemoryPackSerializerTestHub>>
{
    readonly WebApplicationFactory<Program> factory;

    public MemoryPackSerializerStreamingHubTest(MagicOnionApplicationFactory<MemoryPackSerializerTestHub> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = MemoryPackMagicOnionSerializerProvider.Instance;
        });
    }

    [Fact]
    public async Task StreamingHub_ReturnCustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.MethodReturnCustomObject();

        // Assert
        result.Item1.Should().Be(12345);
        result.Item2.Should().Be("6789");
    }


    [Fact]
    public async Task StreamingHub_Parameterless()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.MethodParameterless();

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public async Task StreamingHub_Parameter_One()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.MethodParameter_One(12345);

        // Assert
        result.Should().Be(123 + 12345);
    }

    [Fact]
    public async Task StreamingHub_Parameter_Many()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.MethodParameter_Many(12345, "6789");

        // Assert
        result.Should().Be(123 + 12345 + 6789);
    }

    [Fact]
    public async Task StreamingHub_Parameter_CustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.MethodParameter_CustomObject(new MyRequestResponse() { Item1 = 12345, Item2 = "6789" });

        // Assert
        result.Should().Be(123 + 12345 + 6789);
    }

    [Fact]
    public async Task StreamingHub_Callback()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.Callback(12345, "6789");
        await Task.Delay(100);
        var result2 = await client.Callback(98765, "43210");
        await Task.Delay(100);

        // Assert
        result.Should().Be(123);
        result2.Should().Be(123);
        receiver.Received.Should().HaveCount(2);
        receiver.Received.Should().Contain((12345, "6789"));
        receiver.Received.Should().Contain((98765, "43210"));
    }

    [Fact]
    public async Task StreamingHub_CallbackCustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.CallbackCustomObject(new MyRequestResponse() { Item1 = 12345, Item2 = "6789" });
        await Task.Delay(100);
        var result2 = await client.CallbackCustomObject(new MyRequestResponse() { Item1 = 98765, Item2 = "43210" });
        await Task.Delay(100);

        // Assert
        result.Should().Be(123);
        result2.Should().Be(123);
        receiver.Received.Should().HaveCount(2);
        receiver.Received.Should().Contain((12345, "6789"));
        receiver.Received.Should().Contain((98765, "43210"));
    }

    class Receiver : IMemoryPackSerializerTestHubReceiver
    {
        public List<(int Arg0, string Arg1)> Received { get; } = new List<(int Arg0, string Arg1)>();
        public void OnMessage(int arg0, string arg1)
        {
            Received.Add((arg0, arg1));
        }

        public void OnMessageCustomObject(MyRequestResponse arg0)
        {
            Received.Add((arg0.Item1, arg0.Item2));
        }
    }
}


public interface IMemoryPackSerializerTestHubReceiver
{
    void OnMessage(int arg0, string arg1);
    void OnMessageCustomObject(MyRequestResponse arg0);
}

public interface IMemoryPackSerializerTestHub : IStreamingHub<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>
{
    Task MethodReturnWithoutValue();
    Task<MyRequestResponse> MethodReturnCustomObject();
    Task<int> MethodParameterless();
    Task<int> MethodParameter_One(int arg0);
    Task<int> MethodParameter_Many(int arg0, string arg1);
    Task<int> MethodParameter_CustomObject(MyRequestResponse arg0);
    Task<int> Callback(int arg0, string arg1);
    Task<int> CallbackCustomObject(MyRequestResponse arg0);
}

public class MemoryPackSerializerTestHub : StreamingHubBase<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>, IMemoryPackSerializerTestHub
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
    public Task<int> MethodParameter_CustomObject(MyRequestResponse arg0) => Task.FromResult(arg0.Item1 + int.Parse(arg0.Item2) + 123);
    public Task<MyRequestResponse> MethodReturnCustomObject() => Task.FromResult(new MyRequestResponse() { Item1 = 12345, Item2 = "6789" });

    public Task<int> Callback(int arg0, string arg1)
    {
        Broadcast(group!).OnMessage(arg0, arg1);
        return Task.FromResult(123);
    }

    public Task<int> CallbackCustomObject(MyRequestResponse arg0)
    {
        Broadcast(group!).OnMessageCustomObject(new MyRequestResponse() { Item1 = arg0.Item1, Item2 = arg0.Item2 });
        return Task.FromResult(123);
    }
}

[MemoryPackable]
public partial class MyRequestResponse
{
    public int Item1 { get; init; }
    public required string Item2 { get; init; }
}
