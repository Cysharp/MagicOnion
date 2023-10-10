using System.CodeDom.Compiler;
using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Generator.CodeGen;

public class SerializationFormatterCodeGenContext
{
    readonly StringWriter underlyingWriter;

    public string Namespace { get; }
    public string FormatterNamespace { get; }
    public string InitializerName { get; }
    public IReadOnlyList<ISerializationFormatterRegisterInfo> FormatterRegistrations { get; }
    public IReadOnlyList<SerializationTypeHintInfo> TypeHints { get; }

    public IndentedTextWriter TextWriter { get; }

    public SerializationFormatterCodeGenContext(string @namespace, string formatterNamespace, string initializerName, IReadOnlyList<ISerializationFormatterRegisterInfo> formatterRegistrations, IReadOnlyList<SerializationTypeHintInfo> typeHints)
    {
        Namespace = @namespace;
        FormatterNamespace = formatterNamespace;
        InitializerName = initializerName;
        FormatterRegistrations = formatterRegistrations;
        TypeHints = typeHints;

        underlyingWriter = new StringWriter();
        TextWriter = new IndentedTextWriter(underlyingWriter);
    }

    public string GetWrittenText() => underlyingWriter.ToString();
}
