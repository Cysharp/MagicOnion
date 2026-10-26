#nullable enable

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Features;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class AdditionalTransportApplicationFactory : MagicOnionApplicationFactory
{
    protected override IEnumerable<Type> GetServiceImplementationTypes()
    {
        yield return typeof(AdditionalTransportTestHub);
        yield return typeof(ReliableOnlyTransportTestHub);
    }
}

/// <summary>
/// Verifies the additional-transport negotiation between a real client and server. All messages still use gRPC.
/// </summary>
public class StreamingHubAdditionalTransportTest(AdditionalTransportApplicationFactory factory) : IClassFixture<AdditionalTransportApplicationFactory>
{
    WebApplicationFactory<Program> CreateServer(Action<IMagicOnionServerBuilder>? configure = null, Action<MagicOnionOptions>? configureOptions = null)
        => factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            var magicOnion = services.AddMagicOnion();
            configure?.Invoke(magicOnion);
            if (configureOptions is not null) services.Configure(configureOptions);
        }));

    static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(WebApplicationFactory<Program> server, AdditionalTransportEstablishment establishment, params IAdditionalTransportClientProvider[] providers)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        where TReceiver : class
    {
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = server.CreateDefaultClient() });
        var options = StreamingHubClientOptions.CreateWithDefault().WithAdditionalTransport(new AdditionalTransportClientOptions(providers, establishment));
        return await StreamingHubClient.ConnectAsync<TStreamingHub, TReceiver>(channel, Substitute.For<TReceiver>(), options, DynamicStreamingHubClientFactoryProvider.Instance, TestContext.Current.CancellationToken);
    }

    static Task<IAdditionalTransportTestHub> ConnectAsync(WebApplicationFactory<Program> server, AdditionalTransportEstablishment establishment, params IAdditionalTransportClientProvider[] providers)
        => ConnectAsync<IAdditionalTransportTestHub, IAdditionalTransportTestHubReceiver>(server, establishment, providers);

    /// <summary>
    /// Waits until the server finished <c>OnConnected</c> and returns the transport information the hub sees.
    /// </summary>
    /// <remarks>
    /// <c>ConnectAsync</c> completes when the client receives the final result, which the server sends before it calls <c>OnConnected</c>.
    /// The server dispatches hub methods only after <c>OnConnected</c> completed, so a response to a hub method proves that it ran.
    /// </remarks>
    static Task<string> WaitForOnConnectedAsync(IAdditionalTransportTestHub client)
        => client.GetTransportInfoAsync();

    static async Task WaitUntilAsync(Func<bool> condition)
    {
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < timeout)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        Assert.True(condition());
    }

    [Fact]
    public async Task Active_BothPeersFinalizeTheSameResult()
    {
        var serverProvider = new FakeServerProvider("fake", 1);
        using var server = CreateServer(x => x.AddAdditionalTransportProvider(serverProvider));

        var client = await ConnectAsync(server, AdditionalTransportEstablishment.Required, new FakeClientProvider("fake", 1));

        var info = client.GetTransportInfo();
        Assert.Equal(StreamingHubTransportState.ActiveAdditionalTransport, info.State);
        Assert.Equal("fake", info.AdditionalTransportProviderId);
        Assert.Equal(1, info.AdditionalTransportProviderVersion);

        // Unreliable and reliable methods keep working over gRPC. Requests are processed in order.
        await client.MoveAsync(5);
        Assert.Equal(info.ToString(), await WaitForOnConnectedAsync(client));
        Assert.Equal(info.ToString(), server.Items["OnConnected"]);
        Assert.Equal(5, server.Items["MoveAsync"]);

        var candidate = Assert.Single(serverProvider.Candidates);
        Assert.True(candidate.IsActivated);
        Assert.False(candidate.IsDisposed);

        // The activated transport lives as long as the connection.
        await client.DisposeAsync();
        await WaitUntilAsync(() => candidate.IsDisposed);
    }

    [Fact]
    public async Task ServerWithoutProvider_Preferred_FallsBack()
    {
        using var server = CreateServer();

        var client = await ConnectAsync(server, AdditionalTransportEstablishment.Preferred, new FakeClientProvider("fake", 1));

        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        Assert.Equal(nameof(StreamingHubTransportState.GrpcFallback), await WaitForOnConnectedAsync(client));
        Assert.Equal(nameof(StreamingHubTransportState.GrpcFallback), server.Items["OnConnected"]);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task ServerWithoutProvider_Required_FailsBeforeOnConnected()
    {
        using var server = CreateServer();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await ConnectAsync(server, AdditionalTransportEstablishment.Required, new FakeClientProvider("fake", 1)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.False(server.Items.ContainsKey("OnConnected"));
    }

    [Fact]
    public async Task ClientCandidateFails_ServerSelectsNextCandidate()
    {
        var first = new FakeServerProvider("first", 1);
        var second = new FakeServerProvider("second", 1);
        using var server = CreateServer(x => x.AddAdditionalTransportProvider(first).AddAdditionalTransportProvider(second));

        var client = await ConnectAsync(server, AdditionalTransportEstablishment.Preferred, new FakeClientProvider("first", 1, fails: true), new FakeClientProvider("second", 1));

        Assert.Equal("second", client.GetTransportInfo().AdditionalTransportProviderId);
        Assert.Equal(client.GetTransportInfo().ToString(), await WaitForOnConnectedAsync(client));
        Assert.Equal(client.GetTransportInfo().ToString(), server.Items["OnConnected"]);
        Assert.True(Assert.Single(first.Candidates).IsDisposed);
        Assert.True(Assert.Single(second.Candidates).IsActivated);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task ServerRejectsActivation_Preferred_FallsBack()
    {
        var serverProvider = new FakeServerProvider("fake", 1, activates: false);
        using var server = CreateServer(x => x.AddAdditionalTransportProvider(serverProvider));

        var client = await ConnectAsync(server, AdditionalTransportEstablishment.Preferred, new FakeClientProvider("fake", 1));

        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        Assert.Equal(nameof(StreamingHubTransportState.GrpcFallback), await WaitForOnConnectedAsync(client));
        Assert.Equal(nameof(StreamingHubTransportState.GrpcFallback), server.Items["OnConnected"]);
        Assert.True(Assert.Single(serverProvider.Candidates).IsDisposed);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task ServerRejectsActivation_Required_Fails()
    {
        using var server = CreateServer(x => x.AddAdditionalTransportProvider(new FakeServerProvider("fake", 1, activates: false)));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await ConnectAsync(server, AdditionalTransportEstablishment.Required, new FakeClientProvider("fake", 1)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.False(server.Items.ContainsKey("OnConnected"));
    }

    [Fact]
    public async Task ServerDeadlineExpires_FallsBack()
    {
        var timeProvider = new FakeTimeProvider();
        var establishing = new TaskCompletionSource();
        var clientProvider = new FakeClientProvider("fake", 1, onEstablish: async ct =>
        {
            establishing.TrySetResult();
            await Task.Delay(Timeout.Infinite, ct);
        });
        using var server = CreateServer(
            x => x.AddAdditionalTransportProvider(new FakeServerProvider("fake", 1)).ConfigureAdditionalTransport(o => o.NegotiationTimeout = TimeSpan.FromSeconds(5)),
            o => o.TimeProvider = timeProvider);

        var connectTask = ConnectAsync(server, AdditionalTransportEstablishment.Preferred, clientProvider);
        await establishing.Task.WaitAsync(TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromSeconds(6));
        var client = await connectTask.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        Assert.Equal(nameof(StreamingHubTransportState.GrpcFallback), await WaitForOnConnectedAsync(client));
        Assert.Equal(nameof(StreamingHubTransportState.GrpcFallback), server.Items["OnConnected"]);
        Assert.True(Assert.Single(clientProvider.Candidates).IsDisposed);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task ContractWithoutUnreliableMethods_DoesNotNegotiate()
    {
        var serverProvider = new FakeServerProvider("fake", 1);
        using var server = CreateServer(x => x.AddAdditionalTransportProvider(serverProvider));

        var client = await ConnectAsync<IReliableOnlyTransportTestHub, IReliableOnlyTransportTestHubReceiver>(server, AdditionalTransportEstablishment.Required, new FakeClientProvider("fake", 1));

        Assert.Equal(StreamingHubTransportState.GrpcOnly, client.GetTransportInfo().State);
        Assert.Equal(nameof(StreamingHubTransportState.GrpcOnly), await client.GetTransportInfoAsync());
        Assert.Equal("none", server.Items["OnConnected"]);
        Assert.Empty(serverProvider.Candidates);
        await client.DisposeAsync();
    }

    [Fact]
    public void DuplicateServerProviders_IsConfigurationError()
    {
        using var server = CreateServer(x => x.AddAdditionalTransportProvider(new FakeServerProvider("fake", 1)).AddAdditionalTransportProvider(new FakeServerProvider("fake", 1)));

        var ex = Assert.ThrowsAny<Exception>(() => server.CreateDefaultClient());

        // The registry validates the providers when the hub is mapped at startup.
        var messages = new List<string>();
        for (var current = ex; current is not null; current = current.InnerException) messages.Add(current.Message);
        Assert.Contains(messages, x => x.Contains("registered more than once"));
    }

    sealed class FakeClientProvider(string id, int version, bool fails = false, Func<CancellationToken, Task>? onEstablish = null) : IAdditionalTransportClientProvider
    {
        public string Id => id;
        public int Version => version;
        public List<FakeClientCandidate> Candidates { get; } = new();

        public bool TryCreateCandidate([NotNullWhen(true)] out IAdditionalTransportClientCandidate? candidate)
        {
            var fakeCandidate = new FakeClientCandidate(fails, onEstablish);
            Candidates.Add(fakeCandidate);
            candidate = fakeCandidate;
            return true;
        }
    }

    sealed class FakeClientCandidate(bool fails, Func<CancellationToken, Task>? onEstablish) : IAdditionalTransportClientCandidate
    {
        public ReadOnlyMemory<byte> Offer => new byte[] { 1 };
        public bool IsDisposed { get; private set; }

        public async ValueTask<ReadOnlyMemory<byte>> EstablishAsync(ReadOnlyMemory<byte> selection, CancellationToken cancellationToken)
        {
            if (onEstablish is not null) await onEstablish(cancellationToken);
            if (fails) throw new InvalidOperationException("The path is unreachable.");
            // Return the server's token, as a path validation would.
            return selection.ToArray();
        }

        public void OnActivated() { }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return default;
        }
    }

    sealed class FakeServerProvider(string id, int version, bool activates = true) : IAdditionalTransportServerProvider
    {
        public string Id => id;
        public int Version => version;
        public ConcurrentQueue<FakeServerCandidate> Candidates { get; } = new();

        public ValueTask<AdditionalTransportAcceptResult> TryAcceptAsync(ReadOnlyMemory<byte> offer, AdditionalTransportServerContext context, CancellationToken cancellationToken)
        {
            var candidate = new FakeServerCandidate(activates);
            Candidates.Enqueue(candidate);
            return new(AdditionalTransportAcceptResult.Accepted(candidate));
        }
    }

    sealed class FakeServerCandidate(bool activates) : IAdditionalTransportServerCandidate
    {
        readonly byte[] token = Guid.NewGuid().ToByteArray();

        public ReadOnlyMemory<byte> Selection => token;
        public bool IsActivated { get; private set; }
        public bool IsDisposed { get; private set; }

        public ValueTask<bool> TryActivateAsync(ReadOnlyMemory<byte> activation, CancellationToken cancellationToken)
        {
            IsActivated = activates && activation.Span.SequenceEqual(token);
            return new ValueTask<bool>(IsActivated);
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return default;
        }
    }
}

public interface IAdditionalTransportTestHubReceiver
{
    [Unreliable] void OnPosition(int x);
    void OnMessage(string message);
}

public interface IAdditionalTransportTestHub : IStreamingHub<IAdditionalTransportTestHub, IAdditionalTransportTestHubReceiver>
{
    [Unreliable] Task MoveAsync(int x);
    Task<string> GetTransportInfoAsync();
}

public class AdditionalTransportTestHub([FromKeyedServices(MagicOnionApplicationFactory.ItemsKey)] ConcurrentDictionary<string, object> items)
    : StreamingHubBase<IAdditionalTransportTestHub, IAdditionalTransportTestHubReceiver>, IAdditionalTransportTestHub
{
    protected override ValueTask OnConnected()
    {
        var feature = Context.CallContext.GetHttpContext().Features.Get<IStreamingHubTransportFeature>();
        items["OnConnected"] = feature is null ? "none" : feature.TransportInfo.ToString();
        return CompletedTask;
    }

    public Task MoveAsync(int x)
    {
        items["MoveAsync"] = x;
        return Task.CompletedTask;
    }

    public Task<string> GetTransportInfoAsync()
        => Task.FromResult(TransportInfo.ToString());
}

public interface IReliableOnlyTransportTestHubReceiver
{
    void OnMessage(string message);
}

public interface IReliableOnlyTransportTestHub : IStreamingHub<IReliableOnlyTransportTestHub, IReliableOnlyTransportTestHubReceiver>
{
    Task<string> GetTransportInfoAsync();
}

public class ReliableOnlyTransportTestHub([FromKeyedServices(MagicOnionApplicationFactory.ItemsKey)] ConcurrentDictionary<string, object> items)
    : StreamingHubBase<IReliableOnlyTransportTestHub, IReliableOnlyTransportTestHubReceiver>, IReliableOnlyTransportTestHub
{
    protected override ValueTask OnConnected()
    {
        var feature = Context.CallContext.GetHttpContext().Features.Get<IStreamingHubTransportFeature>();
        items["OnConnected"] = feature is null ? "none" : feature.TransportInfo.ToString();
        return CompletedTask;
    }

    public Task<string> GetTransportInfoAsync()
        => Task.FromResult(TransportInfo.ToString());
}
