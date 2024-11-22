using System.Text.Json;
using MagicOnion.Server.JsonTranscoding.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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
            options.AddRequestBodyFilterInstance(new RequestBodyFilter());
        });
    }


    class RequestBodyFilter : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            // Dynamically generate a schema for DynamicArgumentTuple that matches the argument name
            if (requestBody.Content.TryGetValue("application/json", out var mediaType) &&
                context.BodyParameterDescription.ModelMetadata.ModelType is { IsGenericType: true } modelType &&
                modelType.GetGenericTypeDefinition() is { FullName: not null} modelOpenType &&
                modelOpenType.FullName.StartsWith("MagicOnion.DynamicArgumentTuple`") &&
                context.BodyParameterDescription.ModelMetadata is MagicOnionJsonRequestResponseModelMetadata modelMetadata &&
                context.SchemaRepository.TryLookupByType(modelType, out var origSchema)
            )
            {
                var idWithParamNames = $"{string.Join("_", modelMetadata.Parameters.Select(x => x.Name))}{origSchema.Reference.Id}";
                if (context.SchemaRepository.Schemas.TryGetValue(idWithParamNames, out var keyedRequestSchemaRef))
                {
                    mediaType.Schema = keyedRequestSchemaRef;
                    return;
                }

                if (context.SchemaRepository.Schemas.TryGetValue(origSchema.Reference.Id, out var origSchema2))
                {
                    var newSchema = new OpenApiSchema(origSchema2);
                    foreach (var (key, index) in modelMetadata.Parameters.Select((x, i) => (x, i+1)))
                    {
                        var origProp = newSchema.Properties["item" + index];
                        newSchema.Properties.Remove("item" + index);
                        newSchema.Properties.Add(key.Name!, origProp);
                    }
                    var newSchemaRef = context.SchemaRepository.AddDefinition(idWithParamNames, newSchema);
                    mediaType.Schema = newSchemaRef;
                }
            }
        }
    }
}
