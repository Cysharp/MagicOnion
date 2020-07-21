using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server.Glue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MagicOnion.Server.Extensions
{
    public static class MagicOnionServicesExtensions
    {
        public static void AddMagicOnion(this IServiceCollection services)
        {
            var glueServiceType = MagicOnionGlueService.CreateType();
            var methodCreateServiceMethodProviderServiceDescriptor = typeof(MagicOnionServicesExtensions).GetMethod(nameof(CreateServiceMethodProviderServiceDescriptor), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(glueServiceType);
            var serviceMethodProviderDescriptor = (ServiceDescriptor)methodCreateServiceMethodProviderServiceDescriptor.Invoke(null, Array.Empty<object>())!;

            services.AddSingleton<MagicOnionServiceDefinition>(sp => MagicOnionEngine.BuildServerServiceDefinition(sp));
            services.AddSingleton<MagicOnionServiceDefinitionGlueDescriptor>(sp => new MagicOnionServiceDefinitionGlueDescriptor(glueServiceType, sp.GetRequiredService<MagicOnionServiceDefinition>()));
            services.TryAddEnumerable(serviceMethodProviderDescriptor);
            services.TryAddSingleton<IMagicOnionLogger>(new NullMagicOnionLogger());
        }

        private static ServiceDescriptor CreateServiceMethodProviderServiceDescriptor<T>()
            where T : class
        {
            return ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<T>), typeof(MagicOnionGlueServiceMethodProvider<T>));
        }
    }
}
