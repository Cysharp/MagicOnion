using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionGrpcMethodProvider
{
    void MapAllSupportedServiceTypes(MagicOnionGrpcServiceMappingContext context);
    IEnumerable<IMagicOnionGrpcMethod> GetGrpcMethods<TService>() where TService : class;
    IEnumerable<IMagicOnionStreamingHubMethod> GetStreamingHubMethods<TService>() where TService : class;
}

public class MagicOnionGrpcServiceMappingContext(IEndpointRouteBuilder builder) : IEndpointConventionBuilder
{
    readonly List<MagicOnionServiceEndpointConventionBuilder> innerBuilders = new();

    public void Map<T>()
        where T : class, IServiceMarker
    {
        innerBuilders.Add(new MagicOnionServiceEndpointConventionBuilder(builder.MapGrpcService<T>()));
    }

    public void Map(Type t)
    {
        VerifyServiceType(t);

        innerBuilders.Add(new MagicOnionServiceEndpointConventionBuilder((GrpcServiceEndpointConventionBuilder)typeof(GrpcEndpointRouteBuilderExtensions)
            .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))!
            .MakeGenericMethod(t)
            .Invoke(null, [builder])!));
    }

    static void VerifyServiceType(Type type)
    {
        if (!typeof(IServiceMarker).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is not marked as MagicOnion service or hub.");
        }
        if (!type.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(IService<>) || x.GetGenericTypeDefinition() == typeof(IStreamingHub<,>))))
        {
            throw new InvalidOperationException($"Type '{type.FullName}' has no implementation for Service or StreamingHub");
        }
        if (type.IsAbstract)
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is abstract. A service type must be non-abstract class.");
        }
        if (type.IsInterface)
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is interface. A service type must be class.");
        }
        if (type.IsGenericType && type.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is generic type definition. A service type must be plain or constructed-generic class.");
        }
    }

    void IEndpointConventionBuilder.Add(Action<EndpointBuilder> convention)
    {
        foreach (var innerBuilder in innerBuilders)
        {
            innerBuilder.Add(convention);
        }
    }
}
