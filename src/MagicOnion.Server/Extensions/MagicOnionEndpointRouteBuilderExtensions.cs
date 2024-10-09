using MagicOnion.Server.Binder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class MagicOnionEndpointRouteBuilderExtensions
{
    public static GrpcServiceEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder)
    {
        var context = new MagicOnionGrpcServiceRegistrationContext(builder);
        foreach (var methodProvider in builder.ServiceProvider.GetRequiredService<IEnumerable<IMagicOnionGrpcMethodProvider>>())
        {
            methodProvider.OnRegisterGrpcServices(context);
        }

        return builder.MapGrpcService<MagicOnionService>();
    }
}
