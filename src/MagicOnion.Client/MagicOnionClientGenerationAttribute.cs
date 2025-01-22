namespace MagicOnion.Client;

/// <summary>
/// Marker attribute for generating clients of MagicOnion.
/// The source generator collects the classes specified by this attribute and uses them to generate source.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MagicOnionClientGenerationAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to disable automatically calling `Register` during start-up. (Automatic registration requires .NET 5+ or Unity)
    /// </summary>
    public bool DisableAutoRegistration { get; set; }

    /// <summary>
    /// Gets or set the serializer used for message serialization. The default value is <see cref="GenerateSerializerType.MessagePack"/>.
    /// </summary>
    public GenerateSerializerType Serializer { get; set; } = GenerateSerializerType.MessagePack;

    /// <summary>
    /// Gets or set the namespace of pre-generated MessagePackFormatters. The default value is <c>MessagePack.Formatters</c>.
    /// </summary>
    [Obsolete("This property is obsolete. Use [MagicOnionClientGenerationOption(\"MessagePack.FormatterNamespace\", \"MessagePack.Formatters\")] instead.")]
    public string MessagePackFormatterNamespace { get; set; } = "MessagePack.Formatters";

    /// <summary>
    /// Gets or set whether to enable the StreamingHandler diagnostic handler. This is for debugging purpose. The default value is <see langword="false" />.
    /// </summary>
    public bool EnableStreamingHubDiagnosticHandler { get; set; } = false;

    public string GenerateFileHintNamePrefix { get; set; } = string.Empty;

    public Type[] TypesContainedInTargetAssembly { get; }

    /// <param name="typesContainedInTargetAssembly">Types contained in the scan target assembly</param>
    public MagicOnionClientGenerationAttribute(params Type[] typesContainedInTargetAssembly)
    {
        TypesContainedInTargetAssembly = typesContainedInTargetAssembly;
    }

    // This enum must be mirror of `SerializerType` (MagicOnionClientSourceGenerator)
    public enum GenerateSerializerType
    {
        MessagePack = 0,
        MemoryPack = 1,
    }
}

/// <summary>
/// Specifies the options for generating clients of MagicOnion.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MagicOnionClientGenerationOptionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the key of the option.
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// Gets or sets the value of the option.
    /// </summary>
    public object? Value { get; set; }

    public MagicOnionClientGenerationOptionAttribute(string key, object value)
    {
        Key = key;
        Value = value;
    }
}
