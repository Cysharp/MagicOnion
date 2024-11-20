using MagicOnion.Server.JsonTranscoding.Swagger;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServerJsonTranscodingSwaggerBuilderExtension
{
    public static void AddMagicOnionJsonTranscodingSwagger(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, MagicOnionJsonTranscodingDescriptionProvider>());
    }
}
