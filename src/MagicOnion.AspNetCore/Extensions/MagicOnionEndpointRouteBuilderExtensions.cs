using System.Reflection;
using MagicOnion.AspNetCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class MagicOnionEndpointRouteBuilderExtensions
    {
        public static GrpcServiceEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder)
        {
            var descriptor = builder.ServiceProvider.GetService<MagicOnionServiceDefinitionGlueDescriptor>();

            // builder.MapGrpcService<GlueServiceType>();
            var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService), BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(descriptor.GlueServiceType);

            return (GrpcServiceEndpointConventionBuilder)mapGrpcServiceMethod.Invoke(null, new[] { builder });
        }
    }
}