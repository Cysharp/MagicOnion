using System.Diagnostics.CodeAnalysis;
using Grpc.Core;

namespace MagicOnion.Server;

/// <summary>
/// Provides an additional transport, such as UDP, that a StreamingHub server can accept from clients.
/// </summary>
/// <remarks>
/// A provider is registered once and shared by connections, so it must be thread-safe. The state of one connection belongs to the
/// candidate in the result of <see cref="TryAcceptAsync"/>. This API is experimental and may change.
/// </remarks>
public interface IAdditionalTransportServerProvider
{
    /// <summary>
    /// Gets the stable identifier of the provider, for example <c>magiconion.transport.udp</c>.
    /// The identifier consists of 1 to 64 lowercase ASCII letters, digits, '.', '-', or '_'.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the protocol version of the provider. An offer is passed to the provider only when both the identifier and the version match.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Tries to accept an offer from a client by creating a provisional candidate.
    /// </summary>
    /// <param name="offer">The provider-specific offer data. The memory is valid only during the call.</param>
    /// <param name="context">The connection that offered the candidate.</param>
    /// <param name="cancellationToken">A token that is canceled when the negotiation deadline expires or the connection is aborted.</param>
    /// <returns>
    /// <see cref="AdditionalTransportAcceptResult.Accepted"/> with the provisional candidate, or <see cref="AdditionalTransportAcceptResult.Rejected"/> to reject the offer.
    /// </returns>
    /// <remarks>
    /// Rejecting or throwing an exception makes the server try the next offer. The provisional candidate is disposed when it fails,
    /// when the negotiation falls back to gRPC, or when the connection ends.
    /// </remarks>
    ValueTask<AdditionalTransportAcceptResult> TryAcceptAsync(ReadOnlyMemory<byte> offer, AdditionalTransportServerContext context, CancellationToken cancellationToken);
}

/// <summary>
/// The state of an additional-transport candidate for one StreamingHub connection on the server.
/// </summary>
/// <remarks>This API is experimental and may change.</remarks>
public interface IAdditionalTransportServerCandidate : IAsyncDisposable
{
    /// <summary>
    /// Gets the provider-specific selection data returned to the client. The data must not exceed 2048 bytes.
    /// </summary>
    ReadOnlyMemory<byte> Selection { get; }

    /// <summary>
    /// Verifies the activation data sent by the client, for example the result of a path validation, and activates the candidate.
    /// </summary>
    /// <param name="activation">The provider-specific activation data. The memory is valid only during the call.</param>
    /// <param name="cancellationToken">A token that is canceled when the negotiation deadline expires or the connection is aborted.</param>
    /// <returns>
    /// <see langword="true"/> to activate the candidate as the additional transport of the connection;
    /// <see langword="false"/> to reject it, after which the server tries the next offer.
    /// </returns>
    ValueTask<bool> TryActivateAsync(ReadOnlyMemory<byte> activation, CancellationToken cancellationToken);
}

/// <summary>
/// The result of <see cref="IAdditionalTransportServerProvider.TryAcceptAsync"/>: the offer is accepted with a provisional candidate, or rejected.
/// </summary>
/// <remarks>This API is experimental and may change.</remarks>
public readonly struct AdditionalTransportAcceptResult
{
    /// <summary>
    /// Gets the provisional candidate when the offer is accepted; otherwise <see langword="null"/>.
    /// </summary>
    public IAdditionalTransportServerCandidate? Candidate { get; }

    /// <summary>
    /// Gets a value indicating whether the offer is accepted.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Candidate))]
    public bool IsAccepted => Candidate is not null;

    AdditionalTransportAcceptResult(IAdditionalTransportServerCandidate candidate)
    {
        Candidate = candidate;
    }

    /// <summary>
    /// Gets the result that rejects the offer. The server then tries the next offer.
    /// </summary>
    public static AdditionalTransportAcceptResult Rejected => default;

    /// <summary>
    /// Creates the result that accepts the offer with a provisional candidate.
    /// </summary>
    /// <param name="candidate">The provisional candidate of the connection.</param>
    public static AdditionalTransportAcceptResult Accepted(IAdditionalTransportServerCandidate candidate)
        => new(candidate ?? throw new ArgumentNullException(nameof(candidate)));
}

/// <summary>
/// Describes the StreamingHub connection that negotiates an additional transport.
/// </summary>
/// <remarks>This API is experimental and may change.</remarks>
public sealed class AdditionalTransportServerContext
{
    /// <summary>
    /// Gets the identifier of the StreamingHub connection.
    /// </summary>
    public Guid ConnectionId { get; }

    /// <summary>
    /// Gets the gRPC call of the StreamingHub connection.
    /// </summary>
    public ServerCallContext CallContext { get; }

    /// <summary>
    /// Gets the service provider of the StreamingHub connection.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    internal AdditionalTransportServerContext(Guid connectionId, ServerCallContext callContext, IServiceProvider serviceProvider)
    {
        ConnectionId = connectionId;
        CallContext = callContext;
        ServiceProvider = serviceProvider;
    }
}
