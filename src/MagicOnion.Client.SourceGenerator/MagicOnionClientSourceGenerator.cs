using MagicOnion.Generator;
using MagicOnion.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class MagicOnionClientSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance, context.CancellationToken);
        var options = GeneratorOptions.Create(context.AdditionalFiles, context.CancellationToken);
        var outputs = compiler.GenerateAsync(context.Compilation, options).GetAwaiter().GetResult();

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
