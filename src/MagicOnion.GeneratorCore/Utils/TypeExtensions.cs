using System.Text;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.Utils;

internal static class TypeExtensions
{
    public static string GetFullDeclaringTypeName(this Type type)
    {
        var typeNameParts = new List<string>();
        while (type != null)
        {
            typeNameParts.Insert(0, type.Name);
            type = type.DeclaringType;
        }
        return string.Join(".", typeNameParts);
    }

    public static string GetFullDeclaringTypeName(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(
            new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                SymbolDisplayGenericsOptions.None,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
            ));
    }

}
