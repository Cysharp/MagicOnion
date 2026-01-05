using MagicOnion.Server.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Server.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class MagicOnionServerSourceGenerator : IIncrementalGenerator
{
    const string MagicOnionServerGenerationAttributeFullName = "MagicOnion.MagicOnionServerGenerationAttribute";
    const string MagicOnionServerGenerationAttributeName = "MagicOnionServerGenerationAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get reference symbols
        var referenceSymbols = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
                ServerReferenceSymbols.TryCreate(compilation, out var rs) ? rs : default)
            .WithTrackingName("mo_ServerReferenceSymbols");

        // Find classes with [MagicOnionServerGeneration] attribute
        var generationAttr = context.SyntaxProvider.ForAttributeWithMetadataName(
            MagicOnionServerGenerationAttributeFullName,
            predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax,
            transform: static (ctx, cancellationToken) =>
                ((ClassDeclarationSyntax)ctx.TargetNode, ctx.Attributes, ctx.SemanticModel));

        // Collect all service implementation types in the compilation
        var serviceTypes = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax,
            transform: static (ctx, cancellationToken) =>
            {
                var classDecl = (ClassDeclarationSyntax)ctx.Node;
                return ctx.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken) as INamedTypeSymbol;
            })
            .Where(static x => x is not null)
            .Collect()
            .WithTrackingName("mo_AllClassSymbols");

        // Combine and generate
        var combined = generationAttr.Combine(referenceSymbols).Combine(serviceTypes);

        context.RegisterSourceOutput(combined, (sourceProductionContext, value) =>
        {
            var (((classDecl, attrs, semanticModel), referenceSymbols), allClassSymbols) = value;

            if (referenceSymbols is null) return;

            var attr = attrs.FirstOrDefault(x => x.AttributeClass?.Name == MagicOnionServerGenerationAttributeName);
            if (attr is null) return;

            // Validate partial class
            if (!classDecl.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
            {
                sourceProductionContext.ReportDiagnostic(Diagnostic.Create(
                    MagicOnionDiagnosticDescriptors.GenerationAttributeRequiresPartialClass,
                    classDecl.GetLocation(),
                    classDecl.Identifier.Text));
                return;
            }

            // Get namespace and class name
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol is null) return;

            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            // Get explicit service types from attribute or discover all
            var explicitTypes = GetExplicitServiceTypes(attr, semanticModel);
            var serviceImplementations = explicitTypes.Count > 0
                ? explicitTypes
                : DiscoverServiceImplementations(allClassSymbols!, referenceSymbols);

            // Collect service information
            var (services, diagnostics) = ServiceMethodCollector.Collect(
                serviceImplementations,
                referenceSymbols,
                sourceProductionContext.CancellationToken);

            // Report diagnostics
            foreach (var diagnostic in diagnostics)
            {
                sourceProductionContext.ReportDiagnostic(diagnostic);
            }

            if (services.Count == 0)
            {
                sourceProductionContext.ReportDiagnostic(Diagnostic.Create(
                    MagicOnionDiagnosticDescriptors.ServiceImplementationNotFound,
                    classDecl.GetLocation()));
                return;
            }

            // Generate code
            var generatedCode = CodeGen.StaticMethodProviderGenerator.Generate(
                namespaceName,
                className,
                services);

            sourceProductionContext.AddSource($"{className}.g.cs", generatedCode);
        });
    }

    static List<INamedTypeSymbol> GetExplicitServiceTypes(AttributeData attr, SemanticModel semanticModel)
    {
        var types = new List<INamedTypeSymbol>();

        if (attr.ConstructorArguments.Length > 0)
        {
            var arg = attr.ConstructorArguments[0];
            if (arg.Kind == TypedConstantKind.Array)
            {
                foreach (var item in arg.Values)
                {
                    if (item.Value is INamedTypeSymbol typeSymbol)
                    {
                        types.Add(typeSymbol);
                    }
                }
            }
        }

        return types;
    }

    static List<INamedTypeSymbol> DiscoverServiceImplementations(
        System.Collections.Immutable.ImmutableArray<INamedTypeSymbol?> allClassSymbols,
        ServerReferenceSymbols referenceSymbols)
    {
        var services = new List<INamedTypeSymbol>();

        foreach (var symbol in allClassSymbols)
        {
            if (symbol is null) continue;
            if (symbol.IsAbstract) continue;
            if (symbol.TypeKind != TypeKind.Class) continue;

            // Check if implements IService<> or IStreamingHub<,>
            var implementsService = symbol.AllInterfaces.Any(x =>
                x.OriginalDefinition.ApproximatelyEqual(referenceSymbols.IService));
            var implementsHub = symbol.AllInterfaces.Any(x =>
                x.OriginalDefinition.ApproximatelyEqual(referenceSymbols.IStreamingHub));

            if (implementsService || implementsHub)
            {
                services.Add(symbol);
            }
        }

        return services;
    }
}
