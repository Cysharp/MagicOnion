using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MagicOnion.Server.JsonTranscoding.Swagger;

internal class MagicOnionXmlCommentsOperationFilter(XDocument xmlDoc) : IOperationFilter
{
    readonly Dictionary<string, XElement> memberElementByName = xmlDoc
        .Descendants("member")
        .Select(x => (Name: x.Attribute("name")?.Value ?? string.Empty, Element: x))
        .Where(x => !string.IsNullOrWhiteSpace(x.Name))
        .ToDictionary(x => Regex.Replace(x.Name, "\\(.*$", ""), x => x.Element);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<MagicOnionJsonTranscodingMetadata>().FirstOrDefault();
        if (metadata is null) return;

        if (!memberElementByName.TryGetValue($"M:{metadata.Method.Metadata.ServiceInterface.FullName}.{metadata.Method.MethodName}", out var memberE)) return;

        var summaryE = memberE.Element("summary");
        if (summaryE is not null)
        {
            operation.Summary = summaryE.Value;
        }

        // ResponseType or DynamicArgumentTuple
        if (operation.RequestBody.Content["application/json"].Schema.Reference?.Id is { } schemaRefId)
        {
            var bodySchema = context.SchemaRepository.Schemas[schemaRefId];
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
