namespace MagicOnion;

/// <summary>
/// Specifies the delivery semantics of a StreamingHub method or a receiver method.
/// </summary>
/// <remarks>
/// The mode describes logical delivery semantics shared by the client and the server. It does not select a physical route.
/// A method whose effective mode is unreliable may still travel over the gRPC connection when no additional transport is available.
/// </remarks>
public enum StreamingHubTransportMode
{
    /// <summary>
    /// Delivers messages in order with guaranteed arrival over the gRPC connection. This is the default when no attribute is specified.
    /// </summary>
    Reliable = 0,

    /// <summary>
    /// Delivers messages without an arrival guarantee. A message older than the newest accepted message on the same connection and direction is discarded.
    /// The receiver does not wait for missing messages.
    /// </summary>
    UnreliableSequenced = 1,

    /// <summary>
    /// Delivers messages without an arrival or ordering guarantee. Complete messages are passed to the application in the order they are received.
    /// </summary>
    UnreliableUnordered = 2,
}
