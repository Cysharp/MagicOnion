using MagicOnion.Server.JsonTranscoding.Swagger;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MagicOnion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServerJsonTranscodingSwaggerBuilderExtension
{
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
            if (context.BodyParameterDescription.ModelMetadata.ModelType is { IsGenericType: true } modelType &&
                modelType.GetGenericTypeDefinition() == typeof(DynamicArgumentTuple<,>) &&
                requestBody.Content.TryGetValue("application/json", out var mediaType) &&
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
                _ = mediaType;
            }
        }
    }
}
