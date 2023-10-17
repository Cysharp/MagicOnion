using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

public partial class MagicOnionClientSourceGenerator
{
    static void Generate(ImmutableArray<SyntaxNode> interfaces, Compilation compilation, ReferenceSymbols referenceSymbols, SourceProductionContext sourceProductionContext, GeneratorOptions options)
    {
        var interfaceSymbols = interfaces.Select(x => (INamedTypeSymbol)compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)!).ToImmutableArray();
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, sourceProductionContext.CancellationToken);
        var generated = MagicOnionClientGenerator.Generate(serviceCollection, options, sourceProductionContext.CancellationToken);

        foreach (var diagnostic in diagnostics)
        {
            sourceProductionContext.ReportDiagnostic(diagnostic);
        }

        foreach (var (path, source) in generated)
        {
            sourceProductionContext.AddSource(path, source);
        }
    }
}
