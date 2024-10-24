using System.Reflection;
using MagicOnion;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class MagicOnionEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps MagicOnion Unary and StreamingHub services in the loaded assemblies to the route builder.
    /// </summary>
    /// <param name="builder"></param>
    public static IEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder)
    {
        ThrowIfMagicOnionServicesNotRegistered(builder);

        var context = new MagicOnionGrpcServiceMappingContext(builder);
        foreach (var methodProvider in builder.ServiceProvider.GetServices<IMagicOnionGrpcMethodProvider>())
        {
            methodProvider.MapAllSupportedServiceTypes(context);
        }

        return context;
    }

    /// <summary>
    /// Maps specified type as a MagicOnion Unary or StreamingHub service to the route builder.
    /// </summary>
    /// <param name="builder"></param>
    public static IEndpointConventionBuilder MapMagicOnionService<T>(this IEndpointRouteBuilder builder)
        where T : class, IServiceMarker
    {
        ThrowIfMagicOnionServicesNotRegistered(builder);

        var context = new MagicOnionGrpcServiceMappingContext(builder);
        context.Map<T>();

        return context;
    }

    /// <summary>
    /// Maps specified types as MagicOnion Unary and StreamingHub services to the route builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="serviceTypes"></param>
    public static IEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder, params Type[] serviceTypes)
    {
        ThrowIfMagicOnionServicesNotRegistered(builder);

        var context = new MagicOnionGrpcServiceMappingContext(builder);
        foreach (var t in serviceTypes)
        {
            context.Map(t);
        }

        return context;
    }

    /// <summary>
    /// Maps MagicOnion Unary and StreamingHub services in the target assemblies to the route builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="searchAssemblies"></param>
    public static IEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder, params Assembly[] searchAssemblies)
    {
        ThrowIfMagicOnionServicesNotRegistered(builder);

        var context = new MagicOnionGrpcServiceMappingContext(builder);
        foreach (var t in MagicOnionServicesDiscoverer.GetTypesFromAssemblies(searchAssemblies))
        {
            context.Map(t);
        }

        return context;
    }

    static void ThrowIfMagicOnionServicesNotRegistered(IEndpointRouteBuilder builder)
    {
        if (builder.ServiceProvider.GetService<MagicOnionServiceMarker>() is null)
        {
            throw new InvalidOperationException("AddMagicOnion must be called to register the services before route mapping.");
        }
    }
}
