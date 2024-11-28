using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Diagnostics;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingGrpcServiceMethodProvider<TServiceImplementation>(
    IEnumerable<IMagicOnionGrpcMethodProvider> methodProviders,
    IGrpcServiceActivator<TServiceImplementation> serviceActivator,
    IOptions<JsonOptions> jsonOptions,
    IOptions<MagicOnionJsonTranscodingOptions> transcodingOptions,
    IOptions<MagicOnionOptions> options,
    IServiceProvider serviceProvider,
    ILogger<MagicOnionJsonTranscodingGrpcServiceMethodProvider<TServiceImplementation>> logger,
    ILoggerFactory loggerFactory
)
    : IServiceMethodProvider<TServiceImplementation>
    where TServiceImplementation : class
{
    readonly IMagicOnionGrpcMethodProvider[] methodProviders = methodProviders.ToArray();
    readonly JsonOptions jsonOptions = jsonOptions.Value;
    readonly MagicOnionJsonTranscodingOptions transcodingOptions = transcodingOptions.Value;
    readonly MagicOnionOptions options = options.Value;
    readonly ILogger logger = logger;

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TServiceImplementation> context)
    {
        if (!typeof(TServiceImplementation).IsAssignableTo(typeof(IServiceMarker))) return;
        var binder = new MagicOnionJsonTranscodingGrpcMethodBinder<TServiceImplementation>(context, serviceActivator, jsonOptions, transcodingOptions, options, serviceProvider, loggerFactory);

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


