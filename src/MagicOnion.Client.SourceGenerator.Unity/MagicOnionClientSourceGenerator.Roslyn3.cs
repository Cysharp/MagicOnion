using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Client.SourceGenerator;

[Generator]
public partial class MagicOnionClientSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(static () => new SyntaxContextReceiver());
        context.RegisterForPostInitialization(static context => Emitter.AddAttributeSources(context.AddSource));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;
        if (syntaxReceiver.Candidates.Any() && ReferenceSymbols.TryCreate(context.Compilation, out var referenceSymbols))
        {
            var sourceProductionContext = new SourceProductionContext(context);

            foreach (var (initializerClassDecl, semanticModel) in syntaxReceiver.Candidates)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(initializerClassDecl) as INamedTypeSymbol;
                if (classSymbol is null) continue;

                var attrs = classSymbol.GetAttributes();
                var attr = attrs.FirstOrDefault(x => x.AttributeClass?.Name == MagicOnionClientGenerationAttributeName);
                if (attr is null) return; // TODO: ReportDiagnostic

                var options = ParseClientGenerationOptions(attr);
                if (!TryParseClientGenerationSpec(sourceProductionContext, semanticModel, initializerClassDecl, attr, out var spec))
                {
                    return;
                }

                var generationContext = new GenerationContext(spec.InitializerPartialTypeNamespace, spec.InitializerPartialTypeName, sourceProductionContext, options);
                Emitter.Emit(generationContext, spec.InterfaceSymbols, referenceSymbols);
            }
        }
    }
}

class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public List<(ClassDeclarationSyntax Node, SemanticModel SemanticModel)> Candidates { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclSyntax)
        {
            foreach (var attrList in classDeclSyntax.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrSymbol = context.SemanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol;
                    if (attrSymbol is null) continue;

                    var attrContainingType = attrSymbol.ContainingType;
                    if (attrContainingType.ToDisplayString() == MagicOnionClientSourceGenerator.MagicOnionClientGenerationAttributeFullName)
                    {
                        Candidates.Add((classDeclSyntax,context.SemanticModel));
                    }
                }
            }
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
