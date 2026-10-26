namespace MagicOnion.Client;

public static class StreamingHubClientExtensions
{
    /// <summary>
    /// Wait for the disconnection and return the reason.
    /// </summary>
    public static Task<DisconnectionReason> WaitForDisconnectAsync<TStreamingHub>(this TStreamingHub hub) where TStreamingHub : IStreamingHubMarker =>
        CastOrThrow(hub).WaitForDisconnectAsync();

    /// <summary>
    /// Gets the additional-transport negotiation result of the connection, fixed when the connection was established.
    /// </summary>
    /// <remarks>
    /// The result describes the routing capability of the connection, not whether the additional transport is currently reachable.
    /// This API is experimental and may change.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The object is not a StreamingHub client that provides the transport information, such as the object returned by <c>FireAndForget()</c>.</exception>
    public static StreamingHubTransportInfo GetTransportInfo<TStreamingHub>(this TStreamingHub hub) where TStreamingHub : IStreamingHubMarker
    {
        if (TryGetTransportInfo(hub, out var transportInfo))
        {
            return transportInfo;
        }

        throw new InvalidOperationException($"The StreamingHub client '{hub.GetType().FullName}' does not provide the transport information.");
    }

    /// <summary>
    /// Tries to get the additional-transport negotiation result of the connection, fixed when the connection was established.
    /// </summary>
    /// <param name="hub">The StreamingHub client.</param>
    /// <param name="transportInfo">The negotiation result when the client provides it; otherwise <see cref="StreamingHubTransportInfo.GrpcOnly"/>.</param>
    /// <returns>
    /// <see langword="true"/> when the client provides the transport information; <see langword="false"/> when it does not,
    /// such as the object returned by <c>FireAndForget()</c>.
    /// </returns>
    /// <remarks>
    /// The result describes the routing capability of the connection, not whether the additional transport is currently reachable.
    /// This API is experimental and may change.
    /// </remarks>
    public static bool TryGetTransportInfo<TStreamingHub>(this TStreamingHub hub, out StreamingHubTransportInfo transportInfo) where TStreamingHub : IStreamingHubMarker
    {
        if (hub is null)
        {
            throw new ArgumentNullException(nameof(hub));
        }
        if (hub is Internal.IStreamingHubClientTransportInfoAccessor accessor)
        {
            transportInfo = accessor.TransportInfo;
            return true;
        }

        transportInfo = StreamingHubTransportInfo.GrpcOnly;
        return false;
    }

    static IStreamingHubClient CastOrThrow<THub>(THub hub)
    {
        if (hub is null)
        {
            throw new ArgumentNullException(nameof(hub));
        }
        if (hub is IStreamingHubClient client)
        {
            return client;
        }
        else
        {
            throw new InvalidOperationException($"The StreamingHub client '{hub.GetType().FullName}' does not implement IStreamingHubClient interface.");
        }
    }
}
