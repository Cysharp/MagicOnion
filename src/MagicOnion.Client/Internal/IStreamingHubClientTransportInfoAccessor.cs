namespace MagicOnion.Client.Internal;

/// <summary>
/// Exposes the additional-transport negotiation result of a connected StreamingHub client.
/// </summary>
/// <remarks>
/// The accessor is internal so that <see cref="IStreamingHubClient"/> and the hub contracts do not gain infrastructure members.
/// </remarks>
internal interface IStreamingHubClientTransportInfoAccessor
{
    StreamingHubTransportInfo TransportInfo { get; }
}
