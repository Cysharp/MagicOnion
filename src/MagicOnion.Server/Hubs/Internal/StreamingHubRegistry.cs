using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Cysharp.Runtime.Multicast;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Features.Internal;
using MagicOnion.Server.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Hubs.Internal;

internal class StreamingHubRegistry<TService> : IStreamingHubFeature
{
    readonly MagicOnionOptions options;
    readonly IServiceProvider serviceProvider;
    readonly MagicOnionManagedGroupProvider groupProvider;
    readonly IStreamingHubHeartbeatManager heartbeatManager;
    readonly ILogger logger;

    UniqueHashDictionary<StreamingHubHandler>? methodsById;

    public MagicOnionManagedGroupProvider GroupProvider => groupProvider;
    public IStreamingHubHeartbeatManager HeartbeatManager => heartbeatManager;
    public UniqueHashDictionary<StreamingHubHandler> Handlers => methodsById!;

    public StreamingHubRegistry(IOptions<MagicOnionOptions> options, IServiceProvider serviceProvider, ILogger<StreamingHubRegistry<TService>> logger)
    {
        this.options = options.Value;
        this.serviceProvider = serviceProvider;
        this.groupProvider = new MagicOnionManagedGroupProvider(CreateGroupProvider(serviceProvider));
        this.heartbeatManager = CreateHeartbeatManager(options.Value, typeof(TService), serviceProvider);
        this.logger = logger;
    }

    public void RegisterMethods(IEnumerable<IMagicOnionStreamingHubMethod> methods)
    {
        var streamingHubHandlerOptions = new StreamingHubHandlerOptions(options);
        var methodAndIdPairs = methods
            .Select(x => new StreamingHubHandler(x, streamingHubHandlerOptions, serviceProvider))
            .Select(x => (x.MethodId, x))
            .ToArray();

        methodsById = new UniqueHashDictionary<StreamingHubHandler>(methodAndIdPairs);

        foreach (var (_, method) in methodAndIdPairs)
        {
            MagicOnionServerLog.AddStreamingHubMethod(logger, method.HubName, method.MethodInfo.Name, method.MethodId);
        }
    }

    public bool TryGetMethod(int methodId, [NotNullWhen(true)] out StreamingHubHandler? handler)
    {
        return methodsById!.TryGetValue(methodId, out handler);
    }

    static IMulticastGroupProvider CreateGroupProvider(IServiceProvider serviceProvider)
    {
        // Group Provider
        var attr = typeof(TService).GetCustomAttribute<GroupConfigurationAttribute>(true);
        if (attr != null)
        {
            return (IMulticastGroupProvider)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, attr.FactoryType);
        }
        else
        {
            return serviceProvider.GetRequiredService<IMulticastGroupProvider>();
        }
    }

    static IStreamingHubHeartbeatManager CreateHeartbeatManager(MagicOnionOptions options, Type classType, IServiceProvider serviceProvider)
    {
        var heartbeatEnable = options.EnableStreamingHubHeartbeat;
        var heartbeatInterval = options.StreamingHubHeartbeatInterval;
        var heartbeatTimeout = options.StreamingHubHeartbeatTimeout;
        var heartbeatMetadataProvider = default(IStreamingHubHeartbeatMetadataProvider);
        if (classType.GetCustomAttribute<HeartbeatAttribute>(inherit: true) is { } heartbeatAttr)
        {
            heartbeatEnable = heartbeatAttr.Enable;
            if (heartbeatAttr.Timeout != 0)
            {
                heartbeatTimeout = TimeSpan.FromMilliseconds(heartbeatAttr.Timeout);
            }
            if (heartbeatAttr.Interval != 0)
            {
                heartbeatInterval = TimeSpan.FromMilliseconds(heartbeatAttr.Interval);
            }
            if (heartbeatAttr.MetadataProvider != null)
            {
                heartbeatMetadataProvider = (IStreamingHubHeartbeatMetadataProvider)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, heartbeatAttr.MetadataProvider);
            }
        }

        IStreamingHubHeartbeatManager heartbeatManager;
        if (!heartbeatEnable || heartbeatInterval is null)
        {
            heartbeatManager = NopStreamingHubHeartbeatManager.Instance;
        }
        else
        {
            heartbeatManager = new StreamingHubHeartbeatManager(
                heartbeatInterval.Value,
                heartbeatTimeout ?? Timeout.InfiniteTimeSpan,
                heartbeatMetadataProvider ?? serviceProvider.GetService<IStreamingHubHeartbeatMetadataProvider>(),
                options.TimeProvider ?? TimeProvider.System,
                serviceProvider.GetRequiredService<ILogger<StreamingHubHeartbeatManager>>()
            );
        }

        return heartbeatManager;
    }
}
