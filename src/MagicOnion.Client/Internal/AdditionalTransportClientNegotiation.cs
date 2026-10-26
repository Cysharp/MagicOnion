using System.Threading.Channels;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;

namespace MagicOnion.Client.Internal;

/// <summary>
/// Negotiates an additional transport for one StreamingHub connection on the client.
/// </summary>
/// <remarks>
/// <para>
/// The negotiation sends the client's unreliable method tables and its offers in the request metadata. The server returns the agreed
/// method tables and either a selected candidate or a final result in the response metadata. When a candidate is selected, the client
/// establishes it and the server decides the final state with control messages on the gRPC stream. The server is the authority:
/// the client only reports success (<c>Activate</c>) or failure (<c>CandidateFailed</c>) of each selected candidate.
/// </para>
/// <para>
/// Candidates are disposed as soon as they are no longer needed. The activated candidate is handed over with <see cref="TakeActiveCandidate"/>.
/// </para>
/// </remarks>
internal sealed class AdditionalTransportClientNegotiation : IAsyncDisposable
{
    readonly AdditionalTransportClientOptions options;
    readonly IMagicOnionClientLogger logger;
    readonly CandidateSlot[] candidates;
    readonly StreamingHubTransportModeTable localClientToServerMethods;
    readonly StreamingHubTransportModeTable localServerToClientMethods;
    readonly byte[] requestBytes;
    // The server sends at most one decision for each client message, so a few slots are enough. Overflow is a protocol violation.
    readonly Channel<ControlMessage> incoming = Channel.CreateBounded<ControlMessage>(new BoundedChannelOptions(4)
    {
        SingleReader = true,
        SingleWriter = true,
        FullMode = BoundedChannelFullMode.Wait,
        AllowSynchronousContinuations = false,
    });

    AdditionalTransportNegotiationResponse? response;
    IAdditionalTransportClientCandidate? activeCandidate;
    int completed;

    /// <summary>Gets the finalized result. Valid after the negotiation completed.</summary>
    public StreamingHubTransportInfo TransportInfo { get; private set; }

    /// <summary>Gets the unreliable client-to-server methods agreed by both peers.</summary>
    public StreamingHubTransportModeTable AgreedClientToServerMethods => response?.AgreedClientToServerMethods ?? StreamingHubTransportModeTable.Empty;

    /// <summary>Gets the unreliable server-to-client methods agreed by both peers.</summary>
    public StreamingHubTransportModeTable AgreedServerToClientMethods => response?.AgreedServerToClientMethods ?? StreamingHubTransportModeTable.Empty;

    AdditionalTransportClientNegotiation(AdditionalTransportClientOptions options, IMagicOnionClientLogger logger, CandidateSlot[] candidates, StreamingHubTransportModeTable localClientToServerMethods, StreamingHubTransportModeTable localServerToClientMethods, byte[] requestBytes)
    {
        this.options = options;
        this.logger = logger;
        this.candidates = candidates;
        this.localClientToServerMethods = localClientToServerMethods;
        this.localServerToClientMethods = localServerToClientMethods;
        this.requestBytes = requestBytes;
    }

