using System.CodeDom.Compiler;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

public class SerializationFormatterCodeGenContext
{
    readonly StringWriter underlyingWriter;

    public string FormatterNamespace { get; }
    public IReadOnlyList<ISerializationFormatterRegisterInfo> FormatterRegistrations { get; }
    public IReadOnlyList<SerializationTypeHintInfo> TypeHints { get; }

    public TextWriter TextWriter => underlyingWriter;

    public SerializationFormatterCodeGenContext(string formatterNamespace, IReadOnlyList<ISerializationFormatterRegisterInfo> formatterRegistrations, IReadOnlyList<SerializationTypeHintInfo> typeHints)
    {
        FormatterNamespace = formatterNamespace;
        FormatterRegistrations = formatterRegistrations;
        TypeHints = typeHints;

        underlyingWriter = new StringWriter();
    }

    public string GetWrittenText() => underlyingWriter.ToString();
}
