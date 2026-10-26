#nullable enable

using System.Buffers;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Server.Hubs.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace MagicOnion.Server.Tests;

/// <summary>
/// Verifies the server side of the additional-transport negotiation without a gRPC connection.
/// </summary>
public class AdditionalTransportServerNegotiationTest
{
    const int Move = 100;
    const int Emote = 200;
    const int OnPosition = 300;

    static StreamingHubTransportModeTable Table(params (int MethodId, StreamingHubTransportMode Mode)[] entries)
        => StreamingHubTransportModeTable.Create(entries.Select(x => new StreamingHubTransportModeEntry(x.MethodId, x.Mode)));

    static readonly StreamingHubTransportModeTable ServerHubMethods = Table((Move, StreamingHubTransportMode.UnreliableSequenced), (Emote, StreamingHubTransportMode.UnreliableUnordered));
    static readonly StreamingHubTransportModeTable ServerReceiverMethods = Table((OnPosition, StreamingHubTransportMode.UnreliableSequenced));

    static byte[] CreateRequest(AdditionalTransportEstablishment establishment, StreamingHubTransportModeTable? hubMethods = null, StreamingHubTransportModeTable? receiverMethods = null, params (string Id, int Version)[] offers)
        => AdditionalTransportNegotiation.EncodeRequest(new AdditionalTransportNegotiationRequest(
            establishment,
            hubMethods ?? ServerHubMethods,
            receiverMethods ?? ServerReceiverMethods,
            offers.Select(x => new AdditionalTransportOffer(x.Id, x.Version, new byte[] { 1 })).ToArray()));

    sealed class Harness
    {
        public FakeLogger Logger { get; } = new();
        public FakeTimeProvider TimeProvider { get; } = new();
        public List<(AdditionalTransportControlMessageKind Kind, int AttemptId, int Code, byte[] Data)> Sent { get; } = new();
        public CancellationTokenSource Connection { get; } = new();

        public ValueTask<AdditionalTransportServerNegotiation> StartAsync(byte[] request, params IAdditionalTransportServerProvider[] providers)
            => AdditionalTransportServerNegotiation.StartAsync(
                request,
                ServerHubMethods,
                () => ServerReceiverMethods,
                methodId => methodId == Move ? "MoveAsync" : null,
                methodId => methodId == OnPosition ? "OnPosition" : null,
                providers,
                new AdditionalTransportServerContext(Guid.NewGuid(), null!, null!),
                new AdditionalTransportServerOptions { NegotiationTimeout = TimeSpan.FromSeconds(5) },
                TimeProvider,
                payload => Sent.Add(Parse(payload)),
                Logger,
                "TestHub",
                Connection.Token);

        static (AdditionalTransportControlMessageKind, int, int, byte[]) Parse(StreamingHubPayload payload)
        {
            var reader = new StreamingHubClientMessageReader(payload.Memory);
            Assert.Equal(StreamingHubMessageType.AdditionalTransportControl, reader.ReadMessageType());
            var message = reader.ReadAdditionalTransportControl();
            return (message.Kind, message.AttemptId, message.Code, message.Data.ToArray());
        }
    }

    static AdditionalTransportNegotiationResponse DecodeResponse(AdditionalTransportServerNegotiation negotiation)
    {
        Assert.True(AdditionalTransportNegotiation.TryDecodeResponse(negotiation.ResponseBytes, out var response));
        return response!;
    }

