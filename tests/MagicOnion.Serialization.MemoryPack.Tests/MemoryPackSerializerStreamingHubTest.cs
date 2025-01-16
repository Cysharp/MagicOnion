using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

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
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.MethodReturnCustomObject();

        // Assert
        Assert.Equal(12345, result.Item1);
        Assert.Equal("6789", result.Item2);
    }


    [Fact]
    public async Task StreamingHub_Parameterless()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.MethodParameterless();

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public async Task StreamingHub_Parameter_One()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.MethodParameter_One(12345);

        // Assert
        Assert.Equal(123 + 12345, result);
    }

    [Fact]
    public async Task StreamingHub_Parameter_Many()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.MethodParameter_Many(12345, "6789");

        // Assert
        Assert.Equal(123 + 12345 + 6789, result);
    }

    [Fact]
    public async Task StreamingHub_Parameter_CustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.MethodParameter_CustomObject(new MyRequestResponse() { Item1 = 12345, Item2 = "6789" });

        // Assert
        Assert.Equal(123 + 12345 + 6789, result);
    }

    [Fact]
    public async Task StreamingHub_Callback()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.Callback(12345, "6789");
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var result2 = await client.Callback(98765, "43210");
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(123, result);
        Assert.Equal(123, result2);
        Assert.Equal(2, receiver.Received.Count());
        Assert.Contains((12345, "6789"), receiver.Received);
        Assert.Contains((98765, "43210"), receiver.Received);
    }

    [Fact]
    public async Task StreamingHub_CallbackCustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await client.CallbackCustomObject(new MyRequestResponse() { Item1 = 12345, Item2 = "6789" });
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var result2 = await client.CallbackCustomObject(new MyRequestResponse() { Item1 = 98765, Item2 = "43210" });
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(123, result);
        Assert.Equal(123, result2);
        Assert.Equal(2, receiver.Received.Count());
        Assert.Contains((12345, "6789"), receiver.Received);
        Assert.Contains((98765, "43210"), receiver.Received);
    }

    [Fact]
    public async Task StreamingHub_ThrowReturnStatusException()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(() => client.ThrowReturnStatusException());

        // Assert
        Assert.NotNull(ex);
        Assert.Equal(StatusCode.Unknown, ex!.StatusCode);
        Assert.Equal("Detail-String", ex.Status.Detail);
    }

    [Fact]
    public async Task StreamingHub_Throw()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(() => client.Throw());

        // Assert
        Assert.NotNull(ex);
        Assert.Equal(StatusCode.Internal, ex!.StatusCode);
        Assert.StartsWith("An error occurred while processing handler", ex.Status.Detail);
    }

    [Fact]
    public async Task StreamingHub_Throw_WithServerStackTrace()
    {
        // Arrange
        var factory = this.factory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.Configure<MagicOnionOptions>(options => options.IsReturnExceptionStackTraceInErrorDetail = true)));
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await StreamingHubClient.ConnectAsync<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(() => client.Throw());

        // Assert
        Assert.NotNull(ex);
        Assert.Equal(StatusCode.Internal, ex!.StatusCode);
        Assert.Contains("Something went wrong.", ex.Message);
        Assert.StartsWith("An error occurred while processing handler", ex.Status.Detail);
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
    Task ThrowReturnStatusException();
    Task Throw();
}

public class MemoryPackSerializerTestHub : StreamingHubBase<IMemoryPackSerializerTestHub, IMemoryPackSerializerTestHubReceiver>, IMemoryPackSerializerTestHub
{
    IGroup<IMemoryPackSerializerTestHubReceiver>? group;

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
        group!.All.OnMessage(arg0, arg1);
        return Task.FromResult(123);
    }

    public Task<int> CallbackCustomObject(MyRequestResponse arg0)
    {
        group!.All.OnMessageCustomObject(new MyRequestResponse() { Item1 = arg0.Item1, Item2 = arg0.Item2 });
        return Task.FromResult(123);
    }

    public Task ThrowReturnStatusException()
    {
        throw new ReturnStatusException(StatusCode.Unknown, "Detail-String");
    }

    public Task Throw()
    {
        throw new InvalidOperationException("Something went wrong.");
    }
}

[MemoryPackable]
public partial class MyRequestResponse
{
    public int Item1 { get; init; }
    public required string Item2 { get; init; }
}
