#nullable enable
using MagicOnion.Server.HttpGateway;
using MagicOnion.Server.HttpGateway.Swagger;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class MagicOnionMiddlewareExtensions
{
    public static IEndpointConventionBuilder MapMagicOnionSwagger(this IEndpointRouteBuilder endpoints, string pattern, IReadOnlyList<MagicOnion.Server.MethodHandler> handlers, string apiBasePath, SwaggerOptions? swaggerOptions = default)
    {
        var d = endpoints.CreateApplicationBuilder()
            .UseMiddleware<MagicOnionSwaggerMiddleware>(handlers, swaggerOptions ?? new SwaggerOptions("MagicOnion", "", apiBasePath))
            .Build();

        return endpoints.Map(pattern + "/{path?}", d);
    }

    public static IEndpointConventionBuilder MapMagicOnionHttpGateway(this IEndpointRouteBuilder endpoints, string pattern, IReadOnlyList<MagicOnion.Server.MethodHandler> handlers, GrpcChannel channel)
    {
        var d = endpoints.CreateApplicationBuilder()
            .UseMiddleware<MagicOnionHttpGatewayMiddleware>(handlers, channel)
            .Build();

        return endpoints.Map(pattern + "/{service}/{method}", d);
    }
}