    /// <summary>
    /// Prepares the negotiation of a connection.
    /// </summary>
    /// <returns>
    /// The negotiation, or <see langword="null"/> when the connection does not negotiate: the contracts have no unreliable method,
    /// or no provider is applicable and the policy is <see cref="AdditionalTransportEstablishment.Preferred"/>.
    /// </returns>
    /// <exception cref="RpcException">No provider is applicable and the policy is <see cref="AdditionalTransportEstablishment.Required"/>.</exception>
    /// <exception cref="InvalidOperationException">An offer or the request exceeds a negotiation limit.</exception>
    public static async ValueTask<AdditionalTransportClientNegotiation?> PrepareAsync(AdditionalTransportClientOptions options, StreamingHubTransportModeTable clientToServerMethods, StreamingHubTransportModeTable serverToClientMethods, IMagicOnionClientLogger logger)
    {
        // A contract without unreliable methods has nothing to route. It uses the existing flow under both policies.
        if (clientToServerMethods.Count == 0 && serverToClientMethods.Count == 0)
        {
            return null;
        }

        var slots = new List<CandidateSlot>(options.Providers.Count);
        try
        {
            foreach (var provider in options.Providers)
            {
                // A provider declines when it cannot be used in the current environment, such as raw UDP on Unity Web.
                if (provider.TryCreateCandidate(out var candidate))
                {
                    slots.Add(new CandidateSlot(provider, candidate));
                }
            }

            if (slots.Count == 0)
            {
                if (options.Establishment == AdditionalTransportEstablishment.Required)
                {
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "The StreamingHub connection requires an additional transport, but no configured additional transport provider is available in this environment."));
                }

                logger.Debug("No configured additional transport provider is available in this environment. The StreamingHub connection uses gRPC only.");
                return null;
            }

            var offers = slots.Select(static x => new AdditionalTransportOffer(x.Provider.Id, x.Provider.Version, x.Candidate!.Offer)).ToArray();
            var request = new AdditionalTransportNegotiationRequest(options.Establishment, clientToServerMethods, serverToClientMethods, offers);
            var requestBytes = AdditionalTransportNegotiation.EncodeRequest(request);

