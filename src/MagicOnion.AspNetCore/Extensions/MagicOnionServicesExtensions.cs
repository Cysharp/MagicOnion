using System;
using System.Collections.Generic;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.AspNetCore;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MagicOnionServicesExtensions
    {
        public static void AddMagicOnion(this IServiceCollection services)
        {
            var serviceDefinition = MagicOnionEngine.BuildServerServiceDefinition();
            var glueServiceType = MagicOnionGlueService.CreateType();

            var serviceMethodProvider = Activator.CreateInstance(typeof(MagicOnionGlueServiceMethodProvider<>).MakeGenericType(glueServiceType), serviceDefinition.ServerServiceDefinition);

            services.Add(ServiceDescriptor.Singleton(new MagicOnionServiceDefinitionGlueDescriptor(glueServiceType, serviceDefinition)));
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>).MakeGenericType(glueServiceType), serviceMethodProvider));
        }
    }
}