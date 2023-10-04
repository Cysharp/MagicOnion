using MagicOnion.Generator;
using MagicOnion.Generator.Internal;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Client.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class MagicOnionClientSourceGenerator : ISourceGenerator
{
    const string OptionsAttributeName = "MagicOnionClientSourceGeneratorOptions";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var @namespace = "MagicOnion";
        var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance, context.CancellationToken);
        var outputs = compiler.GenerateAsync(context.Compilation, "MagicOnionClient.g.cs",
            disableAutoRegister: false,
            @namespace: @namespace,
            userDefinedFormattersNamespace: "MessagePack.Formatters",
            serializerType: SerializerType.MessagePack
        ).GetAwaiter().GetResult();

        var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;

        foreach (var output in outputs)
        {
            context.AddSource(output.Path, output.Source);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
        }
    }
}
