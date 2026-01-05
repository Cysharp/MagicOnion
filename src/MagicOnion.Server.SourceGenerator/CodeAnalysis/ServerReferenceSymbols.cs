using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Server.SourceGenerator.CodeAnalysis;

/// <summary>
/// Holds reference symbols for MagicOnion server types.
/// </summary>
public record ServerReferenceSymbols(
    INamedTypeSymbol IServiceMarker,
    INamedTypeSymbol IService,
    INamedTypeSymbol IStreamingHubMarker,
    INamedTypeSymbol IStreamingHub,
    INamedTypeSymbol? IgnoreAttribute,
    INamedTypeSymbol? MethodIdAttribute,
    INamedTypeSymbol UnaryResult,
    INamedTypeSymbol UnaryResultOfT,
    INamedTypeSymbol? ClientStreamingResult,
    INamedTypeSymbol? ServerStreamingResult,
    INamedTypeSymbol? DuplexStreamingResult,
    INamedTypeSymbol Task,
    INamedTypeSymbol TaskOfT,
    INamedTypeSymbol ValueTask,
    INamedTypeSymbol ValueTaskOfT
)
{
    public static bool TryCreate(Compilation compilation, [NotNullWhen(true)] out ServerReferenceSymbols? referenceSymbols)
    {
        var symbolIServiceMarker = compilation.GetTypeByMetadataName("MagicOnion.IServiceMarker");
        var symbolIService = compilation.GetTypeByMetadataName("MagicOnion.IService`1");
        var symbolIStreamingHubMarker = compilation.GetTypeByMetadataName("MagicOnion.IStreamingHubMarker");
        var symbolIStreamingHub = compilation.GetTypeByMetadataName("MagicOnion.IStreamingHub`2");
        var symbolUnaryResult = compilation.GetTypeByMetadataName("MagicOnion.UnaryResult");
        var symbolUnaryResultOfT = compilation.GetTypeByMetadataName("MagicOnion.UnaryResult`1");
        var symbolTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var symbolTaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var symbolValueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var symbolValueTaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

        if (symbolIServiceMarker is null ||
            symbolIService is null ||
            symbolIStreamingHubMarker is null ||
            symbolIStreamingHub is null ||
            symbolUnaryResult is null ||
            symbolUnaryResultOfT is null ||
            symbolTask is null ||
            symbolTaskOfT is null ||
            symbolValueTask is null ||
            symbolValueTaskOfT is null)
        {
            referenceSymbols = default;
            return false;
        }

        referenceSymbols = new ServerReferenceSymbols(
            symbolIServiceMarker,
            symbolIService,
            symbolIStreamingHubMarker,
            symbolIStreamingHub,
            compilation.GetTypeByMetadataName("MagicOnion.Server.IgnoreAttribute") ?? compilation.GetTypeByMetadataName("MagicOnion.IgnoreAttribute"),
            compilation.GetTypeByMetadataName("MagicOnion.Server.Hubs.MethodIdAttribute"),
            symbolUnaryResult,
            symbolUnaryResultOfT,
            compilation.GetTypeByMetadataName("MagicOnion.ClientStreamingResult`2"),
            compilation.GetTypeByMetadataName("MagicOnion.ServerStreamingResult`1"),
            compilation.GetTypeByMetadataName("MagicOnion.DuplexStreamingResult`2"),
            symbolTask,
            symbolTaskOfT,
            symbolValueTask,
            symbolValueTaskOfT
        );
        return true;
    }
}
