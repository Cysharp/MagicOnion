using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingGrpcServiceMethodProvider<TServiceImplementation> : IServiceMethodProvider<TServiceImplementation>
    where TServiceImplementation : class
{
    readonly IMagicOnionGrpcMethodProvider[] methodProviders;
    readonly IGrpcServiceActivator<TServiceImplementation> serviceActivator;
    readonly MagicOnionJsonTranscodingOptions transcodingOptions;
    readonly MagicOnionOptions options;
    readonly IServiceProvider serviceProvider;
    readonly ILogger logger;
    readonly ILoggerFactory loggerFactory;

    public MagicOnionJsonTranscodingGrpcServiceMethodProvider(
        IEnumerable<IMagicOnionGrpcMethodProvider> methodProviders,
        IGrpcServiceActivator<TServiceImplementation> serviceActivator,
        IOptions<MagicOnionJsonTranscodingOptions> transcodingOptions,
        IOptions<MagicOnionOptions> options,
        IServiceProvider serviceProvider,
        ILogger<MagicOnionJsonTranscodingGrpcServiceMethodProvider<TServiceImplementation>> logger,
        ILoggerFactory loggerFactory
    )
    {
        this.methodProviders = methodProviders.ToArray();
        this.serviceActivator = serviceActivator;
        this.transcodingOptions = transcodingOptions.Value;
        this.options = options.Value;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.loggerFactory = loggerFactory;
    }

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TServiceImplementation> context)
    {
        if (!typeof(TServiceImplementation).IsAssignableTo(typeof(IServiceMarker))) return;
        var binder = new MagicOnionJsonTranscodingGrpcMethodBinder<TServiceImplementation>(context, serviceActivator, transcodingOptions, options, serviceProvider, loggerFactory);

        var registered = false;
        foreach (var methodProvider in methodProviders.OrderBy(x => x.GetType().Name == "DynamicMagicOnionMethodProvider" ? 1 : 0)) // DynamicMagicOnionMethodProvider is always last.
        {
            if (typeof(TServiceImplementation).IsAssignableTo(typeof(IStreamingHubMarker)))
            {
                // Skip StreamingHub
                continue;
            }

            foreach (var method in methodProvider.GetGrpcMethods<TServiceImplementation>())
            {
                ((IMagicOnionGrpcMethod<TServiceImplementation>)method).Bind(binder);
                registered = true;
            }

            if (registered)
            {
                MagicOnionServerLog.ServiceMethodDiscovered(logger, typeof(TServiceImplementation).Name, methodProvider.GetType().FullName!);
                return;
            }
        }

        if (!registered)
        {
            MagicOnionServerLog.ServiceMethodNotDiscovered(logger, typeof(TServiceImplementation).Name);
        }
    }
}


