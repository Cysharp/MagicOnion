using Grpc.Core;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicOnion.Hosting
{
    /// <summary>MagicOnion extensions for Microsoft.Extensions.Hosting classes</summary>
    public static class MagicOnionServerServiceExtensions
    {
        /// <summary>add MagicOnion service to generic host from all assemblies.</summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            ServerPort ports,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            return UseMagicOnion(hostBuilder, new[] { ports }, new MagicOnionOptions(), channelOptions: channelOptions);
        }

        /// <summary>add MagicOnion service to generic host from all assemblies.</summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            MagicOnionOptions options,
            ServerPort ports,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            return UseMagicOnion(hostBuilder, new[] { ports }, options, channelOptions: channelOptions);
        }

        /// <summary>add MagicOnion service to generic host from target assemblies.</summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            Assembly[] searchAssemblies,
            MagicOnionOptions options,
            ServerPort ports,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            return UseMagicOnion(hostBuilder, new[] { ports }, options, searchAssemblies: searchAssemblies, channelOptions: channelOptions);
        }

        /// <summary>add MagicOnion service to generic host from target types.</summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            Type[] targetTypes,
            MagicOnionOptions options,
            ServerPort ports,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            return UseMagicOnion(hostBuilder, new[] { ports }, options, types: targetTypes, channelOptions: channelOptions);
        }

        /// <summary>add MagicOnion service to generic host from all assemblies.</summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            ServerPort ports,
            MagicOnionOptions options,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            return UseMagicOnion(hostBuilder, new[] { ports }, options, channelOptions: channelOptions);
        }

        /// <summary>
        /// <para>add MagicOnion service to generic host from all assemblies.</para>
        /// </summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            IEnumerable<ServerPort> ports,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            return UseMagicOnion(hostBuilder, ports, new MagicOnionOptions(), channelOptions: channelOptions);
        }

        /// <summary>add MagicOnion service to generic host from all assemblies and use configuration to setup.</summary>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            string configurationName = null,
            IEnumerable<Type> types = null,
            Assembly[] searchAssemblies = null
        )
        {
            configurationName = configurationName ?? Options.DefaultName;

            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                // Register a MagicOnion hosted service.
                services.AddSingleton<IHostedService>(serviceProvider =>
                {
                    var hostingOptions = serviceProvider.GetService<IOptionsMonitor<MagicOnionHostingOptions>>().Get(configurationName);
                    var serverPorts = hostingOptions.ServerPorts
                        .Select(x =>
                        {
                            var credentials = x.UseInsecureConnection ? ServerCredentials.Insecure : new SslServerCredentials(x.ServerCredentials.Select(y => y.ToKeyCertificatePair()));
                            return new ServerPort(x.Host, x.Port, credentials);
                        })
                        .ToArray();

                    var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MagicOnionServerService>();
                    var serviceDefinition = serviceProvider.GetServices<MagicOnionHostedServiceDefinition>().Single(x => x.Name == configurationName);

                    return CreateMagicOnionHostedService(logger, serverPorts, serviceDefinition.ServiceDefinition, hostingOptions.ChannelOptions.ToChannelOptions());
                });

                // Register MagicOnionServiceDefinition (when using HttpGateway, it requests MagicOnionServiceDefinition via host's ServiceProvider)
                services.AddMagicOnionHostedServiceDefinition(configurationName, types, searchAssemblies);

                // Options: Hosting startup configuration
                services.AddOptions<MagicOnionHostingOptions>(configurationName)
                    .Bind(hostContext.Configuration.GetSection(string.IsNullOrEmpty(configurationName) ? "MagicOnion" : configurationName))
                    .Configure(options =>
                    {
                        if (!options.ServerPorts.Any())
                        {
                            options.ServerPorts = new[] { new MagicOnionHostingServerPortOptions() };
                        }
                    });
            });
        }

        /// <summary>
        /// <para>add MagicOnion service to generic host with specific types or assemblies</para>
        /// </summary>
        /// <remarks>you must not pass null to options</remarks>
        public static IHostBuilder UseMagicOnion(this IHostBuilder hostBuilder,
            IEnumerable<ServerPort> ports,
            MagicOnionOptions options,
            IEnumerable<Type> types = null,
            Assembly[] searchAssemblies = null,
            IEnumerable<ChannelOption> channelOptions = null)
        {
            if (ports == null) throw new ArgumentNullException(nameof(ports));
            if (options == null) throw new ArgumentNullException(nameof(options));

            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                // Register a MagicOnion hosted service.
                services.AddSingleton<IHostedService>(serviceProvider =>
                {
                    var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MagicOnionServerService>();
                    var serviceDefinition = serviceProvider.GetService<MagicOnionHostedServiceDefinition>();

                    return CreateMagicOnionHostedService(logger, ports, serviceDefinition.ServiceDefinition, channelOptions);
                });

                // Register MagicOnionServiceDefinition (when using HttpGateway, it requests MagicOnionServiceDefinition via host's ServiceProvider)
                services.AddMagicOnionHostedServiceDefinition(Options.DefaultName, types, searchAssemblies);

                // Options: Hosting startup configuration
                services.AddOptions<MagicOnionHostingOptions>(Options.DefaultName)
                    .Configure(hostingOptions =>
                    {
                        hostingOptions.Service = options;
                    });
            });
        }

        private static void AddMagicOnionHostedServiceDefinition(
            this IServiceCollection services,
            string configurationName = null,
            IEnumerable<Type> types = null,
            Assembly[] searchAssemblies = null
        )
        {
            if (services.Any(x => (x.ImplementationInstance as MagicOnionHostedServiceDefinition)?.Name == configurationName))
            {
                throw new InvalidOperationException($"MagicOnion ServiceDefinition for '{configurationName}' is already registered.");
            }

            services.AddSingleton<MagicOnionHostedServiceDefinition>(serviceProvider =>
            {
                var hostingOptions = serviceProvider.GetService<IOptionsMonitor<MagicOnionHostingOptions>>().Get(configurationName);
                var options = hostingOptions.Service;
                options.ServiceLocator = new ServiceLocatorBridge(serviceProvider);
                options.ServiceActivator = new GenericHostServiceActivator(serviceProvider, options);

                // Build a MagicOnion ServiceDefinition from assemblies/types.
                MagicOnionServiceDefinition serviceDefinition;
                if (searchAssemblies != null)
                {
                    serviceDefinition = MagicOnionEngine.BuildServerServiceDefinition(searchAssemblies, options);
                }
                else if (types != null)
                {
                    serviceDefinition = MagicOnionEngine.BuildServerServiceDefinition(types, options);
                }
                else
                {
                    if (options != null)
                    {
                        serviceDefinition = MagicOnionEngine.BuildServerServiceDefinition(options);
                    }
                    else
                    {
                        serviceDefinition = MagicOnionEngine.BuildServerServiceDefinition();
                    }
                }

                return new MagicOnionHostedServiceDefinition(configurationName, serviceDefinition);
            });
        }

        private static MagicOnionServerService CreateMagicOnionHostedService(
            ILogger logger,
            IEnumerable<ServerPort> ports,
            MagicOnionServiceDefinition serviceDefinition,
            IEnumerable<ChannelOption> channelOptions = null
        )
        {
            logger.LogInformation("MagicOnion is listening on: {ServerPorts}", String.Join(",", ports.Select(x => $"{(x.Credentials == ServerCredentials.Insecure ? "http" : "https")}://{x.Host}:{x.Port}")));
            return new MagicOnionServerService(serviceDefinition, ports, channelOptions);
        }
    }
}
