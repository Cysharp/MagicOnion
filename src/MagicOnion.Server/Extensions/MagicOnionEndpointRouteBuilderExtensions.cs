using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MagicOnion.Server.Glue;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class MagicOnionEndpointRouteBuilderExtensions
    {
        public static GrpcServiceEndpointConventionBuilder MapMagicOnionService(this IEndpointRouteBuilder builder)
        {
            var descriptor = builder.ServiceProvider.GetRequiredService<MagicOnionServiceDefinitionGlueDescriptor>();

            // builder.MapGrpcService<GlueServiceType>();
            var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService), BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(descriptor.GlueServiceType);

            return (GrpcServiceEndpointConventionBuilder)mapGrpcServiceMethod.Invoke(null, new[] { builder })!;
        }
    }
}
