using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MagicOnion.Server.Binder;

public class MagicOnionGrpcServiceRegistrationContext(IEndpointRouteBuilder builder)
{
    public MagicOnionServiceEndpointConventionBuilder Register<T>()
        where T : class, IServiceMarker
    {
        return new MagicOnionServiceEndpointConventionBuilder(builder.MapGrpcService<T>());
    }

    public MagicOnionServiceEndpointConventionBuilder Register(Type t)
    {
        return new MagicOnionServiceEndpointConventionBuilder((GrpcServiceEndpointConventionBuilder)typeof(GrpcEndpointRouteBuilderExtensions)
            .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))!
            .MakeGenericMethod(t)
            .Invoke(null, [builder])!);
    }
}

public interface IMagicOnionGrpcMethodProvider
{
    void OnRegisterGrpcServices(MagicOnionGrpcServiceRegistrationContext context);
    IEnumerable<IMagicOnionGrpcMethod> GetGrpcMethods<TService>() where TService : class;
    IEnumerable<IMagicOnionStreamingHubMethod> GetStreamingHubMethods<TService>() where TService : class;
}
