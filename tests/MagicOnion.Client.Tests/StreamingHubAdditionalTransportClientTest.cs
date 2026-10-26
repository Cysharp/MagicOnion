using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Internal;
using Microsoft.Extensions.Time.Testing;

namespace MagicOnion.Client.Tests;

/// <summary>
/// Verifies the client side of the additional-transport negotiation against a simulated server.
/// </summary>
public class StreamingHubAdditionalTransportClientTest
{
    static readonly byte[] SelectionData = [7, 7];
    static readonly byte[] ActivationData = [9];

    static StreamingHubTransportModeTable HubTable => StreamingHubTransportContract.ForHub<IUnreliableTestHub>().UnreliableMethods;
    static StreamingHubTransportModeTable ReceiverTable => StreamingHubTransportContract.ForReceiver<IUnreliableTestHubReceiver>().UnreliableMethods;

    static StreamingHubClientTestHelper<IUnreliableTestHub, IUnreliableTestHubReceiver> CreateHelper(AdditionalTransportNegotiationResponse? response)
        => new(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance, onResponseHeaderAsync: metadata =>
        {
            if (response is not null)
            {
                metadata.Add(AdditionalTransportNegotiation.MetadataKey, AdditionalTransportNegotiation.EncodeResponse(response));
            }
            return Task.CompletedTask;
        });

    static AdditionalTransportNegotiationResponse SelectFirst()
        => AdditionalTransportNegotiationResponse.Selected(1, 0, SelectionData, HubTable, ReceiverTable);

    static StreamingHubClientOptions CreateOptions(AdditionalTransportEstablishment establishment, params IAdditionalTransportClientProvider[] providers)
        => StreamingHubClientOptions.CreateWithDefault().WithAdditionalTransport(new AdditionalTransportClientOptions(providers, establishment));

    static void WriteControl<THub, TReceiver>(StreamingHubClientTestHelper<THub, TReceiver> helper, AdditionalTransportControlMessageKind kind, int attemptId, int code = 0, byte[]? data = null)
        where THub : IStreamingHub<THub, TReceiver>
        where TReceiver : class
    {
        var buffer = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromServer(buffer, kind, attemptId, code, data ?? ReadOnlySpan<byte>.Empty);
        helper.WriteResponseRaw(buffer.WrittenSpan);
    }

    static async Task<(AdditionalTransportControlMessageKind Kind, int AttemptId, int Code, byte[] Data)> ReadControlAsync<THub, TReceiver>(StreamingHubClientTestHelper<THub, TReceiver> helper, CancellationToken cancellationToken)
        where THub : IStreamingHub<THub, TReceiver>
        where TReceiver : class
    {
        return ParseControl(await helper.ReadRequestRawAsync().WaitAsync(cancellationToken));

        static (AdditionalTransportControlMessageKind Kind, int AttemptId, int Code, byte[] Data) ParseControl(ReadOnlyMemory<byte> raw)
        {
            var reader = new StreamingHubServerMessageReader(raw);
            Assert.Equal(StreamingHubMessageType.AdditionalTransportControl, reader.ReadMessageType());
            var message = reader.ReadAdditionalTransportControl();
            return (message.Kind, message.AttemptId, message.Code, message.Data.ToArray());
        }
    }

    static AdditionalTransportNegotiationRequest? GetSentRequest<THub, TReceiver>(StreamingHubClientTestHelper<THub, TReceiver> helper)
        where THub : IStreamingHub<THub, TReceiver>
        where TReceiver : class
    {
        var entry = helper.LastCallOptions?.Headers?.GetOrDefault(AdditionalTransportNegotiation.MetadataKey);
        if (entry is null) return null;
        Assert.Equal(AdditionalTransportRequestDecodeResult.Success, AdditionalTransportNegotiation.DecodeRequest(entry.ValueBytes, out var request, out _));
        return request;
    }

    [Fact]
    public async Task NoAdditionalTransport_SendsNoMetadata()
    {
        var helper = CreateHelper(null);

        var client = await helper.ConnectAsync(TestContext.Current.CancellationToken);

        Assert.Null(GetSentRequest(helper));
        Assert.Equal(StreamingHubTransportState.GrpcOnly, client.GetTransportInfo().State);
    }