            return new AdditionalTransportClientNegotiation(options, logger, slots.ToArray(), clientToServerMethods, serverToClientMethods, requestBytes);
        }
        catch
        {
            foreach (var slot in slots)
            {
                await DisposeCandidateSafeAsync(slot, logger).ConfigureAwait(false);
            }
            throw;
        }
    }

    /// <summary>
    /// Returns call options whose headers additionally contain the negotiation request. The original headers are not modified.
    /// </summary>
    public CallOptions ApplyTo(CallOptions callOptions)
    {
        var headers = new Metadata();
        if (callOptions.Headers is { } originalHeaders)
        {
            foreach (var entry in originalHeaders)
            {
                if (!string.Equals(entry.Key, AdditionalTransportNegotiation.MetadataKey, StringComparison.OrdinalIgnoreCase))
                {
                    headers.Add(entry);
                }
            }
        }
        headers.Add(AdditionalTransportNegotiation.MetadataKey, requestBytes);

        return callOptions.WithHeaders(headers);
    }

    /// <summary>
    /// Applies the negotiation result in the response headers.
    /// </summary>
    /// <returns><see langword="true"/> when the server selected a candidate and the control phase follows; otherwise the negotiation is complete.</returns>
    /// <exception cref="RpcException">The connection cannot be established under the policy, or the response is invalid.</exception>
    public bool ProcessResponseHeaders(Metadata responseHeaders)
    {
        var entry = responseHeaders.GetOrDefault(AdditionalTransportNegotiation.MetadataKey);
        if (entry is null || !entry.IsBinary)
        {
            // The server did not negotiate, such as an older server. It does not understand control messages, so none are sent.
            return CompleteWithoutControlPhase(AdditionalTransportNegotiationReason.NotSupportedByServer);
        }

        if (!AdditionalTransportNegotiation.TryDecodeResponse(entry.ValueBytes, out var decoded))
        {
            throw CreateProtocolError("The server returned an invalid additional transport negotiation response.");
        }

        switch (decoded!.Status)
        {
            case AdditionalTransportNegotiationStatus.Fallback:
                return CompleteWithoutControlPhase(decoded.Reason);
            case AdditionalTransportNegotiationStatus.Failed:
                throw CreateNegotiationFailure(decoded.Reason);
            default:
                if (decoded.CandidateIndex >= candidates.Length)
                {
                    throw CreateProtocolError("The server selected an additional transport candidate that was not offered.");
                }
                // The agreed methods must be known to the client with the same mode.
                if (!localClientToServerMethods.IsSupersetOf(decoded.AgreedClientToServerMethods) || !localServerToClientMethods.IsSupersetOf(decoded.AgreedServerToClientMethods))
                {
                    throw CreateProtocolError("The server returned agreed unreliable methods that the client does not declare.");
                }
                response = decoded;
                return true;
        }
    }

    bool CompleteWithoutControlPhase(AdditionalTransportNegotiationReason reason)
    {
        if (options.Establishment == AdditionalTransportEstablishment.Required)
        {
            throw CreateNegotiationFailure(reason);
        }

        logger.Debug($"The StreamingHub connection falls back to gRPC. Reason: {reason}");
        TransportInfo = new StreamingHubTransportInfo(StreamingHubTransportState.GrpcFallback, null, 0);
        Interlocked.Exchange(ref completed, 1);
        return false;
    }

    /// <summary>
    /// Runs the control phase after the server selected a candidate.
    /// </summary>
    /// <param name="writer">The writer of the gRPC request stream.</param>
    /// <param name="timeProvider">The time provider for the negotiation deadline.</param>
    /// <param name="cancellationToken">A token to abandon the negotiation.</param>
    /// <returns>The finalized result.</returns>
    /// <exception cref="RpcException">The negotiation failed, timed out, violated the protocol, or the connection was closed.</exception>
    public async Task<StreamingHubTransportInfo> RunControlPhaseAsync(ChannelWriter<StreamingHubPayload> writer, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        var selected = response ?? throw new InvalidOperationException("The server has not selected a candidate.");

#if NET8_0_OR_GREATER
        using var timeoutCts = new CancellationTokenSource(options.NegotiationTimeout, timeProvider);
#else
        using var timeoutCts = timeProvider.CreateCancellationTokenSource(options.NegotiationTimeout);
#endif
        using var negotiationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var ct = negotiationCts.Token;

        try
        {
            var attemptId = selected.AttemptId;
            var index = selected.CandidateIndex;
            var selection = selected.Selection;
            candidates[index].Tried = true;

            while (true)
            {
                var slot = candidates[index];
                Task<ControlMessage> decisionTask;
                (bool Succeeded, ReadOnlyMemory<byte> Activation) established;

                using (var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    var establishTask = EstablishAsync(slot, selection, attemptCts.Token);
                    decisionTask = ReadControlMessageAsync(ct);

                    if (await Task.WhenAny(establishTask, decisionTask).ConfigureAwait(false) == decisionTask)
                    {
                        // The server decided while the candidate was being established, for example because its deadline expired.
                        attemptCts.Cancel();
                        try
                        {
                            await establishTask.ConfigureAwait(false);
                        }
                        catch
                        {
                            // The attempt is abandoned.
                        }
                        await DisposeCandidateAsync(slot).ConfigureAwait(false);
                        return CompleteWithServerDecision(await decisionTask.ConfigureAwait(false));
                    }

                    established = await establishTask.ConfigureAwait(false);
                }

                if (established.Succeeded)
                {
                    Send(writer, AdditionalTransportControlMessageKind.Activate, attemptId, 0, established.Activation.Span);
                }
                else
                {
                    Send(writer, AdditionalTransportControlMessageKind.CandidateFailed, attemptId, (int)AdditionalTransportNegotiationReason.EstablishmentFailed, default);
                    await DisposeCandidateAsync(slot).ConfigureAwait(false);
                }

                var decision = await decisionTask.ConfigureAwait(false);
                if (decision.Kind == AdditionalTransportControlMessageKind.Activated && established.Succeeded && decision.AttemptId == attemptId)
                {
                    var candidate = slot.Candidate!;
                    slot.Candidate = null;
                    activeCandidate = candidate;
                    candidate.OnActivated();

                    logger.Debug($"The StreamingHub connection activated the additional transport '{slot.Provider.Id}' version {slot.Provider.Version}.");
                    return Complete(new StreamingHubTransportInfo(StreamingHubTransportState.ActiveAdditionalTransport, slot.Provider.Id, slot.Provider.Version));
                }

                if (established.Succeeded)
                {
                    // The server rejected the activation.
                    await DisposeCandidateAsync(slot).ConfigureAwait(false);
                }

                if (decision.Kind == AdditionalTransportControlMessageKind.SelectNext)
                {
                    var next = decision.Code;
                    if (decision.AttemptId <= attemptId || next < 0 || next >= candidates.Length || candidates[next].Tried || candidates[next].Candidate is null || decision.Data.Length > AdditionalTransportNegotiation.MaxProviderDataLength)
                    {
                        throw CreateProtocolError("The server selected an invalid additional transport candidate.");
                    }

                    attemptId = decision.AttemptId;
                    index = next;
                    selection = decision.Data;
                    candidates[next].Tried = true;
                    continue;
                }

                return CompleteWithServerDecision(decision);
            }
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, $"The additional transport negotiation did not complete within {options.NegotiationTimeout}."));
        }
        finally
        {
            Interlocked.Exchange(ref completed, 1);
            foreach (var slot in candidates)
            {
                await DisposeCandidateAsync(slot).ConfigureAwait(false);
            }
        }
    }

    StreamingHubTransportInfo CompleteWithServerDecision(ControlMessage decision)
    {
        switch (decision.Kind)
        {
            case AdditionalTransportControlMessageKind.GrpcSelected:
                if (options.Establishment == AdditionalTransportEstablishment.Required)
                {
                    throw CreateProtocolError("The server selected gRPC although the client requires an additional transport.");
                }
                logger.Debug($"The StreamingHub connection falls back to gRPC. Reason: {(AdditionalTransportNegotiationReason)decision.Code}");
                return Complete(new StreamingHubTransportInfo(StreamingHubTransportState.GrpcFallback, null, 0));
            case AdditionalTransportControlMessageKind.NegotiationFailed:
                throw CreateNegotiationFailure((AdditionalTransportNegotiationReason)decision.Code);
            default:
                throw CreateProtocolError($"The server sent an unexpected additional transport control message '{decision.Kind}'.");
        }
    }

    StreamingHubTransportInfo Complete(StreamingHubTransportInfo transportInfo)
    {
        TransportInfo = transportInfo;
        Interlocked.Exchange(ref completed, 1);
        return transportInfo;
    }

    async Task<(bool Succeeded, ReadOnlyMemory<byte> Activation)> EstablishAsync(CandidateSlot slot, ReadOnlyMemory<byte> selection, CancellationToken cancellationToken)
    {
        try
        {
            var activation = await slot.Candidate!.EstablishAsync(selection, cancellationToken).ConfigureAwait(false);
            if (activation.Length > AdditionalTransportNegotiation.MaxProviderDataLength)
            {
                logger.Error(new InvalidOperationException($"The activation data exceeds the limit of {AdditionalTransportNegotiation.MaxProviderDataLength} bytes."), $"The additional transport provider '{slot.Provider.Id}' returned invalid activation data.");
                return (false, default);
            }
            return (true, activation);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.Error(ex, $"The additional transport provider '{slot.Provider.Id}' failed to establish the selected candidate.");
            return (false, default);
        }
    }

    async Task<ControlMessage> ReadControlMessageAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await incoming.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ChannelClosedException ex)
        {
            throw ex.InnerException as RpcException
                ?? new RpcException(new Status(StatusCode.Unavailable, "The StreamingHub connection was closed during the additional transport negotiation.", ex));
        }
    }

    static void Send(ChannelWriter<StreamingHubPayload> writer, AdditionalTransportControlMessageKind kind, int attemptId, int code, ReadOnlySpan<byte> data)
    {
        StreamingHubPayload payload;
        using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
        {
            StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromClient(buffer, kind, attemptId, code, data);
            payload = StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        if (!writer.TryWrite(payload))
        {
            throw new RpcException(new Status(StatusCode.Unavailable, "The StreamingHub connection was closed during the additional transport negotiation."));
        }
    }

    /// <summary>
    /// Receives a control message from the subscription loop. The data is copied before returning.
    /// </summary>
    public void OnControlMessage(AdditionalTransportControlMessageKind kind, int attemptId, int code, ReadOnlyMemory<byte> data)
    {
        // Late messages after the negotiation completed are ignored.
        if (Volatile.Read(ref completed) != 0) return;

        if (!incoming.Writer.TryWrite(new ControlMessage(kind, attemptId, code, data.ToArray())))
        {
            incoming.Writer.TryComplete(CreateProtocolError("The server sent too many additional transport control messages."));
        }
    }

    /// <summary>
    /// Notifies that the subscription loop ended.
    /// </summary>
    /// <param name="exception">The exception that ended the subscription loop, if any.</param>
    public void OnDisconnected(Exception? exception)
    {
        // The server ends the call with FailedPrecondition when a client that requires an additional transport cannot get one.
        // The call status reports the failure even if the NegotiationFailed message was not delivered before the call ended.
        incoming.Writer.TryComplete(exception is RpcException { StatusCode: StatusCode.FailedPrecondition } failed
            ? new RpcException(failed.Status)
            : new RpcException(new Status(StatusCode.Unavailable, "The StreamingHub connection was closed during the additional transport negotiation.", exception)));
    }

    /// <summary>
    /// Hands over the activated candidate. The caller becomes responsible for disposing it.
    /// </summary>
    public IAdditionalTransportClientCandidate? TakeActiveCandidate()
        => Interlocked.Exchange(ref activeCandidate, null);

    /// <summary>
    /// Disposes the candidates that were not handed over.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref completed, 1);
        incoming.Writer.TryComplete();

        foreach (var slot in candidates)
        {
            await DisposeCandidateAsync(slot).ConfigureAwait(false);
        }
        if (TakeActiveCandidate() is { } candidate)
        {
            await DisposeCandidateSafeAsync(candidate, logger, "the activated").ConfigureAwait(false);
        }
    }

    ValueTask DisposeCandidateAsync(CandidateSlot slot)
        => DisposeCandidateSafeAsync(slot, logger);

    static ValueTask DisposeCandidateSafeAsync(CandidateSlot slot, IMagicOnionClientLogger logger)
    {
        var candidate = slot.Candidate;
        slot.Candidate = null;
        return candidate is null ? default : DisposeCandidateSafeAsync(candidate, logger, $"the '{slot.Provider.Id}'");
    }

    static async ValueTask DisposeCandidateSafeAsync(IAdditionalTransportClientCandidate candidate, IMagicOnionClientLogger logger, string description)
    {
        try
        {
            await candidate.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"An error occurred while disposing {description} additional transport candidate.");
        }
    }

    static RpcException CreateNegotiationFailure(AdditionalTransportNegotiationReason reason)
        => new(new Status(StatusCode.FailedPrecondition, $"The StreamingHub connection requires an additional transport, but it could not be established. Reason: {reason}"));

    static RpcException CreateProtocolError(string message)
        => new(new Status(StatusCode.Internal, message));

    sealed class CandidateSlot
    {
        public IAdditionalTransportClientProvider Provider { get; }
        public IAdditionalTransportClientCandidate? Candidate { get; set; }
        public bool Tried { get; set; }

        public CandidateSlot(IAdditionalTransportClientProvider provider, IAdditionalTransportClientCandidate candidate)
        {
            Provider = provider;
            Candidate = candidate;
        }
    }

    readonly struct ControlMessage
    {
        public AdditionalTransportControlMessageKind Kind { get; }
        public int AttemptId { get; }
        public int Code { get; }
        public ReadOnlyMemory<byte> Data { get; }

        public ControlMessage(AdditionalTransportControlMessageKind kind, int attemptId, int code, ReadOnlyMemory<byte> data)
        {
            Kind = kind;
            AttemptId = attemptId;
            Code = code;
            Data = data;
        }
    }
}
