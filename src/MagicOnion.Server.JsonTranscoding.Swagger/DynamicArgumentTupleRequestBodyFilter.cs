using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MagicOnion.Server.JsonTranscoding.Swagger;

internal class DynamicArgumentTupleRequestBodyFilter : IRequestBodyFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        // Dynamically generate a schema for DynamicArgumentTuple that matches the argument name
        if (requestBody.Content.TryGetValue("application/json", out var mediaType) &&
            context.BodyParameterDescription.ModelMetadata.ModelType is { IsGenericType: true } modelType &&
            modelType.GetGenericTypeDefinition() is { FullName: not null } modelOpenType &&
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
                foreach (var (key, index) in modelMetadata.Parameters.Select((x, i) => (x, i + 1)))
                {
                    var origProp = newSchema.Properties["item" + index];
                    newSchema.Properties.Remove("item" + index);
                    newSchema.Properties.Add(key.Name!, origProp);
                }
                var newSchemaRef = context.SchemaRepository.AddDefinition($"{modelMetadata.ServiceName}{modelMetadata.MethodName}Parameters", newSchema);
                mediaType.Schema = newSchemaRef;
            }
        }
    }
}
