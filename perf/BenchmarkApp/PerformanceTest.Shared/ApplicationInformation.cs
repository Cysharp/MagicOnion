using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using MessagePack;

namespace PerformanceTest.Shared;

public class ApplicationInformation
{
    public static ApplicationInformation Current { get; } = new ApplicationInformation();

#if SERVER
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.Server.MagicOnionEngine).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = typeof(Grpc.AspNetCore.Server.GrpcServiceOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#elif CLIENT
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.Client.MagicOnionClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = typeof(Grpc.Net.Client.GrpcChannel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#else
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.UnaryResult).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = default;
#endif
    public string? MessagePackVersion { get; } = typeof(MessagePack.MessagePackSerializer).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    public bool IsReleaseBuild { get; }
#if RELEASE
        = true;
#else
        = false;
#endif

    public string FrameworkDescription { get; } = RuntimeInformation.FrameworkDescription;
    public string OSDescription { get; } = RuntimeInformation.OSDescription;
    public Architecture OSArchitecture { get; } = RuntimeInformation.OSArchitecture;
    public Architecture ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture;
    public bool IsServerGC { get; } = GCSettings.IsServerGC;
    public int ProcessorCount { get; } = Environment.ProcessorCount;
    public bool IsAttached { get; } = Debugger.IsAttached;
}