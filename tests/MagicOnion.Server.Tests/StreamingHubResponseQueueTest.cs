using System.Collections.Concurrent;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace MagicOnion.Server.Tests;

/// <summary>
/// Tests response queue overflow through an ASP.NET Core gRPC call.
/// </summary>
public class StreamingHubResponseQueueTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubResponseQueueTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubResponseQueueTestHub> factory;

    /// <summary>
    /// Creates the test with its application factory.
    /// </summary>
    public StreamingHubResponseQueueTest(MagicOnionApplicationFactory<StreamingHubResponseQueueTestHub> factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// Overflow aborts the affected call, runs disconnection cleanup, and leaves another call usable.
    /// </summary>
    [Fact]
    public async Task OversizedBroadcast_DisconnectsOnlyAffectedHub()
    {
        using var configuredFactory = factory.WithMagicOnionOptions(options =>
        {
            options.StreamingHubResponseQueueMaxLength = 16;
            options.StreamingHubResponseQueueMaxSize = 64;
        });
        var disconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        configuredFactory.Items["OverflowDisconnected"] = disconnected;
        using var httpClient = configuredFactory.CreateDefaultClient();
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = httpClient });
        var affected = await StreamingHubClient.ConnectAsync<IStreamingHubResponseQueueTestHub, IStreamingHubResponseQueueTestReceiver>(
            channel, Substitute.For<IStreamingHubResponseQueueTestReceiver>(), cancellationToken: TestContext.Current.CancellationToken);
        var healthy = await StreamingHubClient.ConnectAsync<IStreamingHubResponseQueueTestHub, IStreamingHubResponseQueueTestReceiver>(
            channel, Substitute.For<IStreamingHubResponseQueueTestReceiver>(), cancellationToken: TestContext.Current.CancellationToken);
        try
        {
            await affected.FireAndForget().SendOversizedAsync();
            await affected.WaitForDisconnect().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(123, await healthy.PingAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        }
        finally
        {
            await affected.DisposeAsync();
            await healthy.DisposeAsync();
        }
    }
}

/// <summary>
/// Provides a small response and a broadcast that exceeds the test's byte limit.
/// </summary>
public interface IStreamingHubResponseQueueTestHub : IStreamingHub<IStreamingHubResponseQueueTestHub, IStreamingHubResponseQueueTestReceiver>
{
    /// <summary>Sends an oversized broadcast to this client.</summary>
    Task SendOversizedAsync();
    /// <summary>Returns a small response.</summary>
    Task<int> PingAsync();
}

/// <summary>
/// Receives test broadcasts.
/// </summary>
public interface IStreamingHubResponseQueueTestReceiver
{
    /// <summary>Receives a payload.</summary>
    void OnPayload(byte[] payload);
}

/// <summary>
/// Exercises response queue limits using the normal remote receiver path.
/// </summary>
public class StreamingHubResponseQueueTestHub(
    [FromKeyedServices(MagicOnionApplicationFactory.ItemsKey)] ConcurrentDictionary<string, object> items)
    : StreamingHubBase<IStreamingHubResponseQueueTestHub, IStreamingHubResponseQueueTestReceiver>, IStreamingHubResponseQueueTestHub
{
    bool sentOversized;

    /// <inheritdoc />
    public Task SendOversizedAsync()
    {
        sentOversized = true;
        Client.OnPayload(new byte[128]);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> PingAsync() => Task.FromResult(123);

    /// <inheritdoc />
    protected override ValueTask OnDisconnected()
    {
        if (sentOversized) ((TaskCompletionSource)items["OverflowDisconnected"]).TrySetResult();
        return default;
    }
}
