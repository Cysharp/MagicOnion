using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Internal;

internal static class SyntaxHelper
{
    public static bool IsCandidateInterface(SyntaxNode node)
        => node is InterfaceDeclarationSyntax interfaceDeclaration &&
           (interfaceDeclaration.BaseList?.Types.Any() ?? false);
}

