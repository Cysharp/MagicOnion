using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Utils;

// Utility and Extension methods for Roslyn
internal static class RoslynExtensions
{
    public static AttributeData? FindAttributeShortName(this IEnumerable<AttributeData> attributeDataList,
        string typeName)
    {
        return attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == typeName);
    }

    public static bool ApproximatelyEqual(this ITypeSymbol left, ITypeSymbol right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;

        if (left is IErrorTypeSymbol || right is IErrorTypeSymbol)
        {
            return left.ToDisplayString() == right.ToDisplayString();
        }
        else
        {
            return SymbolEqualityComparer.Default.Equals(left, right);
        }
    }

    public static Location? GetLocation(this ISymbol typeSymbol)
        => typeSymbol.Locations.Length > 0 ? typeSymbol.Locations[0] : null;

    public static Location? GetLocation(this AttributeData attributeData)
    {
        SyntaxReference? reference = attributeData.ApplicationSyntaxReference;
        return reference?.SyntaxTree.GetLocation(reference.Span);
    }
}
