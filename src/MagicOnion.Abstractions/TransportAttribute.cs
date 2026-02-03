namespace MagicOnion;

/// <summary>
/// Specifies the transport for method calls of StreamingHub.
/// </summary>
/// <remarks>
/// Methods or classes of StreamingHub marked with this attribute can use transports different from the default. However, whether the specified reliability is supported depends on the transport.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TransportAttribute(TransportReliability reliability) : Attribute
{
    /// <summary>
    /// Gets or sets the reliability level of the transport mechanism, determining the guarantees provided for message
    /// delivery.
    /// </summary>
    public TransportReliability Reliability { get; set; } = reliability;
}

public enum TransportReliability
{
    /// <summary>
    /// The method may be invoked over reliable transports such as gRPC.
    /// </summary>
    Reliable,
    /// <summary>
    /// The method may be invoked over reliable transports such as RUDP. However, the order of calls may be unordered.
    /// </summary>
    ReliableUnordered,
    /// <summary>
    /// The method may be invoked over unreliable transports such as UDP, and therefore calls may be lost.
    /// </summary>
    Unreliable,
    /// <summary>
    /// The method may be invoked over unreliable transports such as UDP, and therefore calls may be lost. However, the order of calls is preserved.
    /// </summary>
    UnreliableOrdered,
}
