using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MagicOnion.Generator;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Client.SourceGenerator;

#if LEGACY_SOURCE_GENERATOR
[Generator(LanguageNames.CSharp)]
public class MagicOnionClientSourceGeneratorRoslyn3 : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;
        var options = GeneratorOptions.Create(context.AdditionalFiles, context.CancellationToken);
        if (ReferenceSymbols.TryCreate(context.Compilation, out var referenceSymbols))
        {
            var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance);
            var outputs = compiler.Generate(syntaxReceiver.Candidates.ToImmutableArray(), referenceSymbols, options, context.CancellationToken);
            foreach (var output in outputs)
            {
                context.AddSource(output.Path, output.Source);
            }
        }
    }
}

class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public List<INamedTypeSymbol> Candidates { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (SyntaxHelper.IsCandidateInterface(context.Node))
        {
            Candidates.Add((INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!);
        }
    }
}


#else
[Generator(LanguageNames.CSharp)]
public class MagicOnionClientSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AdditionalTextsProvider.Collect()
            .Select(static (x, cancellationToken) => GeneratorOptions.Create(x, cancellationToken))
            .WithTrackingName("mo_GeneratorOptions");
        var referenceSymbols = context.CompilationProvider
            .Select(static (x, cancellationToken) => ReferenceSymbols.TryCreate(x, out var rs) ? rs : default)
            .WithTrackingName("mo_ReferenceSymbols");
        var interfaces = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, ct) => SyntaxHelper.IsCandidateInterface(node),
            transform: (ctx, ct) => ctx.Node)
            .Collect()
            .WithTrackingName("mo_Interfaces");

        var source = options
            .Combine(interfaces)
            .Combine(referenceSymbols)
            .Combine(context.CompilationProvider)
            .WithTrackingName("mo_Source");

        context.RegisterSourceOutput(source, static (sourceProductionContext, pair) =>
        {
            var (((options, interfaces), referenceSymbols), compilation) = pair;
            var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance);
            var symbols = interfaces.Select(x => (INamedTypeSymbol)compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)!).ToImmutableArray();
            var generated = compiler.Generate(symbols, referenceSymbols, options, sourceProductionContext.CancellationToken);
            foreach (var (path, source) in generated)
            {
                sourceProductionContext.AddSource(path, source);
            }
        });
    }
}
#endif

static class SyntaxHelper
{
    public static bool IsCandidateInterface(SyntaxNode node)
        => node is InterfaceDeclarationSyntax interfaceDeclaration && (interfaceDeclaration.BaseList?.Types.Any() ?? false);
}
