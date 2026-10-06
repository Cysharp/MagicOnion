namespace MagicOnion;

/// <summary>
/// Specifies the delivery mode of a StreamingHub or receiver contract interface, or of a single contract method.
/// </summary>
/// <remarks>
/// <para>
/// Apply the attribute to a shared contract interface to set the default mode for the methods it declares or inherits,
/// or to a method to override the default. A method declaration takes precedence over the connected root interface,
/// which takes precedence over inherited interface defaults. Attributes on implementation classes are not used.
/// </para>
/// <para>
/// <see cref="UnreliableAttribute"/>, <see cref="UnreliableUnorderedAttribute"/>, and <see cref="ReliableAttribute"/> are shorthand forms.
/// Specifying more than one of these attributes on the same interface or method is a contract error.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class TransportAttribute : Attribute
{
    /// <summary>
    /// Gets the delivery mode specified by the attribute.
    /// </summary>
    public StreamingHubTransportMode Mode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransportAttribute"/> class with the specified delivery mode.
    /// </summary>
    /// <param name="mode">The delivery mode.</param>
    public TransportAttribute(StreamingHubTransportMode mode)
    {
        Mode = mode;
    }
}

/// <summary>
/// Specifies that a StreamingHub or receiver method, or every method of a contract interface, is delivered without an arrival guarantee
/// while discarding messages older than the newest accepted message. This is shorthand for <c>[Transport(StreamingHubTransportMode.UnreliableSequenced)]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class UnreliableAttribute : TransportAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnreliableAttribute"/> class.
    /// </summary>
    public UnreliableAttribute() : base(StreamingHubTransportMode.UnreliableSequenced)
    {
    }
}

/// <summary>
/// Specifies that a StreamingHub or receiver method, or every method of a contract interface, is delivered without an arrival or ordering guarantee.
/// This is shorthand for <c>[Transport(StreamingHubTransportMode.UnreliableUnordered)]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class UnreliableUnorderedAttribute : TransportAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnreliableUnorderedAttribute"/> class.
    /// </summary>
    public UnreliableUnorderedAttribute() : base(StreamingHubTransportMode.UnreliableUnordered)
    {
    }
}

/// <summary>
/// Specifies that a StreamingHub or receiver method, or every method of a contract interface, is delivered reliably over the gRPC connection.
/// This is shorthand for <c>[Transport(StreamingHubTransportMode.Reliable)]</c> and is used to override an unreliable interface default.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class ReliableAttribute : TransportAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReliableAttribute"/> class.
    /// </summary>
    public ReliableAttribute() : base(StreamingHubTransportMode.Reliable)
    {
    }
}
