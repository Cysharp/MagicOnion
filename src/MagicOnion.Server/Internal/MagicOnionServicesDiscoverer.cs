using System.Reflection;

namespace MagicOnion.Server.Internal;

internal static class MagicOnionServicesDiscoverer
{
    static readonly string[] wellKnownIgnoreAssemblies =
    [
        "Anonymously Hosted DynamicMethods Assembly",
        "netstandard",
        "mscorlib",
        "NuGet.*",
        "System.*",
        "Microsoft.AspNetCore.*",
        "Microsoft.CSharp.*",
        "Microsoft.CodeAnalysis.*",
        "Microsoft.Extensions.*",
        "Microsoft.Identity.*",
        "Microsoft.IdentityModel.*",
        "Microsoft.Net.*",
        "Microsoft.VisualStudio.*",
        "Microsoft.WebTools.*",
        "Microsoft.Win32.*",
        // Popular 3rd-party libraries
        "Newtonsoft.Json",
        "Pipelines.Sockets.Unofficial",
        "Polly.*",
        "StackExchange.Redis.*",
        "StatsdClient",
        // AWS
        "AWSSDK.*",
        // Azure
        "Azure.*",
        "Microsoft.Azure.*",
        // gRPC
        "Grpc.*",
        "Google.Protobuf.*",
        // WPF
        "Accessibility",
        "PresentationFramework",
        "PresentationCore",
        "WindowsBase",
        // MessagePack, MemoryPack
        "MessagePack.*",
        "MemoryPack.*",
        // Multicaster
        "Cysharp.Runtime.Multicaster",
        // MagicOnion
        "MagicOnion.Server.*",
        "MagicOnion.Client.*", // MagicOnion.Client.DynamicClient (MagicOnionClient.Create<T>)
        "MagicOnion.Abstractions",
        "MagicOnion.Shared",
        // Cysharp libraries
        "Cysharp.Threading.LogicLooper",
        "MasterMemory.*",
        "MessagePipe.*",
        "Ulid",
        "ZString",
        "ZLogger",
    ];


    public static IEnumerable<Assembly> GetSearchAssemblies()
    {
        // NOTE: Exclude well-known system assemblies from automatic discovery of services.
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !ShouldIgnoreAssembly(x.GetName().Name!))
            .ToArray();
    }

    public static IEnumerable<Type> GetTypesFromAssemblies(IEnumerable<Assembly> searchAssemblies)
    {
        return searchAssemblies
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
    }

    static bool ShouldIgnoreAssembly(string name)
    {
        return wellKnownIgnoreAssemblies.Any(y =>
        {
            if (y.EndsWith(".*"))
            {
                return name.StartsWith(y.Substring(0, y.Length - 1), StringComparison.OrdinalIgnoreCase) || // Starts with 'MagicOnion.Client.'
                       name.Equals(y.Substring(0, y.Length - 2), StringComparison.OrdinalIgnoreCase); // Exact match 'MagicOnion.Client' (w/o last dot)
            }
            else
            {
                return name == y;
            }
        });
    }
}
