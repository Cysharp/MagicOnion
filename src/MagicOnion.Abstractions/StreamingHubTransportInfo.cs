namespace MagicOnion;

/// <summary>
/// Describes how a StreamingHub connection routes the methods whose transport mode is unreliable.
/// </summary>
/// <remarks>
/// The gRPC connection is always present. Reliable methods, client results, and heartbeats use it in every state.
/// This API is experimental and may change.
/// </remarks>
public enum StreamingHubTransportState
{
    /// <summary>
    /// No additional transport was negotiated, because none was offered or the contracts have no unreliable method.
    /// All messages use the gRPC connection.
    /// </summary>
    GrpcOnly = 0,

    /// <summary>
    /// An additional transport is active. Unreliable methods agreed by both peers can use it; all other messages use the gRPC connection.
    /// </summary>
    ActiveAdditionalTransport = 1,

    /// <summary>
    /// An additional transport was offered but not established. All messages use the gRPC connection.
    /// </summary>
    GrpcFallback = 2,
}

/// <summary>
/// Specifies whether a StreamingHub connection requires an additional transport for its unreliable methods.
/// </summary>
/// <remarks>This API is experimental and may change.</remarks>
public enum AdditionalTransportEstablishment
{
    /// <summary>
    /// Uses an additional transport when one can be established; otherwise the connection uses gRPC for every message.
    /// </summary>
    Preferred = 0,

    /// <summary>
    /// Fails the connection when no additional transport can be established for a contract with unreliable methods.
    /// </summary>
    Required = 1,
}

/// <summary>
/// The additional-transport negotiation result of a StreamingHub connection, fixed when the connection is established.
/// </summary>
/// <remarks>
/// The result describes the routing capability of the connection. It does not guarantee that the additional transport is currently reachable.
/// This API is experimental and may change.
/// </remarks>
public readonly struct StreamingHubTransportInfo
{
    /// <summary>
    /// Gets the result of a connection that negotiated no additional transport.
    /// </summary>
    public static StreamingHubTransportInfo GrpcOnly => default;

    /// <summary>
    /// Gets the routing state of the connection.
    /// </summary>
    public StreamingHubTransportState State { get; }

    /// <summary>
    /// Gets the identifier of the active additional-transport provider, or <see langword="null"/> when no additional transport is active.
    /// </summary>
    public string? AdditionalTransportProviderId { get; }

    /// <summary>
    /// Gets the protocol version of the active additional-transport provider, or 0 when no additional transport is active.
    /// </summary>
    public int AdditionalTransportProviderVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamingHubTransportInfo"/> struct.
    /// </summary>
    /// <param name="state">The routing state of the connection.</param>
    /// <param name="additionalTransportProviderId">The identifier of the active provider. Required when <paramref name="state"/> is <see cref="StreamingHubTransportState.ActiveAdditionalTransport"/>; otherwise must be <see langword="null"/>.</param>
    /// <param name="additionalTransportProviderVersion">The protocol version of the active provider, or 0 when no additional transport is active.</param>
    /// <exception cref="ArgumentException">The provider identifier does not match the state.</exception>
    public StreamingHubTransportInfo(StreamingHubTransportState state, string? additionalTransportProviderId, int additionalTransportProviderVersion)
    {
        if (state == StreamingHubTransportState.ActiveAdditionalTransport)
        {
            if (string.IsNullOrEmpty(additionalTransportProviderId)) throw new ArgumentException("An active additional transport requires a provider identifier.", nameof(additionalTransportProviderId));
        }
        else if (additionalTransportProviderId is not null || additionalTransportProviderVersion != 0)
        {
            throw new ArgumentException("A provider identifier and version can be specified only for an active additional transport.", nameof(additionalTransportProviderId));
        }

        State = state;
        AdditionalTransportProviderId = additionalTransportProviderId;
        AdditionalTransportProviderVersion = additionalTransportProviderVersion;
    }

    /// <inheritdoc />
    public override string ToString()
        => State == StreamingHubTransportState.ActiveAdditionalTransport
            ? $"{State} ({AdditionalTransportProviderId}/{AdditionalTransportProviderVersion})"
            : State.ToString();
}
