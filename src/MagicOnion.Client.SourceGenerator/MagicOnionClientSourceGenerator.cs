using System.Collections.Immutable;
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
            predicate: static (node, cancellationToken) => true,
            transform: static (ctx, cancellationToken) => ((ClassDeclarationSyntax)ctx.TargetNode, ctx.Attributes, ctx.SemanticModel));

        context.RegisterPostInitializationOutput(static context => AddAttributeSources(context.AddSource));

        context.RegisterSourceOutput(generationAttr.Combine(referenceSymbols), static (sourceProductionContext, value) =>
        {
            var ((initializerClassDecl, attrs, semanticModel), referenceSymbols) = value;
            if (referenceSymbols is null) return; // TODO: ReportDiagnostic

            var interfaceSymbols = new List<INamedTypeSymbol>();
            var attr = attrs.FirstOrDefault(x => x.AttributeClass?.Name == MagicOnionClientGenerationAttributeName);
            if (attr is null) return; // TODO: ReportDiagnostic

            // If the constructor has errors in the arguments, the first argument may be `Type` instead of `Array`. We need `Array` to proceed.
            if (attr.ConstructorArguments[0].Kind != TypedConstantKind.Array) return; // TODO: ReportDiagnostic

            foreach (var typeContainedInTargetAssembly in attr.ConstructorArguments[0].Values) // Type[] typesContainedInTargetAssembly
            {
                if (typeContainedInTargetAssembly.Value is INamedTypeSymbol typeSymbolContainedInTargetAssembly)
                {
                    var scanTargetAssembly = typeSymbolContainedInTargetAssembly.ContainingAssembly;
                    if (scanTargetAssembly is null) continue;

                    Traverse(scanTargetAssembly.GlobalNamespace, interfaceSymbols);
                }
            }

            var initializerClassSymbol = semanticModel.GetDeclaredSymbol(initializerClassDecl) as INamedTypeSymbol;
            if (initializerClassSymbol is null) return; // TODO: ReportDiagnostic

            // TODO: ReportDiagnostic if the class is not partial.

            var initializerPartialTypeNamespace = initializerClassSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : initializerClassSymbol.ContainingNamespace.ToDisplayString();
            var initializerPartialTypeName = initializerClassSymbol.Name;

            var option = GenerationOptions.Parse(attr);
            var generationContext = new GenerationContext(initializerPartialTypeNamespace, initializerPartialTypeName, sourceProductionContext, option);
            Generate(generationContext, interfaceSymbols.ToImmutableArray(), referenceSymbols);

            static void Traverse(INamespaceOrTypeSymbol rootNamespaceOrTypeSymbol, List<INamedTypeSymbol> interfaceSymbols)
            {
                foreach (var namespaceOrTypeSymbol in rootNamespaceOrTypeSymbol.GetMembers())
                {
                    if (namespaceOrTypeSymbol is INamedTypeSymbol { TypeKind: TypeKind.Interface } typeSymbol)
                    {
                        interfaceSymbols.Add(typeSymbol);
                    }
                    else if (namespaceOrTypeSymbol is INamespaceSymbol namespaceSymbol)
                    {
                        Traverse(namespaceSymbol, interfaceSymbols);
                    }
                }
            }
        });
    }

}
