using System.Xml.Linq;
using MagicOnion.Server.JsonTranscoding.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionSwaggerGenOptionsExtensions
{
    public static void IncludeMagicOnionXmlComments(this SwaggerGenOptions options, string path)
        => IncludeMagicOnionXmlComments(options, XDocument.Load(path));

    public static void IncludeMagicOnionXmlComments(this SwaggerGenOptions options, XDocument xmlDoc)
    {
        options.AddOperationFilterInstance(new MagicOnionXmlCommentsOperationFilter(xmlDoc));
    }
}
