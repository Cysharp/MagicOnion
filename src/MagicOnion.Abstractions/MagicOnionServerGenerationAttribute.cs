namespace MagicOnion;

/// <summary>
/// Marks a partial class as the entry point for MagicOnion server source generation.
/// The generated code will implement IMagicOnionGrpcMethodProvider for AOT-compatible service registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MagicOnionServerGenerationAttribute : Attribute
{
    /// <summary>
    /// Gets the service implementation types to generate method providers for.
    /// If empty, the generator will scan for all IService and IStreamingHub implementations in the compilation.
    /// </summary>
    public Type[] ServiceTypes { get; }

    /// <summary>
    /// Initializes a new instance with automatic service discovery.
    /// </summary>
    public MagicOnionServerGenerationAttribute()
    {
        ServiceTypes = Array.Empty<Type>();
    }

    /// <summary>
    /// Initializes a new instance with explicit service types.
    /// </summary>
    /// <param name="serviceTypes">The service implementation types to generate.</param>
    public MagicOnionServerGenerationAttribute(params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes;
    }
}
