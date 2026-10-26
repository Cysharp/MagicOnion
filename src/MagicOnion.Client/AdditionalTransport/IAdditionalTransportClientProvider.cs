using System.Diagnostics.CodeAnalysis;

namespace MagicOnion.Client;

/// <summary>
/// Provides an additional transport, such as UDP, that a StreamingHub client can offer to the server.
/// </summary>
/// <remarks>
/// A provider is configured once and shared by connections, so it must be thread-safe. The state of one connection belongs to the
/// candidate created by <see cref="TryCreateCandidate"/>. This API is experimental and may change.
/// </remarks>
public interface IAdditionalTransportClientProvider
{
    /// <summary>
    /// Gets the stable identifier of the provider, for example <c>magiconion.transport.udp</c>.
    /// The identifier consists of 1 to 64 lowercase ASCII letters, digits, '.', '-', or '_'.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the protocol version of the provider. The server selects a candidate only when both the identifier and the version match.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Tries to create the state of one connection.
    /// </summary>
    /// <param name="candidate">When this method returns <see langword="true"/>, contains a new candidate.</param>
    /// <returns><see langword="true"/> if a candidate was created; <see langword="false"/> when the provider cannot be used in the current environment.</returns>
    bool TryCreateCandidate([NotNullWhen(true)] out IAdditionalTransportClientCandidate? candidate);
}

/// <summary>
/// The state of an additional-transport candidate for one StreamingHub connection on the client.
/// </summary>
/// <remarks>
/// The client disposes the candidate as soon as it is no longer needed: when it fails, when the server selects another candidate,
/// when the connection falls back to gRPC, or when the connection ends after the candidate was activated.
/// This API is experimental and may change.
/// </remarks>
public interface IAdditionalTransportClientCandidate : IAsyncDisposable
{
    /// <summary>
    /// Gets the provider-specific offer sent to the server in the request metadata. The data must not exceed 2048 bytes.
    /// </summary>
    ReadOnlyMemory<byte> Offer { get; }

    /// <summary>
    /// Establishes the provider-specific connection using the selection returned by the server.
    /// </summary>
    /// <param name="selection">The provider-specific selection data. The memory is valid only during the call.</param>
    /// <param name="cancellationToken">A token that is canceled when the negotiation is abandoned or times out.</param>
    /// <returns>The activation data sent to the server. The data must not exceed 2048 bytes.</returns>
    /// <remarks>
    /// Throwing an exception reports the candidate as failed. The server then selects another candidate, falls back to gRPC,
    /// or fails the connection according to the establishment policy.
    /// </remarks>
    ValueTask<ReadOnlyMemory<byte>> EstablishAsync(ReadOnlyMemory<byte> selection, CancellationToken cancellationToken);

    /// <summary>
    /// Called when the server confirmed the activation. The candidate becomes the additional transport of the connection.
    /// </summary>
    void OnActivated();
}
