using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.Internal;
using MagicOnion.Generator;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Unity;

[Generator]
public class MagicOnionClientSourceGeneratorRoslyn3 : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxReceiver!;
        var options = GeneratorOptions.Create(context.AdditionalFiles, context.CancellationToken);
        if (ReferenceSymbols.TryCreate(context.Compilation, out var referenceSymbols))
        {
            var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance);
            var interfaceSymbols = syntaxReceiver.Candidates
                .Select(x => (INamedTypeSymbol)context.Compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)!)
                .ToImmutableArray();
            var outputs = compiler.Generate(interfaceSymbols, referenceSymbols, options, context.CancellationToken);
            foreach (var output in outputs)
            {
                context.AddSource(output.Path, output.Source);
            }
        }
    }
}

class SyntaxContextReceiver : ISyntaxReceiver
{
    public List<SyntaxNode> Candidates { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (SyntaxHelper.IsCandidateInterface(syntaxNode))
        {
            Candidates.Add(syntaxNode);
        }
    }
}
