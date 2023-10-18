namespace MagicOnion.Client.SourceGenerator.CodeGen;

public interface ISerializerFormatterGenerator
{
    string Build(GenerationContext generationContext, SerializationFormatterCodeGenContext ctx);
}
