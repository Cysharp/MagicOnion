using MagicOnion.Server.Hubs;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Cysharp.Runtime.Multicast;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using MagicOnion.Server.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace MagicOnion.Server;

public static class MagicOnionEngine
{
    const string LoggerNameMagicOnionEngine = "MagicOnion.Server.MagicOnionEngine";
    const string LoggerNameMethodHandler = "MagicOnion.Server.MethodHandler";

    static readonly string[] wellKnownIgnoreAssemblies = new[]
    {
        "netstandard",
        "mscorlib",
        "Microsoft.AspNetCore.*",
        "Microsoft.CSharp.*",
        "Microsoft.CodeAnalysis.*",
        "Microsoft.Extensions.*",
        "Microsoft.Win32.*",
        "NuGet.*",
        "System.*",
        "Newtonsoft.Json",
        "Microsoft.Identity.*",
        "Microsoft.IdentityModel.*",
        "StackExchange.Redis.*",
        // gRPC
        "Grpc.*",
        // WPF
        "Accessibility",
        "PresentationFramework",
        "PresentationCore",
        "WindowsBase",
        // MessagePack, MemoryPack
        "MessagePack.*",
        "MemoryPack.*",
        // MagicOnion
        "MagicOnion.Server.*",
        "MagicOnion.Client.*", // MagicOnion.Client.DynamicClient (MagicOnionClient.Create<T>)
        "MagicOnion.Abstractions",
        "MagicOnion.Shared",
    };