    [Fact]
    public async Task TryGetTransportInfo_ReturnsFalseForObjectsWithoutTransportInformation()
    {
        var helper = CreateHelper(null);
        var client = await helper.ConnectAsync(TestContext.Current.CancellationToken);

        Assert.True(client.TryGetTransportInfo(out var info));
        Assert.Equal(StreamingHubTransportState.GrpcOnly, info.State);

        // The FireAndForget proxy does not hold the connection state.
        var fireAndForget = client.FireAndForget();
        Assert.False(fireAndForget.TryGetTransportInfo(out var fireAndForgetInfo));
        Assert.Equal(StreamingHubTransportState.GrpcOnly, fireAndForgetInfo.State);
        Assert.Throws<InvalidOperationException>(() => fireAndForget.GetTransportInfo());
    }

    [Fact]
    public async Task ContractWithoutUnreliableMethods_SendsNoMetadata()
    {
        var provider = new FakeClientProvider("fake", 1);
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);

        var client = await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Required, provider), TestContext.Current.CancellationToken);

        Assert.Null(GetSentRequest(helper));
        Assert.Empty(provider.Candidates);
        Assert.Equal(StreamingHubTransportState.GrpcOnly, client.GetTransportInfo().State);
    }

    [Fact]
    public async Task NoApplicableProvider_Preferred_SendsNoMetadata()
    {
        var provider = new FakeClientProvider("fake", 1, isApplicable: false);
        var helper = CreateHelper(null);

        var client = await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), TestContext.Current.CancellationToken);

        Assert.Null(GetSentRequest(helper));
        Assert.Equal(StreamingHubTransportState.GrpcOnly, client.GetTransportInfo().State);
    }

    [Fact]
    public async Task NoApplicableProvider_Required_FailsWithoutStartingCall()
    {
        var provider = new FakeClientProvider("fake", 1, isApplicable: false);
        var helper = CreateHelper(null);

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Required, provider), TestContext.Current.CancellationToken));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Null(helper.LastCallOptions);
    }

    [Fact]
    public async Task Offer_ContainsTablesOffersAndOriginalHeaders()
    {
        var provider = new FakeClientProvider("fake", 3);
        var helper = CreateHelper(AdditionalTransportNegotiationResponse.Fallback(AdditionalTransportNegotiationReason.NoMatchingProvider));
        var options = CreateOptions(AdditionalTransportEstablishment.Preferred, provider)
            .WithCallOptions(new CallOptions(new Metadata { { "x-custom", "1" } }));

        await helper.ConnectAsync(options, TestContext.Current.CancellationToken);

        var request = GetSentRequest(helper);
        Assert.NotNull(request);
        Assert.Equal(AdditionalTransportEstablishment.Preferred, request.Establishment);
        Assert.True(request.ClientToServerMethods.ContentEquals(HubTable));
        Assert.True(request.ServerToClientMethods.ContentEquals(ReceiverTable));
        var offer = Assert.Single(request.Offers);
        Assert.Equal("fake", offer.ProviderId);
        Assert.Equal(3, offer.ProviderVersion);
        Assert.Equal(FakeClientCandidate.DefaultOffer, offer.Data.ToArray());
        Assert.Equal("1", helper.LastCallOptions!.Value.Headers!.GetValueOrDefault("x-custom"));
        // The caller's metadata is not modified.
        Assert.Null(options.CallOptions.Headers!.GetOrDefault(AdditionalTransportNegotiation.MetadataKey));
    }

    [Fact]
    public Task DynamicClient_EmbedsReceiverTable_WhenOnlyReceiverHasUnreliableMethods()
        => AssertDynamicClientEmbedsTablesAsync<IUnreliableReceiverOnlyTestHub, IUnreliableReceiverTestHubReceiver>(expectHubMethods: false);

    [Fact]
    public Task DynamicClient_EmbedsBothTables()
        => AssertDynamicClientEmbedsTablesAsync<IUnreliableBothDirectionsTestHub, IUnreliableReceiverTestHubReceiver>(expectHubMethods: true);

    static async Task AssertDynamicClientEmbedsTablesAsync<THub, TReceiver>(bool expectHubMethods)
        where THub : IStreamingHub<THub, TReceiver>
        where TReceiver : class
    {
        var helper = new StreamingHubClientTestHelper<THub, TReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);

        await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, new FakeClientProvider("fake", 1)), TestContext.Current.CancellationToken);

        // The dynamic client overrides only the properties of non-empty tables, and the base class reads them for the request.
        var request = GetSentRequest(helper);
        Assert.NotNull(request);
        Assert.Equal(expectHubMethods, request.ClientToServerMethods.Count != 0);
        Assert.NotEqual(0, request.ServerToClientMethods.Count);
        Assert.True(request.ClientToServerMethods.ContentEquals(StreamingHubTransportContract.ForHub<THub>().UnreliableMethods));
        Assert.True(request.ServerToClientMethods.ContentEquals(StreamingHubTransportContract.ForReceiver<TReceiver>().UnreliableMethods));
    }

    [Fact]
    public async Task OlderServer_Preferred_FallsBackWithoutControlMessages()
    {
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(null);

        var client = await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), TestContext.Current.CancellationToken);

        Assert.NotNull(GetSentRequest(helper));
        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
        Assert.False(helper.HasPendingRequest);
    }

    [Fact]
    public async Task OlderServer_Required_Fails()
    {
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(null);

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Required, provider), TestContext.Current.CancellationToken));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
    }

    [Fact]
    public async Task ServerFallback_FinalizesGrpcFallback()
    {
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(AdditionalTransportNegotiationResponse.Fallback(AdditionalTransportNegotiationReason.NoAgreedMethods));

        var client = await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), TestContext.Current.CancellationToken);

        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
    }

    [Fact]
    public async Task ServerFailed_Throws()
    {
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(AdditionalTransportNegotiationResponse.Failed(AdditionalTransportNegotiationReason.NoMatchingProvider));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Required, provider), TestContext.Current.CancellationToken));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Contains(nameof(AdditionalTransportNegotiationReason.NoMatchingProvider), ex.Status.Detail);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
    }

    [Fact]
    public async Task Selected_Activated_FinalizesActive()
    {
        var timeout = TestContext.Current.CancellationToken;
        var provider = new FakeClientProvider("fake", 2);
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Required, provider), timeout);

        var activate = await ReadControlAsync(helper, timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.Activate, activate.Kind);
        Assert.Equal(1, activate.AttemptId);
        Assert.Equal(ActivationData, activate.Data);
        Assert.False(connectTask.IsCompleted);

        WriteControl(helper, AdditionalTransportControlMessageKind.Activated, 1);
        var client = await connectTask.WaitAsync(timeout);

        var info = client.GetTransportInfo();
        Assert.Equal(StreamingHubTransportState.ActiveAdditionalTransport, info.State);
        Assert.Equal("fake", info.AdditionalTransportProviderId);
        Assert.Equal(2, info.AdditionalTransportProviderVersion);
        Assert.True(client.TryGetTransportInfo(out var triedInfo));
        Assert.Equal(info.ToString(), triedInfo.ToString());
        var candidate = Assert.Single(provider.Candidates);
        Assert.Equal(SelectionData, candidate.ReceivedSelection);
        Assert.True(candidate.IsActivated);
        Assert.False(candidate.IsDisposed);

        // The activated candidate lives as long as the connection.
        await client.DisposeAsync();
        Assert.True(candidate.IsDisposed);
    }

    [Fact]
    public async Task Selected_EstablishmentFails_ServerSelectsNext()
    {
        var timeout = TestContext.Current.CancellationToken;
        var failing = new FakeClientProvider("first", 1, establish: static (_, _) => throw new InvalidOperationException("Unreachable"));
        var working = new FakeClientProvider("second", 1);
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, failing, working), timeout);

        var failed = await ReadControlAsync(helper, timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.CandidateFailed, failed.Kind);
        Assert.Equal(1, failed.AttemptId);
        Assert.Equal((int)AdditionalTransportNegotiationReason.EstablishmentFailed, failed.Code);

        WriteControl(helper, AdditionalTransportControlMessageKind.SelectNext, 2, code: 1, data: [5]);
        var activate = await ReadControlAsync(helper, timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.Activate, activate.Kind);
        Assert.Equal(2, activate.AttemptId);

        WriteControl(helper, AdditionalTransportControlMessageKind.Activated, 2);
        var client = await connectTask.WaitAsync(timeout);

        Assert.Equal("second", client.GetTransportInfo().AdditionalTransportProviderId);
        Assert.True(Assert.Single(failing.Candidates).IsDisposed);
        Assert.Equal(new byte[] { 5 }, Assert.Single(working.Candidates).ReceivedSelection);
        Assert.True(working.Candidates[0].IsActivated);
    }

    [Fact]
    public async Task Selected_ActivationRejected_GrpcSelected_FinalizesGrpcFallback()
    {
        var timeout = TestContext.Current.CancellationToken;
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), timeout);

        Assert.Equal(AdditionalTransportControlMessageKind.Activate, (await ReadControlAsync(helper, timeout)).Kind);
        WriteControl(helper, AdditionalTransportControlMessageKind.GrpcSelected, 1, code: (int)AdditionalTransportNegotiationReason.CandidatesExhausted);
        var client = await connectTask.WaitAsync(timeout);

        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        var candidate = Assert.Single(provider.Candidates);
        Assert.False(candidate.IsActivated);
        Assert.True(candidate.IsDisposed);
    }

    [Fact]
    public async Task Selected_GrpcSelectedWhileEstablishing_CancelsEstablishment()
    {
        var timeout = TestContext.Current.CancellationToken;
        var establishing = new TaskCompletionSource();
        var provider = new FakeClientProvider("fake", 1, establish: async (_, ct) =>
        {
            establishing.TrySetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return default;
        });
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), timeout);
        await establishing.Task.WaitAsync(timeout);

        // For example, the server's negotiation deadline expired.
        WriteControl(helper, AdditionalTransportControlMessageKind.GrpcSelected, 1, code: (int)AdditionalTransportNegotiationReason.Timeout);
        var client = await connectTask.WaitAsync(timeout);

        Assert.Equal(StreamingHubTransportState.GrpcFallback, client.GetTransportInfo().State);
        var candidate = Assert.Single(provider.Candidates);
        Assert.True(candidate.EstablishmentCanceled);
        Assert.True(candidate.IsDisposed);
        Assert.False(helper.HasPendingRequest);
    }

    [Fact]
    public async Task Selected_NegotiationFailed_Throws()
    {
        var timeout = TestContext.Current.CancellationToken;
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Required, provider), timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.Activate, (await ReadControlAsync(helper, timeout)).Kind);
        WriteControl(helper, AdditionalTransportControlMessageKind.NegotiationFailed, 1, code: (int)AdditionalTransportNegotiationReason.CandidatesExhausted);

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await connectTask.WaitAsync(timeout));
        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
    }

    [Fact]
    public async Task Selected_ActivatedForAnotherAttempt_IsProtocolError()
    {
        var timeout = TestContext.Current.CancellationToken;
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.Activate, (await ReadControlAsync(helper, timeout)).Kind);
        WriteControl(helper, AdditionalTransportControlMessageKind.Activated, 99);

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await connectTask.WaitAsync(timeout));
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        var candidate = Assert.Single(provider.Candidates);
        Assert.False(candidate.IsActivated);
        Assert.True(candidate.IsDisposed);
    }

    [Fact]
    public async Task Selected_SelectNextOfTriedCandidate_IsProtocolError()
    {
        var timeout = TestContext.Current.CancellationToken;
        var provider = new FakeClientProvider("fake", 1, establish: static (_, _) => throw new InvalidOperationException());
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.CandidateFailed, (await ReadControlAsync(helper, timeout)).Kind);
        WriteControl(helper, AdditionalTransportControlMessageKind.SelectNext, 2, code: 0);

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await connectTask.WaitAsync(timeout));
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
    }

    [Fact]
    public async Task Selected_AgreedMethodsNotDeclaredByClient_IsProtocolError()
    {
        var provider = new FakeClientProvider("fake", 1);
        var unknownMethod = StreamingHubTransportModeTable.Create([new StreamingHubTransportModeEntry(12345, StreamingHubTransportMode.UnreliableSequenced)]);
        var helper = CreateHelper(AdditionalTransportNegotiationResponse.Selected(1, 0, SelectionData, unknownMethod, StreamingHubTransportModeTable.Empty));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), TestContext.Current.CancellationToken));

        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
        Assert.False(helper.HasPendingRequest);
    }

    [Fact]
    public async Task Selected_ServerDoesNotAnswer_TimesOut()
    {
        var timeout = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(SelectFirst());
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithTimeProvider(timeProvider)
            .WithAdditionalTransport(new AdditionalTransportClientOptions([provider], AdditionalTransportEstablishment.Preferred, TimeSpan.FromSeconds(3)));

        var connectTask = helper.ConnectAsync(options, timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.Activate, (await ReadControlAsync(helper, timeout)).Kind);
        timeProvider.Advance(TimeSpan.FromSeconds(4));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await connectTask.WaitAsync(timeout));
        Assert.Equal(StatusCode.DeadlineExceeded, ex.StatusCode);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
    }

    [Fact]
    public async Task Selected_ConnectionClosed_Throws()
    {
        var timeout = TestContext.Current.CancellationToken;
        var provider = new FakeClientProvider("fake", 1);
        var helper = CreateHelper(SelectFirst());

        var connectTask = helper.ConnectAsync(CreateOptions(AdditionalTransportEstablishment.Preferred, provider), timeout);
        Assert.Equal(AdditionalTransportControlMessageKind.Activate, (await ReadControlAsync(helper, timeout)).Kind);
        helper.ThrowRpcException();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await connectTask.WaitAsync(timeout));
        Assert.Equal(StatusCode.Unavailable, ex.StatusCode);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
    }

    [Fact]
    public void Options_Validation()
    {
        Assert.Throws<ArgumentException>(() => new AdditionalTransportClientOptions([new FakeClientProvider("Invalid ID", 1)]));
        Assert.Throws<ArgumentException>(() => new AdditionalTransportClientOptions([new FakeClientProvider("fake", 1), new FakeClientProvider("fake", 1)]));
        Assert.Throws<ArgumentException>(() => new AdditionalTransportClientOptions(Enumerable.Range(0, 9).Select(x => new FakeClientProvider($"fake{x}", 1))));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AdditionalTransportClientOptions([new FakeClientProvider("fake", 1)], negotiationTimeout: TimeSpan.Zero));

        // The same identifier with another version is a different provider.
        var options = new AdditionalTransportClientOptions([new FakeClientProvider("fake", 1), new FakeClientProvider("fake", 2)]);
        Assert.Equal(2, options.Providers.Count);
        Assert.Equal(AdditionalTransportClientOptions.DefaultNegotiationTimeout, options.NegotiationTimeout);
    }

    [Fact]
    public void Options_WithMethods_PreserveAdditionalTransport()
    {
        var additionalTransport = new AdditionalTransportClientOptions([new FakeClientProvider("fake", 1)]);
        var options = StreamingHubClientOptions.CreateWithDefault().WithAdditionalTransport(additionalTransport);

        Assert.Same(additionalTransport, options.WithHost("example").AdditionalTransport);
        Assert.Same(additionalTransport, options.WithClientHeartbeatInterval(TimeSpan.FromSeconds(1)).AdditionalTransport);
        Assert.Same(additionalTransport, options.WithTimeProvider(TimeProvider.System).AdditionalTransport);
        Assert.Null(options.WithAdditionalTransport(null).AdditionalTransport);
    }

    sealed class FakeClientProvider(string id, int version, bool isApplicable = true, Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask<ReadOnlyMemory<byte>>>? establish = null) : IAdditionalTransportClientProvider
    {
        public string Id => id;
        public int Version => version;
        public List<FakeClientCandidate> Candidates { get; } = new();

        public bool TryCreateCandidate([NotNullWhen(true)] out IAdditionalTransportClientCandidate? candidate)
        {
            if (!isApplicable)
            {
                candidate = null;
                return false;
            }

            var fakeCandidate = new FakeClientCandidate(establish);
            Candidates.Add(fakeCandidate);
            candidate = fakeCandidate;
            return true;
        }
    }

    sealed class FakeClientCandidate(Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask<ReadOnlyMemory<byte>>>? establish) : IAdditionalTransportClientCandidate
    {
        public static readonly byte[] DefaultOffer = [1, 2, 3];

        public ReadOnlyMemory<byte> Offer => DefaultOffer;
        public byte[]? ReceivedSelection { get; private set; }
        public bool IsActivated { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool EstablishmentCanceled { get; private set; }

        public async ValueTask<ReadOnlyMemory<byte>> EstablishAsync(ReadOnlyMemory<byte> selection, CancellationToken cancellationToken)
        {
            ReceivedSelection = selection.ToArray();
            try
            {
                return establish is null ? ActivationData : await establish(selection, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                EstablishmentCanceled = true;
                throw;
            }
        }

        public void OnActivated() => IsActivated = true;

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return default;
        }
    }
}

public interface IUnreliableReceiverTestHubReceiver
{
    [Unreliable] void OnMove(int x);
    [UnreliableUnordered] void OnEmote(int id);
    void OnJoin(string name);
}

public interface IUnreliableReceiverOnlyTestHub : IStreamingHub<IUnreliableReceiverOnlyTestHub, IUnreliableReceiverTestHubReceiver>
{
    Task JoinAsync(string name);
}

public interface IUnreliableBothDirectionsTestHub : IStreamingHub<IUnreliableBothDirectionsTestHub, IUnreliableReceiverTestHubReceiver>
{
    [Unreliable] Task MoveAsync(int x);
    Task JoinAsync(string name);
}