    [Fact]
    public async Task NoAgreedMethods_Preferred_FallsBackWithoutAskingProviders()
    {
        var harness = new Harness();
        var provider = new FakeServerProvider("fake", 1);

        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, StreamingHubTransportModeTable.Empty, StreamingHubTransportModeTable.Empty, ("fake", 1)), provider);

        Assert.True(negotiation.IsCompleted);
        Assert.Equal(AdditionalTransportNegotiationStatus.Fallback, negotiation.ResponseStatus);
        Assert.Equal(AdditionalTransportNegotiationReason.NoAgreedMethods, DecodeResponse(negotiation).Reason);
        Assert.Equal(StreamingHubTransportState.GrpcFallback, negotiation.TransportInfo.State);
        Assert.Equal(0, provider.AcceptCount);
    }

    [Fact]
    public async Task NoAgreedMethods_Required_Fails()
    {
        var harness = new Harness();

        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Required, StreamingHubTransportModeTable.Empty, StreamingHubTransportModeTable.Empty, ("fake", 1)), new FakeServerProvider("fake", 1));

        Assert.Equal(AdditionalTransportNegotiationStatus.Failed, negotiation.ResponseStatus);
        Assert.Equal(AdditionalTransportNegotiationReason.NoAgreedMethods, DecodeResponse(negotiation).Reason);
    }

    [Fact]
    public async Task RollingUpgrade_ServerHasAdditionalMethods_AgreesOnClientMethods()
    {
        // The client still runs the previous contract: it knows Move but not Emote, and has no unreliable receiver method.
        var harness = new Harness();
        var clientHubMethods = Table((Move, StreamingHubTransportMode.UnreliableSequenced));

        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, clientHubMethods, StreamingHubTransportModeTable.Empty, ("fake", 1)), new FakeServerProvider("fake", 1));

        Assert.Equal(AdditionalTransportNegotiationStatus.Selected, negotiation.ResponseStatus);
        var response = DecodeResponse(negotiation);
        Assert.True(response.AgreedClientToServerMethods.ContentEquals(clientHubMethods));
        Assert.Equal(0, response.AgreedServerToClientMethods.Count);
        Assert.DoesNotContain(harness.Logger.Collector.GetSnapshot(), x => x.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task ModeConflict_ExcludesMethodAndLogsWarning()
    {
        var harness = new Harness();
        var clientHubMethods = Table((Move, StreamingHubTransportMode.UnreliableUnordered), (Emote, StreamingHubTransportMode.UnreliableUnordered));

        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, clientHubMethods, null, ("fake", 1)), new FakeServerProvider("fake", 1));

        var response = DecodeResponse(negotiation);
        Assert.True(response.AgreedClientToServerMethods.ContentEquals(Table((Emote, StreamingHubTransportMode.UnreliableUnordered))));
        Assert.True(response.AgreedServerToClientMethods.ContentEquals(ServerReceiverMethods));
        var warning = Assert.Single(harness.Logger.Collector.GetSnapshot(), x => x.Level == LogLevel.Warning);
        Assert.Contains("MoveAsync", warning.Message);
        Assert.Contains(nameof(StreamingHubTransportMode.UnreliableSequenced), warning.Message);
        Assert.Contains(nameof(StreamingHubTransportMode.UnreliableUnordered), warning.Message);
    }

    [Fact]
    public async Task NoMatchingProvider_FallsBack()
    {
        var harness = new Harness();
        var otherVersion = new FakeServerProvider("fake", 2);

        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("fake", 1), ("unknown", 1)]), otherVersion);

        Assert.Equal(AdditionalTransportNegotiationStatus.Fallback, negotiation.ResponseStatus);
        Assert.Equal(AdditionalTransportNegotiationReason.NoMatchingProvider, DecodeResponse(negotiation).Reason);
        Assert.Equal(0, otherVersion.AcceptCount);
    }

    [Fact]
    public async Task ProviderRejectsOrThrows_SelectsNextOffer()
    {
        var harness = new Harness();
        var rejecting = new FakeServerProvider("rejecting", 1, accept: static () => null);
        var throwing = new FakeServerProvider("throwing", 1, accept: static () => throw new InvalidOperationException("Failure"));
        var accepting = new FakeServerProvider("accepting", 1);

        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("rejecting", 1), ("throwing", 1), ("accepting", 1)]), rejecting, throwing, accepting);

        var response = DecodeResponse(negotiation);
        Assert.Equal(AdditionalTransportNegotiationStatus.Selected, response.Status);
        Assert.Equal(2, response.CandidateIndex);
        Assert.Equal(1, response.AttemptId);
        Assert.Equal(FakeServerCandidate.DefaultSelection, response.Selection.ToArray());
        Assert.Contains(harness.Logger.Collector.GetSnapshot(), x => x.Level == LogLevel.Error && x.Exception is InvalidOperationException);
    }

    [Fact]
    public async Task Activate_Accepted_FinalizesActive()
    {
        var harness = new Harness();
        var provider = new FakeServerProvider("fake", 3);
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("fake", 3)]), provider);

        await negotiation.ProcessClientMessageAsync(AdditionalTransportControlMessageKind.Activate, 1, 0, new byte[] { 9 });

        Assert.True(negotiation.IsCompleted);
        Assert.Equal(StreamingHubTransportState.ActiveAdditionalTransport, negotiation.TransportInfo.State);
        Assert.Equal("fake", negotiation.TransportInfo.AdditionalTransportProviderId);
        Assert.Equal(3, negotiation.TransportInfo.AdditionalTransportProviderVersion);
        var candidate = Assert.Single(provider.Candidates);
        Assert.Same(candidate, negotiation.ActiveCandidate);
        Assert.Equal(new byte[] { 9 }, candidate.ReceivedActivation);
        var sent = Assert.Single(harness.Sent);
        Assert.Equal(AdditionalTransportControlMessageKind.Activated, sent.Kind);
        Assert.Equal(1, sent.AttemptId);

        // Late messages after the result is finalized are ignored.
        await negotiation.ProcessClientMessageAsync(AdditionalTransportControlMessageKind.CandidateFailed, 1, 0, default);
        Assert.Single(harness.Sent);

        await negotiation.DisposeAsync();
        Assert.True(candidate.IsDisposed);
    }

    [Fact]
    public async Task ActivationRejected_SelectsNext_ThenExhausted_FallsBack()
    {
        var harness = new Harness();
        var rejecting = new FakeServerProvider("first", 1, activate: false);
        var second = new FakeServerProvider("second", 1);
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("first", 1), ("second", 1)]), rejecting, second);

        await negotiation.ProcessClientMessageAsync(AdditionalTransportControlMessageKind.Activate, 1, 0, default);

        Assert.False(negotiation.IsCompleted);
        Assert.True(Assert.Single(rejecting.Candidates).IsDisposed);
        var selectNext = Assert.Single(harness.Sent);
        Assert.Equal(AdditionalTransportControlMessageKind.SelectNext, selectNext.Kind);
        Assert.Equal(2, selectNext.AttemptId);
        Assert.Equal(1, selectNext.Code);
        Assert.Equal(FakeServerCandidate.DefaultSelection, selectNext.Data);

        await negotiation.ProcessClientMessageAsync(AdditionalTransportControlMessageKind.CandidateFailed, 2, (int)AdditionalTransportNegotiationReason.EstablishmentFailed, default);

        Assert.True(negotiation.IsCompleted);
        Assert.Equal(StreamingHubTransportState.GrpcFallback, negotiation.TransportInfo.State);
        Assert.True(Assert.Single(second.Candidates).IsDisposed);
        var grpcSelected = harness.Sent[1];
        Assert.Equal(AdditionalTransportControlMessageKind.GrpcSelected, grpcSelected.Kind);
        Assert.Equal((int)AdditionalTransportNegotiationReason.CandidatesExhausted, grpcSelected.Code);
    }

    [Fact]
    public async Task CandidatesExhausted_Required_SendsNegotiationFailedAndThrows()
    {
        var harness = new Harness();
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Required, offers: [("fake", 1)]), new FakeServerProvider("fake", 1));

        var ex = await Assert.ThrowsAsync<ReturnStatusException>(async () => await negotiation.ProcessClientMessageAsync(AdditionalTransportControlMessageKind.CandidateFailed, 1, 0, default));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Equal(AdditionalTransportControlMessageKind.NegotiationFailed, Assert.Single(harness.Sent).Kind);
    }

    [Theory]
    [InlineData((byte)AdditionalTransportControlMessageKind.Activate, 2)]
    [InlineData((byte)AdditionalTransportControlMessageKind.Activated, 1)]
    [InlineData((byte)0, 1)]
    [InlineData((byte)0x7d, 1)]
    public async Task InvalidControlMessage_IsProtocolError(byte kind, int attemptId)
    {
        var harness = new Harness();
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("fake", 1)]), new FakeServerProvider("fake", 1));

        var ex = await Assert.ThrowsAsync<ReturnStatusException>(async () => await negotiation.ProcessClientMessageAsync((AdditionalTransportControlMessageKind)kind, attemptId, 0, default));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task UnsupportedVersion_FollowsClientPolicy()
    {
        var harness = new Harness();
        var preferred = CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("fake", 1)]);
        preferred[0] = 99;
        var required = CreateRequest(AdditionalTransportEstablishment.Required, offers: [("fake", 1)]);
        required[0] = 99;

        await using var preferredNegotiation = await harness.StartAsync(preferred, new FakeServerProvider("fake", 1));
        await using var requiredNegotiation = await harness.StartAsync(required, new FakeServerProvider("fake", 1));

        Assert.Equal(AdditionalTransportNegotiationStatus.Fallback, preferredNegotiation.ResponseStatus);
        Assert.Equal(AdditionalTransportNegotiationReason.UnsupportedVersion, DecodeResponse(preferredNegotiation).Reason);
        Assert.Equal(AdditionalTransportNegotiationStatus.Failed, requiredNegotiation.ResponseStatus);
    }

    [Fact]
    public async Task MalformedRequest_IsConnectionError()
    {
        var harness = new Harness();

        var ex = await Assert.ThrowsAsync<ReturnStatusException>(async () => await harness.StartAsync(new byte[] { AdditionalTransportNegotiation.FormatVersion, 0, 0xff }, new FakeServerProvider("fake", 1)));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task Deadline_DuringControlPhase_Preferred_FallsBack()
    {
        var harness = new Harness();
        var provider = new FakeServerProvider("fake", 1);
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("fake", 1)]), provider);

        Assert.False(negotiation.Deadline.IsCompleted);
        harness.TimeProvider.Advance(TimeSpan.FromSeconds(6));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => negotiation.Deadline);
        await negotiation.OnDeadlineAsync();

        Assert.True(negotiation.IsCompleted);
        Assert.Equal(StreamingHubTransportState.GrpcFallback, negotiation.TransportInfo.State);
        Assert.True(Assert.Single(provider.Candidates).IsDisposed);
        var grpcSelected = Assert.Single(harness.Sent);
        Assert.Equal(AdditionalTransportControlMessageKind.GrpcSelected, grpcSelected.Kind);
        Assert.Equal((int)AdditionalTransportNegotiationReason.Timeout, grpcSelected.Code);
    }

    [Fact]
    public async Task Deadline_DuringControlPhase_Required_Fails()
    {
        var harness = new Harness();
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Required, offers: [("fake", 1)]), new FakeServerProvider("fake", 1));

        harness.TimeProvider.Advance(TimeSpan.FromSeconds(6));
        var ex = await Assert.ThrowsAsync<ReturnStatusException>(async () => await negotiation.OnDeadlineAsync());

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Equal(AdditionalTransportControlMessageKind.NegotiationFailed, Assert.Single(harness.Sent).Kind);
    }

    [Fact]
    public async Task Deadline_ConnectionAborted_Throws()
    {
        var harness = new Harness();
        await using var negotiation = await harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("fake", 1)]), new FakeServerProvider("fake", 1));

        harness.Connection.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => negotiation.Deadline);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await negotiation.OnDeadlineAsync());
        Assert.Empty(harness.Sent);
    }

    [Fact]
    public async Task Deadline_WhileProviderAccepts_FallsBack()
    {
        var harness = new Harness();
        var accepting = new TaskCompletionSource();
        var slow = new FakeServerProvider("slow", 1, acceptAsync: async ct =>
        {
            accepting.TrySetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return null;
        });

        var startTask = harness.StartAsync(CreateRequest(AdditionalTransportEstablishment.Preferred, offers: [("slow", 1)]), slow).AsTask();
        await accepting.Task.WaitAsync(TestContext.Current.CancellationToken);
        harness.TimeProvider.Advance(TimeSpan.FromSeconds(6));
        await using var negotiation = await startTask.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(AdditionalTransportNegotiationStatus.Fallback, negotiation.ResponseStatus);
        Assert.Equal(AdditionalTransportNegotiationReason.Timeout, DecodeResponse(negotiation).Reason);
    }

    [Fact]
    public void AcceptResult_RejectedHasNoCandidate_AcceptedRequiresCandidate()
    {
        Assert.False(AdditionalTransportAcceptResult.Rejected.IsAccepted);
        Assert.Null(AdditionalTransportAcceptResult.Rejected.Candidate);
        Assert.False(default(AdditionalTransportAcceptResult).IsAccepted);

        var candidate = new FakeServerCandidate(activate: true);
        var accepted = AdditionalTransportAcceptResult.Accepted(candidate);
        Assert.True(accepted.IsAccepted);
        Assert.Same(candidate, accepted.Candidate);

        Assert.Throws<ArgumentNullException>(() => AdditionalTransportAcceptResult.Accepted(null!));
    }

    sealed class FakeServerProvider(string id, int version, Func<FakeServerCandidate?>? accept = null, Func<CancellationToken, Task<FakeServerCandidate?>>? acceptAsync = null, bool activate = true) : IAdditionalTransportServerProvider
    {
        public string Id => id;
        public int Version => version;
        public int AcceptCount { get; private set; }
        public List<FakeServerCandidate> Candidates { get; } = new();

        public async ValueTask<AdditionalTransportAcceptResult> TryAcceptAsync(ReadOnlyMemory<byte> offer, AdditionalTransportServerContext context, CancellationToken cancellationToken)
        {
            AcceptCount++;
            var candidate = acceptAsync is not null
                ? await acceptAsync(cancellationToken)
                : accept is not null ? accept() : new FakeServerCandidate(activate);
            if (candidate is null) return AdditionalTransportAcceptResult.Rejected;

            Candidates.Add(candidate);
            return AdditionalTransportAcceptResult.Accepted(candidate);
        }
    }

    sealed class FakeServerCandidate(bool activate) : IAdditionalTransportServerCandidate
    {
        public static readonly byte[] DefaultSelection = [4, 2];

        public ReadOnlyMemory<byte> Selection => DefaultSelection;
        public byte[]? ReceivedActivation { get; private set; }
        public bool IsDisposed { get; private set; }

        public ValueTask<bool> TryActivateAsync(ReadOnlyMemory<byte> activation, CancellationToken cancellationToken)
        {
            ReceivedActivation = activation.ToArray();
            return new ValueTask<bool>(activate);
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return default;
        }
    }
}
