using System.Text.Json;
using MagicOnion.Server.JsonTranscoding.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServerJsonTranscodingSwaggerBuilderExtension
{
    /// <summary>
    /// Adds Swagger support for MagicOnion JSON transcoding.
    /// </summary>
    /// <param name="services"></param>
    public static void AddMagicOnionJsonTranscodingSwagger(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, MagicOnionJsonTranscodingDescriptionProvider>());

        services.Replace(ServiceDescriptor.Transient<ISerializerDataContractResolver>(s =>
        {
            var serializerOptions = s.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions ?? new JsonSerializerOptions();
            var innerContractResolver = new JsonSerializerDataContractResolver(serializerOptions);
            return new MagicOnionGrpcJsonDataContractResolver(innerContractResolver);
        }));

        services.PostConfigure<SwaggerGenOptions>(options =>
        {
            options.AddRequestBodyFilterInstance(new DynamicArgumentTupleRequestBodyFilter());
        });
    }
}
