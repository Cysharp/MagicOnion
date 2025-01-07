using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Client.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class MagicOnionClientSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var referenceSymbols = context.CompilationProvider
            .Select(static (x, cancellationToken) => ReferenceSymbols.TryCreate(x, out var rs) ? rs : default)
            .WithTrackingName("mo_ReferenceSymbols");
        var generationAttr = context.SyntaxProvider.ForAttributeWithMetadataName(
            MagicOnionClientGenerationAttributeFullName,
            predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax,
            transform: static (ctx, cancellationToken) => ((ClassDeclarationSyntax)ctx.TargetNode, ctx.Attributes, ctx.SemanticModel));

        context.RegisterSourceOutput(generationAttr.Combine(referenceSymbols), static (sourceProductionContext, value) =>
        {
            var ((initializerClassDecl, attrs, semanticModel), referenceSymbols) = value;
            if (referenceSymbols is null) return; // TODO: ReportDiagnostic

            var attr = attrs.FirstOrDefault(x => x.AttributeClass?.Name == MagicOnionClientGenerationAttributeName);
            if (attr is null) return; // TODO: ReportDiagnostic

            var options = ParseClientGenerationOptions(attr);
            if (!TryParseClientGenerationSpec(sourceProductionContext, semanticModel, initializerClassDecl, attr, out var spec))
            {
                return;
            }

            var generationContext = new GenerationContext(spec.InitializerPartialTypeNamespace, spec.InitializerPartialTypeName, sourceProductionContext, options);
            Emitter.Emit(generationContext, spec.InterfaceSymbols, referenceSymbols);
        });
    }
}
