namespace MagicOnion.Client.SourceGenerator.CodeGen;

public interface ISerializerFormatterGenerator
{
    string Build(SerializationFormatterCodeGenContext ctx);
}
