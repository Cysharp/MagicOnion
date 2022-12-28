namespace MagicOnion.Generator.CodeGen;

public interface ISerializerFormatterGenerator
{
    string Build(SerializationFormatterCodeGenContext ctx);
}
