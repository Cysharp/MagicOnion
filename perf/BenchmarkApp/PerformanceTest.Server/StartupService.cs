using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace PerformanceTest.Server;

class StartupService : IHostedService
{
    readonly IServer server;

    public StartupService(IHostApplicationLifetime applicationLifetime, IServer server)
    {
        this.server = server;
        applicationLifetime.ApplicationStarted.Register(PrintStartupInformation);
    }

    private void PrintStartupInformation()
    {
        Console.WriteLine($"MagicOnion {typeof(MagicOnionEngine).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
        Console.WriteLine($"grpc-dotnet {typeof(Grpc.AspNetCore.Server.GrpcServiceOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
        Console.WriteLine();

        Console.WriteLine($"Listening on:");
        Console.WriteLine(string.Join(", ", server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Array.Empty<string>()));
        Console.WriteLine();

        Console.WriteLine("Configurations:");
        #if RELEASE
        Console.WriteLine($"Build Configuration: Release");
        #else
        Console.WriteLine($"Build Configuration: Debug");
        #endif
        Console.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"{nameof(RuntimeInformation.OSDescription)}: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"{nameof(RuntimeInformation.OSArchitecture)}: {RuntimeInformation.OSArchitecture}");
        Console.WriteLine($"{nameof(RuntimeInformation.ProcessArchitecture)}: {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"{nameof(GCSettings.IsServerGC)}: {GCSettings.IsServerGC}");
        Console.WriteLine($"{nameof(Environment.ProcessorCount)}: {Environment.ProcessorCount}");
        Console.WriteLine();

        Console.WriteLine("Application started.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}