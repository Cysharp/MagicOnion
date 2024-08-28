using System.Reflection;
using MagicOnion.Server.Glue;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class MagicOnionEndpointRouteBuilderExtensions
{
    public static GrpcServiceEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder)
    {
        return builder.MapGrpcService<MagicOnionService>();
    }
}
