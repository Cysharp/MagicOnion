using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public static class MethodCollectorTestHelper
{
    public static IEnumerable<INamedTypeSymbol> Traverse(INamespaceOrTypeSymbol rootNamespaceOrTypeSymbol)
    {
        foreach (var namespaceOrTypeSymbol in rootNamespaceOrTypeSymbol.GetMembers())
        {
            if (namespaceOrTypeSymbol is INamedTypeSymbol { TypeKind: TypeKind.Interface } typeSymbol)
            {
                yield return typeSymbol;
            }
            else if (namespaceOrTypeSymbol is INamespaceSymbol namespaceSymbol)
            {
                foreach (var t in Traverse(namespaceSymbol))
                {
                    yield return t;
                }
            }
        }
    }
}
