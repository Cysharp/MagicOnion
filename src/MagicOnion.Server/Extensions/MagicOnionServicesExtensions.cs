using System.Reflection;
using Cysharp.Runtime.Multicast;
using Cysharp.Runtime.Multicast.InMemory;
using Cysharp.Runtime.Multicast.Remoting;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Binder.Internal;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServicesExtensions
{
    public static IMagicOnionServerBuilder AddMagicOnion(this IServiceCollection services, Action<MagicOnionOptions>? configureOptions = null)
        => services.AddMagicOnionCore(configureOptions);

    [Obsolete("Use MapMagicOnionService(Assembly[]) instead.", error: true)]
    public static IMagicOnionServerBuilder AddMagicOnion(this IServiceCollection services, Assembly[] searchAssemblies, Action<MagicOnionOptions>? configureOptions = null)
        => throw new NotSupportedException();

    [Obsolete("Use MapMagicOnionService(Type[]) instead.", error: true)]
    public static IMagicOnionServerBuilder AddMagicOnion(this IServiceCollection services, IEnumerable<Type> searchTypes, Action<MagicOnionOptions>? configureOptions = null)
        => throw new NotSupportedException();

    // NOTE: `internal` is required for unit tests.
    internal static IMagicOnionServerBuilder AddMagicOnionCore(this IServiceCollection services, Action<MagicOnionOptions>? configureOptions = null)
    {
        // Return if the services are already registered.
        if (services.Any(x => x.ServiceType == typeof(MagicOnionServiceMarker)))
        {
            return new MagicOnionServerBuilder(services);
        }

        var configName = Options.Options.DefaultName;

        // Required services (ASP.NET Core, gRPC)
        services.AddLogging();
        services.AddGrpc();
        services.AddMetrics();

        // MagicOnion: Core services
        services.AddSingleton<MagicOnionServiceMarker>();
        services.AddSingleton(typeof(StreamingHubRegistry<>));
        services.AddSingleton(typeof(IServiceMethodProvider<>), typeof(MagicOnionGrpcServiceMethodProvider<>));
        services.TryAddSingleton<IMagicOnionGrpcMethodProvider, DynamicMagicOnionMethodProvider>();

        // MagicOnion: Metrics
        services.TryAddSingleton<MagicOnionMetrics>();

        // MagicOnion: Options
        services.AddOptions<MagicOnionOptions>(configName)
            .Configure<IConfiguration>((o, configuration) =>
            {
                configuration.GetSection(string.IsNullOrWhiteSpace(configName) ? "MagicOnion" : configName).Bind(o);
                configureOptions?.Invoke(o);
            });

        // Add: Multicaster
        services.TryAddSingleton<IInMemoryProxyFactory>(DynamicInMemoryProxyFactory.Instance);
        services.TryAddSingleton<IRemoteProxyFactory>(DynamicRemoteProxyFactory.Instance);
        services.TryAddSingleton<IRemoteSerializer, MagicOnionRemoteSerializer>();
        services.TryAddSingleton<IRemoteClientResultPendingTaskRegistry, RemoteClientResultPendingTaskRegistry>();
        services.TryAddSingleton<IMulticastGroupProvider, RemoteGroupProvider>();
        services.TryAddSingleton<MagicOnionManagedGroupProvider>();


        return new MagicOnionServerBuilder(services);
    }
}
