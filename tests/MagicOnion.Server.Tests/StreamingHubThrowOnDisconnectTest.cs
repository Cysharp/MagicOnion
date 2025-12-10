using System.Collections.Concurrent;
using Cysharp.Runtime.Multicast.Remoting;
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

public class StreamingHubThrowOnDisconnectTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubThrowOnDisconnectTestHub>>
{
    readonly WebApplicationFactory<Program> factory;

    public StreamingHubThrowOnDisconnectTest(MagicOnionApplicationFactory<StreamingHubThrowOnDisconnectTestHub> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var desc = services.Single(x => x.ServiceType == typeof(IRemoteClientResultPendingTaskRegistry));
                services.Remove(desc);
                services.AddSingleton<IRemoteClientResultPendingTaskRegistry>(sp => sp.GetRequiredService<RemoteClientResultPendingTaskRegistryWrapper>());
                services.AddSingleton<RemoteClientResultPendingTaskRegistryWrapper>(sp =>
                    new RemoteClientResultPendingTaskRegistryWrapper(
                        (IRemoteClientResultPendingTaskRegistry)(desc.ImplementationInstance ?? desc.ImplementationFactory(sp))));
            });
        });
        this.factory.Initialize();
    }

    [Fact]
    public async Task ThrowOnDisconnect()
    {
        var pendingTaskRegistry = factory.Server.Services.GetRequiredService<RemoteClientResultPendingTaskRegistryWrapper>();

        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() {HttpClient = httpClient});
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubThrowOnDisconnectTestHub, IStreamingHubThrowOnDisconnectTestHubReceiver>(
            channel,
            Substitute.For<IStreamingHubThrowOnDisconnectTestHubReceiver>());

        await client.DoWorkAsync();
        await client.DisposeAsync();
        channel.Dispose();
        httpClient.Dispose();

        await Task.Delay(100);

        var logs = this.factory.Logs.GetSnapshot();
        Assert.True(logs.Any(x => x.Message == "OnDisconnected"));
        Assert.True(pendingTaskRegistry.IsDisposed);
    }
}

file class RemoteClientResultPendingTaskRegistryWrapper(IRemoteClientResultPendingTaskRegistry inner) : IRemoteClientResultPendingTaskRegistry
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        inner.Dispose();
        IsDisposed = true;
    }

    public void Register(PendingTask pendingTask)
    {
        inner.Register(pendingTask);
    }

    public bool TryGetAndUnregisterPendingTask(Guid messageId, out PendingTask pendingTask)
    {
        return inner.TryGetAndUnregisterPendingTask(messageId, out pendingTask);
    }

    public PendingTask CreateTask<TResult>(string methodName, int methodId, Guid messageId, TaskCompletionSource<TResult> taskCompletionSource, CancellationToken timeoutCancellationToken, IRemoteSerializer serializer)
    {
        return inner.CreateTask(methodName, methodId, messageId, taskCompletionSource, timeoutCancellationToken, serializer);
    }

    public PendingTask CreateTask(string methodName, int methodId, Guid messageId, TaskCompletionSource taskCompletionSource, CancellationToken timeoutCancellationToken, IRemoteSerializer serializer)
    {
        return inner.CreateTask(methodName, methodId, messageId, taskCompletionSource, timeoutCancellationToken, serializer);
    }
}

public interface IStreamingHubThrowOnDisconnectTestHub : IStreamingHub<IStreamingHubThrowOnDisconnectTestHub, IStreamingHubThrowOnDisconnectTestHubReceiver>
{
    Task DoWorkAsync();
}

public interface IStreamingHubThrowOnDisconnectTestHubReceiver;

public class StreamingHubThrowOnDisconnectTestHub(ILogger<StreamingHubThrowOnDisconnectTestHub> logger) :
        StreamingHubBase<IStreamingHubThrowOnDisconnectTestHub, IStreamingHubThrowOnDisconnectTestHubReceiver>,
        IStreamingHubThrowOnDisconnectTestHub
{
    public async Task DoWorkAsync()
    {
        logger.LogInformation("DoWorkAsync");
    }

    protected override ValueTask OnDisconnected()
    {
        logger.LogInformation("OnDisconnected");
        throw new Exception("Something went wrong");
    }
}
