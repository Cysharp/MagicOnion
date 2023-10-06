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
        var options = context.AdditionalTextsProvider.Collect().Select(static (x, cancellationToken) => GeneratorOptions.Create(x, cancellationToken)).WithTrackingName("GeneratorOptions");
        var referenceSymbols = context.CompilationProvider.Select(static (x, cancellationToken) => ReferenceSymbols.TryCreate(x, out var rs) ? rs : default).WithTrackingName("ReferenceSymbols");

        var compilationAndOptions = context.CompilationProvider.Combine(options);

        context.RegisterSourceOutput(compilationAndOptions, static (sourceProductionContext, pair) =>
        {
            var compiler = new MagicOnionCompiler(MagicOnionGeneratorNullLogger.Instance);
            var generated = compiler.Generate(pair.Item1, pair.Item2, sourceProductionContext.CancellationToken);
            foreach (var (path, source) in generated)
            {
                sourceProductionContext.AddSource(path, source);
            }
        });
    }
}
#endif
