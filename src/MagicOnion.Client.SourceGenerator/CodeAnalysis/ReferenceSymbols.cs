using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

public record ReferenceSymbols(INamedTypeSymbol IServiceMarker, INamedTypeSymbol IService, INamedTypeSymbol IStreamingHubMarker, INamedTypeSymbol IStreamingHub)
{
    public static bool TryCreate(Compilation compilation, [NotNullWhen(true)] out ReferenceSymbols? referenceSymbols)
    {
        if (compilation.GetTypeByMetadataName("MagicOnion.IServiceMarker") is { } symbolIServiceMarker &&
            compilation.GetTypeByMetadataName("MagicOnion.IService`1") is { } symbolIService &&
            compilation.GetTypeByMetadataName("MagicOnion.IStreamingHubMarker") is { } symbolIStreamingHubMarker &&
            compilation.GetTypeByMetadataName("MagicOnion.IStreamingHub`2") is { } symbolIStreamingHub
           )
        {
            referenceSymbols = new ReferenceSymbols(
                symbolIServiceMarker,
                symbolIService,
                symbolIStreamingHubMarker,
                symbolIStreamingHub
            );
            return true;
        }

        referenceSymbols = default;
        return false;
    }
}
