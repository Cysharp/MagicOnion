using MagicOnion.Server.JsonTranscoding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public static class SwaggerJwtExtention
{
    public static void AddJwtSecurityScheme(this SwaggerGenOptions options)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the token with the `Bearer ` prefix, e.g. \"Bearer abcde12345\"",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        options.AddSecurityDefinition("Bearer", securityScheme);
        options.OperationFilter<SecurityFilter>();
    }

    private class SecurityFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.ApiDescription.ActionDescriptor
                .EndpointMetadata.Any(em => em is MagicOnionJsonTranscodingMetadata m && m.Method.Metadata.Metadata.Any(a => a is AuthorizeAttribute));
            
            if (hasAuthorize)
            {
                // Add security definition to endpoints that require JWT authentication
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    }
                };
            }
        }
    }
}