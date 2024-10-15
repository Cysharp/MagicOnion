using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Binder.Internal;

internal class MagicOnionGrpcServiceMethodProvider<TService> : IServiceMethodProvider<TService>
    where TService : class
{
    readonly IMagicOnionGrpcMethodProvider[] methodProviders;
    readonly MagicOnionOptions options;
    readonly IServiceProvider serviceProvider;
    readonly ILoggerFactory loggerFactory;
    readonly ILogger logger;

    public MagicOnionGrpcServiceMethodProvider(IEnumerable<IMagicOnionGrpcMethodProvider> methodProviders, IOptions<MagicOnionOptions> options, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ILogger<MagicOnionGrpcServiceMethodProvider<TService>> logger)
    {
        this.methodProviders = methodProviders.ToArray();
        this.options = options.Value;
        this.serviceProvider = serviceProvider;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
    {
        if (!typeof(TService).IsAssignableTo(typeof(IServiceMarker))) return;

        var binder = new MagicOnionGrpcMethodBinder<TService>(context, options, loggerFactory, serviceProvider);

        var registered = false;
        foreach (var methodProvider in methodProviders.OrderBy(x => x is DynamicMagicOnionMethodProvider ? 1 : 0)) // DynamicMagicOnionMethodProvider is always last.
        {
            foreach (var method in methodProvider.GetGrpcMethods<TService>())
            {
                ((IMagicOnionGrpcMethod<TService>)method).Bind(binder);
                registered = true;
            }

            if (typeof(TService).IsAssignableTo(typeof(IStreamingHubMarker)))
            {
                var registry = serviceProvider.GetRequiredService<StreamingHubRegistry<TService>>();
                registry.RegisterMethods(methodProvider.GetStreamingHubMethods<TService>());
            }

            if (registered)
            {
                MagicOnionServerLog.ServiceMethodDiscovered(logger, typeof(TService).Name, methodProvider.GetType().FullName!);
                return;
            }
        }

        if (!registered)
        {
            MagicOnionServerLog.ServiceMethodNotDiscovered(logger, typeof(TService).Name);
        }
    }
}
