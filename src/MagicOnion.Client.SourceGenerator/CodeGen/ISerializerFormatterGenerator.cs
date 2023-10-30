namespace MagicOnion.Client.SourceGenerator.CodeGen;

public interface ISerializerFormatterGenerator
{
    (string HintNameSuffix, string Source) Build(GenerationContext generationContext, SerializationFormatterCodeGenContext ctx);
}
