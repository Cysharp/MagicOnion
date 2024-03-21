using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Client.SourceGenerator;

public partial class MagicOnionClientSourceGenerator
{
    static bool TryParseClientGenerationSpec(SourceProductionContext sourceProductionContext, SemanticModel semanticModel, ClassDeclarationSyntax initializerDeclarationSyntax, AttributeData attr, [NotNullWhen(true)] out ClientGenerationSpec? spec)
    {
        // If the constructor has errors in the arguments, the first argument may be `Type` instead of `Array`. We need `Array` to proceed.
        if (attr.ConstructorArguments[0].Kind != TypedConstantKind.Array)
        {
            spec = null;
            return false; // TODO: ReportDiagnostic
        }

        var interfaceSymbols = new List<INamedTypeSymbol>();
        foreach (var typeContainedInTargetAssembly in attr.ConstructorArguments[0].Values) // Type[] typesContainedInTargetAssembly
        {
            if (typeContainedInTargetAssembly.Value is INamedTypeSymbol typeSymbolContainedInTargetAssembly)
            {
                var scanTargetAssembly = typeSymbolContainedInTargetAssembly.ContainingAssembly;
                if (scanTargetAssembly is null) continue;

                Traverse(scanTargetAssembly.GlobalNamespace, interfaceSymbols);
            }
        }

        var initializerClassSymbol = semanticModel.GetDeclaredSymbol(initializerDeclarationSyntax) as INamedTypeSymbol;
        if (initializerClassSymbol is null)
        {
            spec = null;
            return false;
        }

        if (initializerClassSymbol.IsValueType)
        {
            sourceProductionContext.ReportDiagnostic(Diagnostic.Create(MagicOnionDiagnosticDescriptors.TypeSpecifyingClientGenerationAttributedMustBePartial, initializerClassSymbol.GetLocation()));
            spec = null;
            return false;
        }
        if (!initializerDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
        {
            sourceProductionContext.ReportDiagnostic(Diagnostic.Create(MagicOnionDiagnosticDescriptors.TypeSpecifyingClientGenerationAttributedMustBePartial, initializerClassSymbol.GetLocation()));
            spec = null;
            return false;
        }

        var initializerPartialTypeNamespace = initializerClassSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : initializerClassSymbol.ContainingNamespace.ToDisplayString();
        var initializerPartialTypeName = initializerClassSymbol.Name;

        spec = new ClientGenerationSpec(initializerPartialTypeNamespace, initializerPartialTypeName, interfaceSymbols.ToImmutableArray());
        return true;

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
    }

    static GenerationOptions ParseClientGenerationOptions(AttributeData attr)
    {
        var options = GenerationOptions.Default;

        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Value.Kind is TypedConstantKind.Error or (not TypedConstantKind.Primitive and not TypedConstantKind.Enum)) continue;

            switch (namedArg.Key)
            {
                case nameof(GenerationOptions.DisableAutoRegistration):
                    options = options with { DisableAutoRegistration = (bool)namedArg.Value.Value! };
                    break;
                case nameof(GenerationOptions.Serializer):
                    options = options with { Serializer = (SerializerType)(int)namedArg.Value.Value! };
                    break;
                case nameof(GenerationOptions.MessagePackFormatterNamespace):
                    options = options with { MessagePackFormatterNamespace = (string)namedArg.Value.Value! };
                    break;
                case nameof(GenerationOptions.EnableStreamingHubDiagnosticHandler):
                    options = options with { EnableStreamingHubDiagnosticHandler = (bool)namedArg.Value.Value! };
                    break;
                case nameof(GenerationOptions.GenerateFileHintNamePrefix):
                    options = options with { GenerateFileHintNamePrefix = (string)namedArg.Value.Value! };
                    break;
            }
        }

        return options;
    }

    record ClientGenerationSpec(string? InitializerPartialTypeNamespace, string InitializerPartialTypeName, ImmutableArray<INamedTypeSymbol> InterfaceSymbols);
}
