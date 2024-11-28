using System.Xml.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MagicOnion.Server.JsonTranscoding.Swagger;

internal class MagicOnionXmlCommentsOperationFilter(XDocument xmlDoc) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<MagicOnionJsonTranscodingMetadata>().FirstOrDefault();
        if (metadata is null) return;

        var memberE = xmlDoc
            .Descendants("member")
            .FirstOrDefault(x => x.Attribute("name")?.Value.StartsWith($"M:{metadata.Method.Metadata.ServiceInterface.FullName}.{metadata.Method.MethodName}") ?? false);
        if (memberE is null) return;

        var summaryE = memberE.Element("summary");
        if (summaryE is not null)
        {
            operation.Summary = summaryE.Value;
        }

        if (metadata.Method.Metadata.Parameters.Any())
        {
            // DynamicArgumentTuple
            var bodySchema = context.SchemaRepository.Schemas[operation.RequestBody.Content["application/json"].Schema.Reference.Id];
            var paramByName = memberE.Elements("param").ToDictionary(x => x.Attribute("name")?.Value ?? "-", x => x.Value);
            foreach (var param in bodySchema.Properties)
            {
                if (paramByName.TryGetValue(param.Key, out var desc))
                {
                    param.Value.Description = desc;
                }
            }
        }
    }
}
