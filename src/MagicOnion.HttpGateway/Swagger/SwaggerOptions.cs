using Microsoft.AspNetCore.Http;
using System;

namespace MagicOnion.HttpGateway.Swagger
{
    public class SwaggerOptions
    {
        public string ApiBasePath { get; private set; }
        public Info Info { get; set; }

        /// <summary>
        /// (FilePath, LoadedEmbeddedBytes) => CustomBytes)
        /// </summary>
        public Func<string, byte[], byte[]> ResolveCustomResource { get; set; }
        public Func<HttpContext, string> CustomHost { get; set; }
        public string XmlDocumentPath { get; set; }
        public string JsonName { get; set; }

        public SwaggerOptions(string title, string description, string apiBasePath)
        {
            ApiBasePath = apiBasePath;
            JsonName = "api-default.json";
            Info = new Info { description = description, title = title };
        }
    }
}