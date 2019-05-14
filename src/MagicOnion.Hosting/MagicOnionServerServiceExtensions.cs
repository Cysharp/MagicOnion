using Grpc.Core;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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
            return hostBuilder.ConfigureServices((ctx, services) =>
            {
                var serviceLocator = new ServiceLocatorBridge(services);
                options.ServiceLocator = serviceLocator; // replace it.

                // build immediately(require register service before create it).
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

                // should transient or singleton?
                // in AddHostedService<T> implementation, singleton is used
                // https://github.com/aspnet/Extensions/blob/8b2482fa68c548e904e4aa1ae38a29c72dcd32a5/src/Hosting/Abstractions/src/ServiceCollectionHostedServiceExtensions.cs#L18
                services.AddSingleton<IHostedService, MagicOnionServerService>(serviceProvider =>
                {
                    serviceLocator.provider = serviceProvider; // set built provider.

                    return new MagicOnionServerService(serviceDefinition, ports, channelOptions);
                });
            });
        }
    }
}
