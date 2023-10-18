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
    public static GenerationOptions Default { get; } = new GenerationOptions(SerializerType.MessagePack, false, "MessagePack.Formatters");

    public static GenerationOptions Parse(AttributeData attr)
    {
        var options = GenerationOptions.Default;

        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Value.Kind is TypedConstantKind.Error or not TypedConstantKind.Primitive) continue;

            switch (namedArg.Key)
            {
                case nameof(GenerationOptions.DisableAutoRegistration):
                    options = options with { DisableAutoRegistration = (bool)namedArg.Value.Value! };
                    break;
                case nameof(GenerationOptions.Serializer):
                    options = options with { Serializer = (SerializerType)(int)namedArg.Value.Value! };
                    break;
                case nameof(GenerationOptions.MessagePackFormatterNamespace):
                    options = options with { MessagePackFormatterNamespace = (string)namedArg.Value.Value! };
                    break;
            }
        }

        return options;
    }
}
