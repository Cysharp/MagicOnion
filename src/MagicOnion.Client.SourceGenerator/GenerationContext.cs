using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

public readonly record struct GenerationContext(
    string? Namespace,
    string InitializerPartialTypeName,
    SourceProductionContext SourceProductionContext,
    GenerationOptions Options
);


// This enum must be mirror of generated `SerializerType` (MagicOnionClientSourceGenerator.Emit)
public enum SerializerType
{
    MessagePack = 0,
    MemoryPack = 1,
}

public record GenerationOptions(SerializerType Serializer, bool DisableAutoRegistration, string MessagePackFormatterNamespace)
{
    public static GenerationOptions Default { get; } = new (SerializerType.MessagePack, false, "MessagePack.Formatters");
}
