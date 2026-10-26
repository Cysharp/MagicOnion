using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MagicOnion.Server.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Hubs.Internal;

/// <summary>
/// Negotiates an additional transport for one StreamingHub connection on the server.
/// </summary>
/// <remarks>
/// <para>
/// The server is the authority of the negotiation. It computes the agreed unreliable methods, selects candidates in the client's order
/// of preference, and finalizes the connection as ActiveAdditionalTransport, GrpcFallback, or a failure. Once the result is finalized,
/// later control messages are ignored and the result never changes.
/// </para>
/// <para>
/// The negotiation deadline covers both the selection before the response headers and the control phase that follows.
/// Provisional candidates are disposed as soon as they fail. The activated candidate lives until <see cref="DisposeAsync"/>.
/// </para>
/// </remarks>
internal sealed class AdditionalTransportServerNegotiation : IAsyncDisposable
{
    readonly AdditionalTransportNegotiationRequest? request;
    readonly AdditionalTransportEstablishment establishment;
    readonly IReadOnlyList<IAdditionalTransportServerProvider> providers;
    readonly AdditionalTransportServerContext context;
    readonly Action<StreamingHubPayload> sendControlMessage;
    readonly ILogger logger;
    readonly string hubName;
    readonly CancellationToken connectionToken;
    readonly CancellationTokenSource timeoutCts;
    readonly CancellationTokenSource negotiationCts;

    int nextOfferIndex;
    int attemptId;
    Selected? current;
    bool deadlineEnded;

    /// <summary>Gets the negotiation status returned in the response headers.</summary>
    public AdditionalTransportNegotiationStatus ResponseStatus { get; private set; }

    /// <summary>Gets the encoded response returned in the response headers.</summary>
    public byte[] ResponseBytes { get; private set; } = Array.Empty<byte>();

    /// <summary>Gets a value indicating whether the result is finalized.</summary>
    public bool IsCompleted { get; private set; }

    /// <summary>Gets the reason of a fallback or a failure.</summary>
    public AdditionalTransportNegotiationReason Reason { get; private set; }

    /// <summary>Gets the finalized result. Valid after <see cref="IsCompleted"/> becomes <see langword="true"/>.</summary>
    public StreamingHubTransportInfo TransportInfo { get; private set; }

    /// <summary>Gets the unreliable client-to-server methods agreed by both peers.</summary>
    public StreamingHubTransportModeTable AgreedClientToServerMethods { get; private set; } = StreamingHubTransportModeTable.Empty;

    /// <summary>Gets the unreliable server-to-client methods agreed by both peers.</summary>
    public StreamingHubTransportModeTable AgreedServerToClientMethods { get; private set; } = StreamingHubTransportModeTable.Empty;

    /// <summary>Gets the activated candidate, or <see langword="null"/> when no additional transport is active.</summary>
    public IAdditionalTransportServerCandidate? ActiveCandidate { get; private set; }

    /// <summary>Gets a task that completes, as canceled, when the deadline expires or the connection is aborted.</summary>
    public Task Deadline { get; }

    AdditionalTransportServerNegotiation(AdditionalTransportNegotiationRequest? request, AdditionalTransportEstablishment establishment, IReadOnlyList<IAdditionalTransportServerProvider> providers, AdditionalTransportServerContext context, AdditionalTransportServerOptions options, TimeProvider timeProvider, Action<StreamingHubPayload> sendControlMessage, ILogger logger, string hubName, CancellationToken connectionToken)
    {
        this.request = request;
        this.establishment = establishment;
        this.providers = providers;
        this.context = context;
        this.sendControlMessage = sendControlMessage;
        this.logger = logger;
        this.hubName = hubName;
        this.connectionToken = connectionToken;

        timeoutCts = new CancellationTokenSource(options.NegotiationTimeout, timeProvider);
        negotiationCts = CancellationTokenSource.CreateLinkedTokenSource(connectionToken, timeoutCts.Token);
        Deadline = Task.Delay(Timeout.InfiniteTimeSpan, negotiationCts.Token);
    }

    bool IsTimedOut => timeoutCts.IsCancellationRequested && !connectionToken.IsCancellationRequested;

