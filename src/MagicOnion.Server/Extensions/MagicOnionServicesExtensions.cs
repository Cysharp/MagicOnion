using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server;
using MagicOnion.Server.Glue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MagicOnionServicesExtensions
    {
        public static void AddMagicOnion(this IServiceCollection services, Action<MagicOnionOptions>? configureOptions = null)
        {
            var configName = Options.Options.DefaultName;
            services.AddSingleton<MagicOnionServiceDefinition>(sp => MagicOnionEngine.BuildServerServiceDefinition(sp, sp.GetRequiredService<IOptionsMonitor<MagicOnionOptions>>().Get(configName)));
            services.AddMagicOnionCore(configureOptions);
        }

        public static void AddMagicOnion(this IServiceCollection services, Assembly[] searchAssemblies, Action<MagicOnionOptions>? configureOptions = null)
        {
            var configName = Options.Options.DefaultName;
            services.AddSingleton<MagicOnionServiceDefinition>(sp => MagicOnionEngine.BuildServerServiceDefinition(sp, searchAssemblies, sp.GetRequiredService<IOptionsMonitor<MagicOnionOptions>>().Get(configName)));
            services.AddMagicOnionCore(configureOptions);
        }

        public static void AddMagicOnion(this IServiceCollection services, IEnumerable<Type> searchTypes, Action<MagicOnionOptions>? configureOptions = null)
        {
            var configName = Options.Options.DefaultName;
            services.AddSingleton<MagicOnionServiceDefinition>(sp => MagicOnionEngine.BuildServerServiceDefinition(sp, searchTypes, sp.GetRequiredService<IOptionsMonitor<MagicOnionOptions>>().Get(configName)));
            services.AddMagicOnionCore(configureOptions);
        }

        private static void AddMagicOnionCore(this IServiceCollection services, Action<MagicOnionOptions>? configureOptions = null)
        {
            var configName = Options.Options.DefaultName;
            var glueServiceType = MagicOnionGlueService.CreateType();

            services.TryAddSingleton<IMagicOnionLogger, MagicOnionLogToLogger>();

            services.AddSingleton<MagicOnionServiceDefinitionGlueDescriptor>(sp => new MagicOnionServiceDefinitionGlueDescriptor(glueServiceType, sp.GetRequiredService<MagicOnionServiceDefinition>()));
            services.AddSingleton(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>).MakeGenericType(glueServiceType), typeof(MagicOnionGlueServiceMethodProvider<>).MakeGenericType(glueServiceType)));

            services.AddOptions<MagicOnionOptions>(configName)
                .Configure<IConfiguration>((o, configuration) =>
                {
                    configuration.GetSection(string.IsNullOrWhiteSpace(configName) ? "MagicOnion" : configName).Bind(o);
                    configureOptions?.Invoke(o);
                });
        }
    }
}
