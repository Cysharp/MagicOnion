using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

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
                predicate: static (node, ct) => SyntaxHelper.IsCandidateInterface(node),
                transform: static (ctx, ct) => ctx.Node)
            .Collect()
            .WithTrackingName("mo_Interfaces");

        var source = options
            .Combine(interfaces)
            .Combine(referenceSymbols)
            .Combine(context.CompilationProvider)
            .WithTrackingName("mo_Source");

        context.RegisterSourceOutput(source, static (sourceProductionContext, values) =>
        {
            var (((options, interfaces), referenceSymbols), compilation) = values;
            if (referenceSymbols is null) return;

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
        });
    }
}
