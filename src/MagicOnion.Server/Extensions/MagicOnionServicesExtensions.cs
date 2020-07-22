using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server.Glue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Extensions
{
    public static class MagicOnionServicesExtensions
    {
        public static void AddMagicOnion(this IServiceCollection services, Action<MagicOnionOptions>? configureOptions = null)
        {
            var configName = "MagicOnion";
            var glueServiceType = MagicOnionGlueService.CreateType();

            services.AddSingleton<MagicOnionServiceDefinition>(sp => MagicOnionEngine.BuildServerServiceDefinition(sp, sp.GetRequiredService<IOptionsMonitor<MagicOnionOptions>>().Get(configName)));
            services.AddSingleton<MagicOnionServiceDefinitionGlueDescriptor>(sp => new MagicOnionServiceDefinitionGlueDescriptor(glueServiceType, sp.GetRequiredService<MagicOnionServiceDefinition>()));
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>).MakeGenericType(glueServiceType), typeof(MagicOnionGlueServiceMethodProvider<>).MakeGenericType(glueServiceType)));
            services.TryAddSingleton<IMagicOnionLogger>(new NullMagicOnionLogger());

            services.AddOptions<MagicOnionOptions>(configName)
                .Configure<IConfiguration>((o, configuration) =>
                {
                    configuration.GetSection(configName).Bind(o);
                    configureOptions?.Invoke(o);
                });
        }
    }
}
