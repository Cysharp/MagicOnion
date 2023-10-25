using System.CodeDom.Compiler;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

public class SerializationFormatterCodeGenContext
{
    public string FormatterNamespace { get; }
    public IReadOnlyList<ISerializationFormatterRegisterInfo> FormatterRegistrations { get; }
    public IReadOnlyList<SerializationTypeHintInfo> TypeHints { get; }

    public SerializationFormatterCodeGenContext(string formatterNamespace, IReadOnlyList<ISerializationFormatterRegisterInfo> formatterRegistrations, IReadOnlyList<SerializationTypeHintInfo> typeHints)
    {
        FormatterNamespace = formatterNamespace;
        FormatterRegistrations = formatterRegistrations;
        TypeHints = typeHints;
    }
}
