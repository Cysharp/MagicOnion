using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using MagicOnion.Generator;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

#if LEGACY_SOURCE_GENERATOR
[Generator(LanguageNames.CSharp)]
public class MagicOnionClientSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance);
        var options = GeneratorOptions.Create(context.AdditionalFiles, context.CancellationToken);
        var outputs = compiler.Generate(context.Compilation, options, context.CancellationToken);

        var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;

        foreach (var output in outputs)
        {
            context.AddSource(output.Path, output.Source);
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
        var interfaceSymbols = context.CompilationProvider
            .SelectMany(static (x, cancellationToken) => GetNamedTypeSymbols(x))
            .Where(x => x.TypeKind == TypeKind.Interface)
            .WithComparer(SymbolEqualityComparer.Default)
            .Collect()
            .WithTrackingName("mo_InterfaceSymbols");

        var source = options
            .Combine(interfaceSymbols)
            .Combine(referenceSymbols)
            .WithTrackingName("mo_Combined");

        context.RegisterSourceOutput(source, static (sourceProductionContext, pair) =>
        {
            var ((options, interfaceSymbols), referenceSymbols) = pair;
            var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance);
            var generated = compiler.Generate(interfaceSymbols, referenceSymbols, options, sourceProductionContext.CancellationToken);
            foreach (var (path, source) in generated)
            {
                sourceProductionContext.AddSource(path, source);
            }
        });
    }

    public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(Compilation compilation)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semModel = compilation.GetSemanticModel(syntaxTree);

            foreach (var item in syntaxTree.GetRoot()
                         .DescendantNodes()
                         .Select(x => semModel.GetDeclaredSymbol(x))
                         .Where(x => x != null))
            {
                var namedType = item as INamedTypeSymbol;
                if (namedType != null)
                {
                    yield return namedType;
                }
            }
        }
    }
}
#endif
