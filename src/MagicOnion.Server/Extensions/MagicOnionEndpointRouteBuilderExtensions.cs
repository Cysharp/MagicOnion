using MagicOnion.Server.Binder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class MagicOnionEndpointRouteBuilderExtensions
{
    public static GrpcServiceEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder)
    {
        return builder.MapGrpcService<MagicOnionService>();
    }
}
