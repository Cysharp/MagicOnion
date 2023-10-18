using Microsoft.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace MagicOnion.Client.SourceGenerator.Tests;

internal static class MicrosoftCodeAnalysisExtensions
{
    public static ISourceGenerator AsSourceGenerator(this ISourceGenerator generator) => generator; // Shim
}
