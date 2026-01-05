using Microsoft.CodeAnalysis;

namespace MagicOnion.Server.SourceGenerator;

internal static class TypeExtensions
{
    public static string GetFullDeclaringTypeName(this Type type)
    {
        var typeNameParts = new List<string>();
        while (type != null)
        {
            typeNameParts.Insert(0, type.Name);
            type = type.DeclaringType!;
        }
        return string.Join(".", typeNameParts);
    }

    public static string GetFullDeclaringTypeName(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true, IsValueType: true, ContainingNamespace.Name: "System", Name: "ValueTuple" })
        {
            return "ValueTuple";
        }

        return typeSymbol.ToDisplayString(
            new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                SymbolDisplayGenericsOptions.None,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
            ));
    }
}
