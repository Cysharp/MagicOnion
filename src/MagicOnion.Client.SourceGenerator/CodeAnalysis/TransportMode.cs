namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

/// <summary>
/// The delivery mode of a StreamingHub or receiver method as seen by the source generator.
/// The values mirror <c>MagicOnion.StreamingHubTransportMode</c> in MagicOnion.Abstractions.
/// </summary>
public enum TransportMode
{
    /// <summary>Delivered in order with guaranteed arrival over the gRPC connection.</summary>
    Reliable = 0,

    /// <summary>Delivered without an arrival guarantee; messages older than the newest accepted one are discarded.</summary>
    UnreliableSequenced = 1,

    /// <summary>Delivered without an arrival or ordering guarantee.</summary>
    UnreliableUnordered = 2,
}
