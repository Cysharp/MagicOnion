using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class StreamingHubConnectionEventsTest: IClassFixture<MagicOnionApplicationFactory<StreamingHubConnectionEventsTestHub>>
{
    readonly WebApplicationFactory<Program> factory;

    public StreamingHubConnectionEventsTest(MagicOnionApplicationFactory<StreamingHubConnectionEventsTestHub> factory)
    {
        this.factory = factory;
        this.factory.Initialize();
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

        Assert.True((bool)factory.Items.GetValueOrDefault("OnConnecting", false));
        Assert.True((bool)factory.Items.GetValueOrDefault("OnConnected", false));
        Assert.True((bool)factory.Items.GetValueOrDefault("OnDisconnected", false));
        Assert.Equal(["E/OnConnecting", "E/OnConnected", "E/OnDisconnected"], factory.Logs.GetSnapshot().Select(x => x.Message).Where(x => x.StartsWith("E/")));
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

        var logSnapshot = factory.Logs.GetSnapshot();
        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Contains(logSnapshot.Select(x => x.Message), x => x == "E/OnConnecting");
        Assert.DoesNotContain(logSnapshot.Select(x => x.Message), x => x == "E/OnConnected");
        Assert.DoesNotContain(logSnapshot.Select(x => x.Message), x => x == "Error");
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
        var logSnapshot = factory.Logs.GetSnapshot();
        Assert.Contains("StreamingHubClient has already been disconnected from the server", ex.Message);
        Assert.Equal(StatusCode.Unavailable, ex.StatusCode);
        Assert.Contains(logSnapshot.Select(x => x.Message), x => x == "E/OnConnecting");
        Assert.Contains(logSnapshot.Select(x => x.Message), x => x == "E/OnConnected");
        Assert.DoesNotContain(logSnapshot.Select(x => x.Message), x => x == "Error");

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
