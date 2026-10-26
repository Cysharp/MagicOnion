namespace MagicOnion.Server.Features;

/// <summary>
/// Provides the additional-transport negotiation result of a StreamingHub connection.
/// </summary>
/// <remarks>
/// <para>
/// The feature is set on the connection's <c>HttpContext.Features</c> before <c>OnConnected</c> is called, only when the client offered
/// an additional transport. When the feature is absent from <c>OnConnected</c> onward, the connection uses gRPC only.
/// The feature is never available during <c>OnConnecting</c>, so its absence there does not mean that the connection uses gRPC only.
/// </para>
/// <para>This API is experimental and may change.</para>
/// </remarks>
public interface IStreamingHubTransportFeature
{
    /// <summary>
    /// Gets the finalized negotiation result of the connection.
    /// </summary>
    StreamingHubTransportInfo TransportInfo { get; }
}

internal sealed class StreamingHubTransportFeature(StreamingHubTransportInfo transportInfo) : IStreamingHubTransportFeature
{
    public StreamingHubTransportInfo TransportInfo => transportInfo;
}
