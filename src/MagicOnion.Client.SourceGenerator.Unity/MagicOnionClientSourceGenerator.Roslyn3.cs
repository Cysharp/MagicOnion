using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                var classSymbol = semanticModel.GetDeclaredSymbol(initializerClassDecl);
                if (classSymbol is null) continue;

                var attrs = classSymbol.GetAttributes();
                var attr = attrs.FirstOrDefault(x => string.Equals(x.AttributeClass?.Name, MagicOnionClientGenerationAttributeName, StringComparison.Ordinal));
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
    public IReadOnlyList<(ClassDeclarationSyntax Node, SemanticModel SemanticModel)> Candidates { get; private set; } = Array.Empty<(ClassDeclarationSyntax Node, SemanticModel SemanticModel)>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        List<(ClassDeclarationSyntax Node, SemanticModel SemanticModel)>? candidates = default;

        if (context.Node is AttributeSyntax attrSyntax &&
            attrSyntax.Parent is AttributeListSyntax &&
            attrSyntax.Parent.Parent is ClassDeclarationSyntax classDeclSyntax)
        {
            var attrName = attrSyntax.Name is QualifiedNameSyntax qualifiedName
                ? qualifiedName.Right.Identifier.ValueText
                : ((IdentifierNameSyntax)attrSyntax.Name).Identifier.ValueText;

            if (attrName.StartsWith(MagicOnionClientSourceGenerator.MagicOnionClientGenerationAttributeShortName, StringComparison.Ordinal))
            {
                if (string.Equals(attrName, MagicOnionClientSourceGenerator.MagicOnionClientGenerationAttributeShortName, StringComparison.Ordinal) ||
                    string.Equals(attrName, MagicOnionClientSourceGenerator.MagicOnionClientGenerationAttributeName, StringComparison.Ordinal))
                {
                    var attrSymbol = context.SemanticModel.GetSymbolInfo(attrSyntax).Symbol as IMethodSymbol;
                    if (attrSymbol is null) return;

                    candidates ??= new List<(ClassDeclarationSyntax Node, SemanticModel SemanticModel)>();

                    candidates.Add((classDeclSyntax, context.SemanticModel));
                }
            }
        }

        if (candidates is not null)
        {
            Candidates = candidates;
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
