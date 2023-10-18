using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

[Generator]
public partial class MagicOnionClientSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(static () => new SyntaxContextReceiver());
        context.RegisterForPostInitialization(static context => AddAttributeSources(context.AddSource));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxReceiver!;
        var options = GeneratorOptions.Create(context.AdditionalFiles, context.CancellationToken);
        if (ReferenceSymbols.TryCreate(context.Compilation, out var referenceSymbols))
        {
            var generationContext = new GenerationContext(null, "MagicOnionInitializer", new SourceProductionContext(context));
            Generate(generationContext, syntaxReceiver.Candidates.ToImmutableArray(), context.Compilation, referenceSymbols);
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

public readonly struct SourceProductionContext
{
    readonly GeneratorExecutionContext context;

    public SourceProductionContext(GeneratorExecutionContext context)
    {
        this.context = context;
    }

    public CancellationToken CancellationToken
        => context.CancellationToken;

    public void AddSource(string hintName, string source)
        => context.AddSource(hintName, source);

    public void ReportDiagnostic(Diagnostic diagnostic)
        => context.ReportDiagnostic(diagnostic);
}