    /// <summary>
    /// Starts the negotiation for a request received in the request metadata, and selects the first candidate.
    /// </summary>
    /// <exception cref="ReturnStatusException">The request is malformed.</exception>
    public static async ValueTask<AdditionalTransportServerNegotiation> StartAsync(
        ReadOnlyMemory<byte> requestData,
        StreamingHubTransportModeTable localClientToServerMethods,
        Func<StreamingHubTransportModeTable> getLocalServerToClientMethods,
        Func<int, string?> getClientToServerMethodName,
        Func<int, string?> getServerToClientMethodName,
        IReadOnlyList<IAdditionalTransportServerProvider> providers,
        AdditionalTransportServerContext context,
        AdditionalTransportServerOptions options,
        TimeProvider timeProvider,
        Action<StreamingHubPayload> sendControlMessage,
        ILogger logger,
        string hubName,
        CancellationToken connectionToken)
    {
        var decodeResult = AdditionalTransportNegotiation.DecodeRequest(requestData, out var request, out var establishment);
        if (decodeResult == AdditionalTransportRequestDecodeResult.Malformed)
        {
            // A broken negotiation request is a connection error, not an unavailable candidate.
            throw new ReturnStatusException(StatusCode.InvalidArgument, "The additional transport negotiation request is invalid.");
        }

        var negotiation = new AdditionalTransportServerNegotiation(request, establishment, providers, context, options, timeProvider, sendControlMessage, logger, hubName, connectionToken);
        try
        {
            await negotiation.SelectInitialCandidateAsync(localClientToServerMethods, getLocalServerToClientMethods, getClientToServerMethodName, getServerToClientMethodName).ConfigureAwait(false);
            return negotiation;
        }
        catch
        {
            await negotiation.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    async ValueTask SelectInitialCandidateAsync(StreamingHubTransportModeTable localClientToServerMethods, Func<StreamingHubTransportModeTable> getLocalServerToClientMethods, Func<int, string?> getClientToServerMethodName, Func<int, string?> getServerToClientMethodName)
    {
        if (request is null)
        {
            CompleteBeforeControlPhase(AdditionalTransportNegotiationReason.UnsupportedVersion);
            return;
        }

        // Only methods known to both peers with the same mode use the additional transport. Methods known to one peer only
        // behave as they do over gRPC; methods whose modes differ fall back to gRPC on this connection.
        AgreedClientToServerMethods = StreamingHubTransportModeTable.Agree(localClientToServerMethods, request.ClientToServerMethods, out var clientToServerConflicts);
        AgreedServerToClientMethods = StreamingHubTransportModeTable.Agree(getLocalServerToClientMethods(), request.ServerToClientMethods, out var serverToClientConflicts);
        foreach (var conflict in clientToServerConflicts)
        {
            MagicOnionServerLog.AdditionalTransportModeConflict(logger, hubName, "hub", conflict.MethodId, getClientToServerMethodName(conflict.MethodId), conflict.LocalMode, conflict.PeerMode);
        }
        foreach (var conflict in serverToClientConflicts)
        {
            MagicOnionServerLog.AdditionalTransportModeConflict(logger, hubName, "receiver", conflict.MethodId, getServerToClientMethodName(conflict.MethodId), conflict.LocalMode, conflict.PeerMode);
        }

        if (AgreedClientToServerMethods.Count == 0 && AgreedServerToClientMethods.Count == 0)
        {
            CompleteBeforeControlPhase(AdditionalTransportNegotiationReason.NoAgreedMethods);
            return;
        }

        try
        {
            if (await TrySelectNextAsync().ConfigureAwait(false))
            {
                ResponseStatus = AdditionalTransportNegotiationStatus.Selected;
                ResponseBytes = AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Selected(
                    attemptId, current!.OfferIndex, current.Candidate.Selection, AgreedClientToServerMethods, AgreedServerToClientMethods));
                return;
            }
            CompleteBeforeControlPhase(AdditionalTransportNegotiationReason.NoMatchingProvider);
        }
        catch (OperationCanceledException) when (IsTimedOut)
        {
            await DisposeCurrentAsync().ConfigureAwait(false);
            CompleteBeforeControlPhase(AdditionalTransportNegotiationReason.Timeout);
        }
    }

    void CompleteBeforeControlPhase(AdditionalTransportNegotiationReason reason)
    {
        Reason = reason;
        IsCompleted = true;
        if (establishment == AdditionalTransportEstablishment.Required)
        {
            ResponseStatus = AdditionalTransportNegotiationStatus.Failed;
            ResponseBytes = AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Failed(reason));
            MagicOnionServerLog.AdditionalTransportNegotiationFailed(logger, hubName, reason.ToString());
        }
        else
        {
            ResponseStatus = AdditionalTransportNegotiationStatus.Fallback;
            ResponseBytes = AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Fallback(reason));
            TransportInfo = new StreamingHubTransportInfo(StreamingHubTransportState.GrpcFallback, null, 0);
            MagicOnionServerLog.AdditionalTransportNegotiated(logger, hubName, TransportInfo.State, null, reason.ToString());
        }
    }

    /// <summary>
    /// Processes a control message received during the control phase.
    /// </summary>
    /// <param name="kind">The kind of the control message.</param>
    /// <param name="messageAttemptId">The attempt identifier in the message.</param>
    /// <param name="code">The code in the message.</param>
    /// <param name="data">The data in the message. The memory is valid only during the call.</param>
    /// <exception cref="ReturnStatusException">The message violates the protocol, or the negotiation failed for a client that requires an additional transport.</exception>
    public async ValueTask ProcessClientMessageAsync(AdditionalTransportControlMessageKind kind, int messageAttemptId, int code, ReadOnlyMemory<byte> data)
    {
        if (IsCompleted) return;

        // The server answers each client message before the client sends the next one, so a message must refer to the current attempt.
        if (kind is not (AdditionalTransportControlMessageKind.Activate or AdditionalTransportControlMessageKind.CandidateFailed) || current is null || messageAttemptId != attemptId)
        {
            throw new ReturnStatusException(StatusCode.InvalidArgument, "The additional transport control message is invalid.");
        }

        try
        {
            if (kind == AdditionalTransportControlMessageKind.Activate && await TryActivateCurrentAsync(data).ConfigureAwait(false))
            {
                CompleteActive();
                return;
            }

            // The client could not establish the candidate, or the provider rejected the activation.
            await FailCurrentAndSelectNextAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (IsTimedOut)
        {
            await CompleteByTimeoutAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Finalizes the negotiation after <see cref="Deadline"/> completed.
    /// </summary>
    /// <exception cref="OperationCanceledException">The connection was aborted.</exception>
    /// <exception cref="ReturnStatusException">The client requires an additional transport.</exception>
    public async ValueTask OnDeadlineAsync()
    {
        connectionToken.ThrowIfCancellationRequested();
        if (IsCompleted) return;
        await CompleteByTimeoutAsync().ConfigureAwait(false);
    }

    async ValueTask<bool> TryActivateCurrentAsync(ReadOnlyMemory<byte> activation)
    {
        var selected = current!;
        try
        {
            return await selected.Candidate.TryActivateAsync(activation, negotiationCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (negotiationCts.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            MagicOnionServerLog.AdditionalTransportProviderError(logger, ex, hubName, selected.Provider.Id);
            return false;
        }
    }

    void CompleteActive()
    {
        var selected = current!;
        current = null;

        // Publish the result before sending the acknowledgement, which the response queue delivers before any message sent from OnConnected.
        ActiveCandidate = selected.Candidate;
        TransportInfo = new StreamingHubTransportInfo(StreamingHubTransportState.ActiveAdditionalTransport, selected.Provider.Id, selected.Provider.Version);
        IsCompleted = true;
        Send(AdditionalTransportControlMessageKind.Activated, attemptId, 0, default);

        MagicOnionServerLog.AdditionalTransportNegotiated(logger, hubName, TransportInfo.State, selected.Provider.Id, AdditionalTransportNegotiationReason.None.ToString());
    }

    async ValueTask FailCurrentAndSelectNextAsync()
    {
        await DisposeCurrentAsync().ConfigureAwait(false);

        if (await TrySelectNextAsync().ConfigureAwait(false))
        {
            Send(AdditionalTransportControlMessageKind.SelectNext, attemptId, current!.OfferIndex, current.Candidate.Selection.Span);
            return;
        }

        CompleteWithoutAdditionalTransport(AdditionalTransportNegotiationReason.CandidatesExhausted);
    }

    async ValueTask CompleteByTimeoutAsync()
    {
        await DisposeCurrentAsync().ConfigureAwait(false);
        CompleteWithoutAdditionalTransport(AdditionalTransportNegotiationReason.Timeout);
    }

    void CompleteWithoutAdditionalTransport(AdditionalTransportNegotiationReason reason)
    {
        Reason = reason;
        IsCompleted = true;

        if (establishment == AdditionalTransportEstablishment.Required)
        {
            Send(AdditionalTransportControlMessageKind.NegotiationFailed, attemptId, (int)reason, default);
            MagicOnionServerLog.AdditionalTransportNegotiationFailed(logger, hubName, reason.ToString());
            throw new ReturnStatusException(StatusCode.FailedPrecondition, $"The client requires an additional transport, but it could not be established. Reason: {reason}");
        }

        TransportInfo = new StreamingHubTransportInfo(StreamingHubTransportState.GrpcFallback, null, 0);
        Send(AdditionalTransportControlMessageKind.GrpcSelected, attemptId, (int)reason, default);
        MagicOnionServerLog.AdditionalTransportNegotiated(logger, hubName, TransportInfo.State, null, reason.ToString());
    }

    async ValueTask<bool> TrySelectNextAsync()
    {
        var offers = request!.Offers;
        while (nextOfferIndex < offers.Count)
        {
            var offerIndex = nextOfferIndex++;
            var offer = offers[offerIndex];
            var provider = FindProvider(offer.ProviderId, offer.ProviderVersion);
            if (provider is null) continue;

            AdditionalTransportAcceptResult acceptResult;
            try
            {
                acceptResult = await provider.TryAcceptAsync(offer.Data, context, negotiationCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (negotiationCts.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                MagicOnionServerLog.AdditionalTransportProviderError(logger, ex, hubName, provider.Id);
                continue;
            }

            if (!acceptResult.IsAccepted) continue;

            var candidate = acceptResult.Candidate;
            if (candidate.Selection.Length > AdditionalTransportNegotiation.MaxProviderDataLength)
            {
                MagicOnionServerLog.AdditionalTransportProviderError(logger, new InvalidOperationException($"The selection data exceeds the limit of {AdditionalTransportNegotiation.MaxProviderDataLength} bytes."), hubName, provider.Id);
                await DisposeSafeAsync(provider, candidate).ConfigureAwait(false);
                continue;
            }

            attemptId++;
            current = new Selected(provider, candidate, offerIndex);
            return true;
        }

        return false;
    }

    IAdditionalTransportServerProvider? FindProvider(string providerId, int providerVersion)
    {
        foreach (var provider in providers)
        {
            if (provider.Version == providerVersion && string.Equals(provider.Id, providerId, StringComparison.Ordinal))
            {
                return provider;
            }
        }
        return null;
    }

    void Send(AdditionalTransportControlMessageKind kind, int messageAttemptId, int code, ReadOnlySpan<byte> data)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromServer(buffer, kind, messageAttemptId, code, data);
        sendControlMessage(StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan));
    }

    /// <summary>
    /// Stops the negotiation deadline. Called when the control phase ended.
    /// </summary>
    public void EndDeadline()
    {
        if (deadlineEnded) return;
        deadlineEnded = true;

        // Cancel before disposing so that the pending deadline task completes and releases its registration.
        negotiationCts.Cancel();
        negotiationCts.Dispose();
        timeoutCts.Dispose();
    }

    async ValueTask DisposeCurrentAsync()
    {
        if (current is { } selected)
        {
            current = null;
            await DisposeSafeAsync(selected.Provider, selected.Candidate).ConfigureAwait(false);
        }
    }

    async ValueTask DisposeSafeAsync(IAdditionalTransportServerProvider provider, IAdditionalTransportServerCandidate candidate)
    {
        try
        {
            await candidate.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            MagicOnionServerLog.AdditionalTransportProviderError(logger, ex, hubName, provider.Id);
        }
    }

    /// <summary>
    /// Releases the deadline, the provisional candidate, and the activated candidate.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        EndDeadline();
        await DisposeCurrentAsync().ConfigureAwait(false);

        if (ActiveCandidate is { } active)
        {
            ActiveCandidate = null;
            try
            {
                await active.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MagicOnionServerLog.AdditionalTransportProviderError(logger, ex, hubName, TransportInfo.AdditionalTransportProviderId ?? string.Empty);
            }
        }
    }

    sealed class Selected(IAdditionalTransportServerProvider provider, IAdditionalTransportServerCandidate candidate, int offerIndex)
    {
        public IAdditionalTransportServerProvider Provider { get; } = provider;
        public IAdditionalTransportServerCandidate Candidate { get; } = candidate;
        public int OfferIndex { get; } = offerIndex;
    }
}
