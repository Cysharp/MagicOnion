using System.Text;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

public readonly record struct GenerationContext(
    string? Namespace,
    string InitializerPartialTypeName,
    SourceProductionContext SourceProductionContext,
    GenerationOptions Options
)
{
    readonly StringBuilder pooledStringBuilder = new();

    public PooledStringBuilder GetPooledStringBuilder()
        => new PooledStringBuilder(pooledStringBuilder);

    public record struct PooledStringBuilder(StringBuilder Instance) : IDisposable
    {
        public void Dispose() => Instance.Clear();
    }
}


// This enum must be mirror of generated `SerializerType` (MagicOnionClientSourceGenerator.Emit)
public enum SerializerType
{
    MessagePack = 0,
    MemoryPack = 1,
}

public record GenerationOptions(
    SerializerType Serializer,
    bool DisableAutoRegistration,
    string MessagePackFormatterNamespace,
    bool EnableStreamingHubDiagnosticHandler,
    string GenerateFileHintNamePrefix
)
{
    public static GenerationOptions Default { get; } = new (
        SerializerType.MessagePack,
        DisableAutoRegistration: false,
        MessagePackFormatterNamespace: "MessagePack.Formatters",
        EnableStreamingHubDiagnosticHandler: false,
        GenerateFileHintNamePrefix: string.Empty
    );
}
