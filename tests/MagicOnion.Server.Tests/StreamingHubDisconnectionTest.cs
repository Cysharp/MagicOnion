using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class StreamingHubDisconnectionTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubDisconnectionTestHub>>
{
    readonly ConcurrentBag<string> logs;
    readonly WebApplicationFactory<Program> factory;

    public StreamingHubDisconnectionTest(MagicOnionApplicationFactory<StreamingHubDisconnectionTestHub> factory)
    {
        factory.Initialize();
        this.factory = factory;
        this.logs = factory.Logs;
    }

    [Fact]
    public async Task DisconnectWhileConsumingHubMethodQueue()
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() {HttpClient = httpClient});
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubDisconnectionTestHub, IStreamingHubDisconnectionTestHubReceiver>(
            channel,
            Substitute.For<IStreamingHubDisconnectionTestHubReceiver>());

        client.DoWorkAsync();
        await Task.Delay(50);
        client.DoWorkAsync(); // 150ms
        client.DoWorkAsync(); // 250ms
        client.DoWorkAsync(); // 350ms

        await client.DisposeAsync();
        channel.Dispose();
        httpClient.Dispose();

        await Task.Delay(500);

        var doWorkAsyncCallCount = logs.Count(x => x.Contains("DoWorkAsync:Begin"));
        var doWorkAsyncDoneCallCount = logs.Count(x => x.Contains("DoWorkAsync:Done"));
        Assert.True(doWorkAsyncCallCount < 3);
        Assert.True(doWorkAsyncCallCount == doWorkAsyncDoneCallCount);
    }
}

public interface IStreamingHubDisconnectionTestHub : IStreamingHub<IStreamingHubDisconnectionTestHub, IStreamingHubDisconnectionTestHubReceiver>
{
    Task DoWorkAsync();
}

public interface IStreamingHubDisconnectionTestHubReceiver;

public class StreamingHubDisconnectionTestHub(ILogger<StreamingHubDisconnectionTestHub> logger) :
        StreamingHubBase<IStreamingHubDisconnectionTestHub, IStreamingHubDisconnectionTestHubReceiver>,
        IStreamingHubDisconnectionTestHub
{
    public async Task DoWorkAsync()
    {
        logger.LogInformation("DoWorkAsync:Begin");
        await Task.Delay(100);
        _ = this.Context.CallContext.GetHttpContext().Features;
        logger.LogInformation("DoWorkAsync:Done");
    }
}
