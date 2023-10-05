using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MagicOnion.Generator;

/*
[Option("u", "Do not use UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer. (Same as --disable-auto-register)")]bool noUseUnityAttr = false,
[Option("d", "Do not automatically call MagicOnionInitializer during start-up. (Automatic registration requires .NET 5+ or Unity)")]bool disableAutoRegister = false,
[Option("n", "The namespace of clients to generate.")]string @namespace = "MagicOnion",
[Option("m", "The namespace of pre-generated MessagePackFormatters.")]string messagepackFormatterNamespace = "MessagePack.Formatters",
[Option("s", "The serializer used for message serialization")] SerializerType serializer = SerializerType.MessagePack
*/

public record GeneratorOptions
{
    public const string JsonFileName = "MagicOnionClientGeneratorOptions.json";

    public static GeneratorOptions Default { get; } = new GeneratorOptions();

    /// <summary>
    /// Gets or sets whether to disable automatically calling MagicOnionInitializer during start-up. (Automatic registration requires .NET 5+ or Unity)
    /// </summary>
    public bool DisableAutoRegister { get; init; }

    /// <summary>
    /// Gets or sets the namespace of clients to generate. The default value is <c>MagicOnion</c>.
    /// </summary>
    public string Namespace { get; init; } = "MagicOnion";

    /// <summary>
    /// Gets or set the namespace of pre-generated MessagePackFormatters. The default value is <c>MessagePack.Formatters</c>.
    /// </summary>
    public string MessagePackFormatterNamespace { get; init; } = "MessagePack.Formatters";

    /// <summary>
    /// Gets or set the serializer used for message serialization. The default value is <see cref="SerializerType.MessagePack"/>.
    /// </summary>
    public SerializerType Serializer { get; init; } = SerializerType.MessagePack;

    public enum SerializerType
    {
        MessagePack,
        MemoryPack
    }

    public static GeneratorOptions Create(ImmutableArray<AdditionalText> additionalTexts, CancellationToken cancellationToken)
    {
        var options = Default;

        var optionsJsonFile = additionalTexts.FirstOrDefault(x => string.Equals(Path.GetFileName(x.Path), JsonFileName, StringComparison.InvariantCultureIgnoreCase));
        if (optionsJsonFile != null)
        {
            var content = optionsJsonFile.GetText(cancellationToken)?.ToString();
            if (content != null)
            {
                options = JsonSerializer.Deserialize<GeneratorOptions>(content, new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                }) ?? Default;
            }
        }

        return options;
    }
}
