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
using Microsoft.Extensions.Options;

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
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var loggerMagicOnionEngine = loggerFactory.CreateLogger(LoggerNameMagicOnionEngine);

        MagicOnionServerLog.BeginBuildServiceDefinition(loggerMagicOnionEngine);

        var sw = Stopwatch.StartNew();

        var result = new MagicOnionServiceDefinition(targetTypes.ToArray());

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
