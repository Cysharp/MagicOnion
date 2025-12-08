using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class StreamingHubConnectionEventsTest: IClassFixture<MagicOnionApplicationFactory<StreamingHubConnectionEventsTestHub>>
{
    readonly WebApplicationFactory<Program> factory;
    readonly ConcurrentBag<string> logs;
    readonly ConcurrentDictionary<string, object> items;

    public StreamingHubConnectionEventsTest(MagicOnionApplicationFactory<StreamingHubConnectionEventsTestHub> factory)
    {
        factory.Initialize();
        this.factory = factory;
        this.logs = factory.Logs;
        this.items = factory.Items;
    }

    [Fact]
    public async Task EventsCalled()
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() {HttpClient = httpClient});
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubConnectionEventsTestHub, IStreamingHubConnectionEventsTestHubReceiver>(
            channel: channel,
            receiver: Substitute.For<IStreamingHubConnectionEventsTestHubReceiver>(),
            cancellationToken: TestContext.Current.CancellationToken);

        await client.DisposeAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.True((bool)items.GetValueOrDefault("OnConnecting", false));
        Assert.True((bool)items.GetValueOrDefault("OnConnected", false));
        Assert.True((bool)items.GetValueOrDefault("OnDisconnected", false));
        Assert.Equal(["E/OnDisconnected", "E/OnConnected", "E/OnConnecting"], logs.Select(x => x.Split('\t')[3]).Where(x => x.StartsWith("E")));
    }

    [Fact]
    public async Task Throws_RSE_OnConnecting()
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() {HttpClient = httpClient});

        var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await StreamingHubClient.ConnectAsync<IStreamingHubConnectionEventsTestHub, IStreamingHubConnectionEventsTestHubReceiver>(
                channel: channel,
                receiver: Substitute.For<IStreamingHubConnectionEventsTestHubReceiver>(),
                option: new CallOptions(new Metadata(){ { "ThrowsRSEOnConnecting", "1" }}),
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.True(logs.Any(x => x.Contains("\tE/OnConnecting\t")));
        Assert.False(logs.Any(x => x.Contains("\tE/OnConnected\t")));
        Assert.False(logs.Any(x => x.Contains("\tError\t")));
    }

    [Fact]
    public async Task Throws_RSE_OnConnected()
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() {HttpClient = httpClient});

        // Even if an exception occurs in OnConnected, the connection is completed,
        // so it is not detected on the client side until a call or disconnection wait.
        var client =
            await StreamingHubClient.ConnectAsync<IStreamingHubConnectionEventsTestHub, IStreamingHubConnectionEventsTestHubReceiver>(
                channel: channel,
                receiver: Substitute.For<IStreamingHubConnectionEventsTestHubReceiver>(),
                option: new CallOptions(new Metadata(){ { "ThrowsRSEOnConnected", "1" }}),
                cancellationToken: TestContext.Current.CancellationToken);

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.HelloAsync());
        Assert.Contains("StreamingHubClient has already been disconnected from the server", ex.Message);
        Assert.Equal(StatusCode.Unavailable, ex.StatusCode);
        Assert.True(logs.Any(x => x.Contains("\tE/OnConnecting\t")));
        Assert.True(logs.Any(x => x.Contains("\tE/OnConnected\t")));
        Assert.False(logs.Any(x => x.Contains("\tError\t")));
    }
}

public interface IStreamingHubConnectionEventsTestHub : IStreamingHub<IStreamingHubConnectionEventsTestHub, IStreamingHubConnectionEventsTestHubReceiver>
{
    ValueTask<string> HelloAsync();
}

public interface IStreamingHubConnectionEventsTestHubReceiver;

public class StreamingHubConnectionEventsTestHub(
    [FromKeyedServices(MagicOnionApplicationFactory.ItemsKey)]ConcurrentDictionary<string, object> items,
    ILogger<StreamingHubConnectionEventsTestHub> logger
) : StreamingHubBase<IStreamingHubConnectionEventsTestHub, IStreamingHubConnectionEventsTestHubReceiver>, IStreamingHubConnectionEventsTestHub
{
    protected override ValueTask OnConnecting()
    {
        logger.LogInformation("E/OnConnecting");
        items.TryAdd("OnConnecting", true);
        if (Context.CallContext.GetHttpContext().Request.Headers
            .TryGetValue("ThrowsRSEOnConnecting", out var value) && value == "1")
        {
            throw new ReturnStatusException(StatusCode.FailedPrecondition, "Some error");
        }
        return base.OnConnecting();
    }
    protected override ValueTask OnConnected()
    {
        logger.LogInformation("E/OnConnected");
        items.TryAdd("OnConnected", true);
        if (Context.CallContext.GetHttpContext().Request.Headers
                .TryGetValue("ThrowsRSEOnConnected", out var value) && value == "1")
        {
            throw new ReturnStatusException(StatusCode.FailedPrecondition, "Some error");
        }
        return base.OnConnected();
    }

    protected override ValueTask OnDisconnected()
    {
        logger.LogInformation("E/OnDisconnected");
        items.TryAdd("OnDisconnected", true);
        return base.OnDisconnected();
    }

    public ValueTask<string> HelloAsync()
    {
        return ValueTask.FromResult("Hello");
    }
}