    /// <summary>
    /// Search MagicOnion service from all assemblies.
    /// </summary>
    /// <param name="serviceProvider">The service provider is used to resolve dependencies</param>
    /// <param name="isReturnExceptionStackTraceInErrorDetail">If true, when method body throws exception send to client exception.ToString message. It is useful for debugging.</param>
    /// <returns></returns>
    public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, bool isReturnExceptionStackTraceInErrorDetail = false)
    {
        return BuildServerServiceDefinition(serviceProvider, new MagicOnionOptions() { IsReturnExceptionStackTraceInErrorDetail = isReturnExceptionStackTraceInErrorDetail });
    }

    /// <summary>
    /// Search MagicOnion service from all assemblies.
    /// </summary>
    /// <param name="serviceProvider">The service provider is used to resolve dependencies</param>
    /// <param name="options">The options for MagicOnion server</param>
    public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, MagicOnionOptions options)
    {
        // NOTE: Exclude well-known system assemblies from automatic discovery of services.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !ShouldIgnoreAssembly(x.GetName().Name!))
            .ToArray();

        return BuildServerServiceDefinition(serviceProvider, assemblies, options);
    }

    /// <summary>
    /// Search MagicOnion service from target assemblies. ex: new[]{ typeof(Startup).GetTypeInfo().Assembly }
    /// </summary>
    /// <param name="serviceProvider">The service provider is used to resolve dependencies</param>
    /// <param name="searchAssemblies">The assemblies to be search for services</param>
    /// <param name="options">The options for MagicOnion server</param>
    public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, Assembly[] searchAssemblies, MagicOnionOptions options)
    {
        var types = searchAssemblies
            .SelectMany(x =>
            {
                try
                {
                    return x.GetTypes()
                        .Where(x => typeof(IServiceMarker).IsAssignableFrom(x))
                        .Where(x => x.GetCustomAttribute<IgnoreAttribute>(false) == null)
                        .Where(x => x.IsPublic && !x.IsAbstract && !x.IsGenericTypeDefinition);
                }
                catch (ReflectionTypeLoadException)
                {
                    return Array.Empty<Type>();
                }
            });

#pragma warning disable CS8620 // Argument of type cannot be used for parameter of type in due to differences in the nullability of reference types.
        return BuildServerServiceDefinition(serviceProvider, types, options);
#pragma warning restore CS8620 // Argument of type cannot be used for parameter of type in due to differences in the nullability of reference types.
    }

    /// <summary>
    /// Search MagicOnion service from target types.
    /// </summary>
    /// <param name="serviceProvider">The service provider is used to resolve dependencies</param>
    /// <param name="targetTypes">The types to be search for services</param>
    /// <param name="options">The options for MagicOnion server</param>
    public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, IEnumerable<Type> targetTypes, MagicOnionOptions options)
    {
        var handlers = new HashSet<MethodHandler>();
        var streamingHubHandlers = new List<StreamingHubHandler>();

        var methodHandlerOptions = new MethodHandlerOptions(options);
        var streamingHubHandlerOptions = new StreamingHubHandlerOptions(options);

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var loggerMagicOnionEngine = loggerFactory.CreateLogger(LoggerNameMagicOnionEngine);
        var loggerMethodHandler = loggerFactory.CreateLogger(LoggerNameMethodHandler);

        var streamingHubHandlerRepository = serviceProvider.GetRequiredService<StreamingHubHandlerRepository>();

        MagicOnionServerLog.BeginBuildServiceDefinition(loggerMagicOnionEngine);

        var sw = Stopwatch.StartNew();

        try
        {
            foreach (var classType in targetTypes)
            {
                VerifyServiceType(classType);

                var className = classType.Name;
                var isStreamingHub = typeof(IStreamingHubMarker).IsAssignableFrom(classType);
                HashSet<StreamingHubHandler>? tempStreamingHubHandlers = null;
                if (isStreamingHub)
                {
                    tempStreamingHubHandlers = new HashSet<StreamingHubHandler>();
                }

                var inheritInterface = classType.GetInterfaces()
                    .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == (isStreamingHub ? typeof(IStreamingHub<,>) : typeof(IService<>)))
                    .GenericTypeArguments[0];

                if (!inheritInterface.IsAssignableFrom(classType))
                {
                    throw new NotImplementedException($"Type '{classType.FullName}' has no implementation of interface '{inheritInterface.FullName}'.");
                }

                var interfaceMap = classType.GetInterfaceMapWithParents(inheritInterface);

                for (int i = 0; i < interfaceMap.TargetMethods.Length; ++i)
                {
                    var methodInfo = interfaceMap.TargetMethods[i];
                    var methodName = interfaceMap.InterfaceMethods[i].Name;

                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue;
                    if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) continue; // ignore

                    // ignore default methods
                    if (methodName == "Equals"
                        || methodName == "GetHashCode"
                        || methodName == "GetType"
                        || methodName == "ToString"
                        || methodName == "WithOptions"
                        || methodName == "WithHeaders"
                        || methodName == "WithDeadline"
                        || methodName == "WithCancellationToken"
                        || methodName == "WithHost"
                       )
                    {
                        continue;
                    }

                    // register for StreamingHub
                    if (isStreamingHub && methodName != "Connect")
                    {
                        var streamingHandler = new StreamingHubHandler(classType, methodInfo, streamingHubHandlerOptions, serviceProvider);
                        if (!tempStreamingHubHandlers!.Add(streamingHandler))
                        {
                            throw new InvalidOperationException($"Method does not allow overload, {className}.{methodName}");
                        }
                        continue;
                    }
                    else
                    {
                        // create handler
                        var handler = new MethodHandler(classType, methodInfo, methodName, methodHandlerOptions, serviceProvider, loggerMethodHandler, isStreamingHub: false);
                        if (!handlers.Add(handler))
                        {
                            throw new InvalidOperationException($"Method does not allow overload, {className}.{methodName}");
                        }
                    }
                }

                if (isStreamingHub)
                {
                    var connectHandler = new MethodHandler(classType, classType.GetMethod("Connect", BindingFlags.NonPublic | BindingFlags.Instance)!, "Connect", methodHandlerOptions, serviceProvider, loggerMethodHandler, isStreamingHub: true);
                    if (!handlers.Add(connectHandler))
                    {
                        throw new InvalidOperationException($"Method does not allow overload, {className}.Connect");
                    }

                    streamingHubHandlers.AddRange(tempStreamingHubHandlers!);
                    streamingHubHandlerRepository.RegisterHandler(connectHandler, tempStreamingHubHandlers!.ToArray());

                    // Group Provider
                    IMulticastGroupProvider groupProvider;
                    var attr = classType.GetCustomAttribute<GroupConfigurationAttribute>(true);
                    if (attr != null)
                    {
                        groupProvider = (IMulticastGroupProvider)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, attr.FactoryType);
                    }
                    else
                    {
                        groupProvider = serviceProvider.GetRequiredService<IMulticastGroupProvider>();
                    }

                    streamingHubHandlerRepository.RegisterGroupProvider(connectHandler, groupProvider);

                    // Heartbeat
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
                            TimeProvider.System,
                            serviceProvider.GetRequiredService<ILogger<StreamingHubHeartbeatManager>>()
                        );
                    }
                    streamingHubHandlerRepository.RegisterHeartbeatManager(connectHandler, heartbeatManager);
                }
            }
        }
        catch (AggregateException agex)
        {
            ExceptionDispatchInfo.Capture(agex.InnerExceptions[0]).Throw();
        }

        streamingHubHandlerRepository.Freeze();

        var result = new MagicOnionServiceDefinition(handlers.ToArray(), streamingHubHandlers.ToArray());

        sw.Stop();
        MagicOnionServerLog.EndBuildServiceDefinition(loggerMagicOnionEngine, sw.Elapsed.TotalMilliseconds);

        return result;
    }

    internal static void VerifyServiceType(Type type)
    {
        if (!typeof(IServiceMarker).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is not marked as MagicOnion service or hub.");
        }
        if (!type.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(IService<>) || x.GetGenericTypeDefinition() == typeof(IStreamingHub<,>))))
        {
            throw new InvalidOperationException($"Type '{type.FullName}' has no implementation for Service or StreamingHub");
        }
        if (type.IsAbstract)
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is abstract. A service type must be non-abstract class.");
        }
        if (type.IsInterface)
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is interface. A service type must be class.");
        }
        if (type.IsGenericType && type.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is generic type definition. A service type must be plain or constructed-generic class.");
        }
    }

    internal static bool ShouldIgnoreAssembly(string name)
    {
        return wellKnownIgnoreAssemblies.Any(y =>
        {
            if (y.EndsWith(".*"))
            {
                return name.StartsWith(y.Substring(0, y.Length - 1)) || // Starts with 'MagicOnion.Client.'
                       name == y.Substring(0, y.Length - 2); // Exact match 'MagicOnion.Client' (w/o last dot)
            }
            else
            {
                return name == y;
            }
        });
    }
}
