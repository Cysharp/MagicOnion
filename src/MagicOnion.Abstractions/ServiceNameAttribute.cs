namespace MagicOnion;

/// <summary>
/// Specifies a custom gRPC service name for the MagicOnion service or StreamingHub interface.
/// When applied, this name is used instead of the default interface type name for gRPC routing.
/// This allows multiple service interfaces with the same short name but different namespaces
/// to coexist on the same server.
/// </summary>
/// <remarks>
/// The attribute must be applied consistently on the shared interface that is referenced
/// by both client and server. The specified name becomes part of the gRPC method path
/// (e.g., <c>/Custom.ServiceName/MethodName</c>).
/// </remarks>
/// <example>
/// <code>
/// [ServiceName("MyNamespace.IMyService")]
/// public interface IMyService : IService&lt;IMyService&gt;
/// {
///     UnaryResult&lt;string&gt; HelloAsync();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class ServiceNameAttribute : Attribute
{
    /// <summary>
    /// Gets the custom service name used for gRPC routing.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceNameAttribute"/> with the specified service name.
    /// </summary>
    /// <param name="name">The custom service name to use for gRPC routing.</param>
    public ServiceNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name cannot be null or whitespace.", nameof(name));
        Name = name;
    }
}
