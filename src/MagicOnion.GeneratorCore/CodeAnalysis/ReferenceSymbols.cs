using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.CodeAnalysis;

public record ReferenceSymbols
{
    public INamedTypeSymbol IServiceMarker { get; init; }
    public INamedTypeSymbol IService { get; init; }
    public INamedTypeSymbol IStreamingHubMarker { get; init; }
    public INamedTypeSymbol IStreamingHub { get; init; }

    public static bool TryCreate(Compilation compilation, out ReferenceSymbols referenceSymbols)
    {
        if (compilation.GetTypeByMetadataName("MagicOnion.IServiceMarker") is { } symbolIServiceMarker &&
            compilation.GetTypeByMetadataName("MagicOnion.IService`1") is { } symbolIService &&
            compilation.GetTypeByMetadataName("MagicOnion.IStreamingHubMarker") is { } symbolIStreamingHubMarker &&
            compilation.GetTypeByMetadataName("MagicOnion.IStreamingHub`2") is { } symbolIStreamingHub
           )
        {
            referenceSymbols = new ReferenceSymbols()
            {
                IService = symbolIService,
                IServiceMarker = symbolIServiceMarker,
                IStreamingHub = symbolIStreamingHub,
                IStreamingHubMarker = symbolIStreamingHubMarker,
            };
            return true;
        }

        referenceSymbols = default;
        return false;
    }
}
