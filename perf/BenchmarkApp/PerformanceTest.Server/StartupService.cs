using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using MagicOnion.Serialization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using PerformanceTest.Shared;

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
        Console.WriteLine($"MagicOnion {ApplicationInformation.Current.MagicOnionVersion}");
        Console.WriteLine($"grpc-dotnet {ApplicationInformation.Current.GrpcNetVersion}");
        Console.WriteLine($"MessagePack {ApplicationInformation.Current.MessagePackVersion}");
        Console.WriteLine($"MemoryPack {ApplicationInformation.Current.MemoryPackVersion}");
        Console.WriteLine();

        Console.WriteLine($"Listening on:");
        Console.WriteLine(string.Join(", ", server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Array.Empty<string>()));
        Console.WriteLine();

        Console.WriteLine("Configurations:");
        Console.WriteLine($"Build Configuration: {(ApplicationInformation.Current.IsReleaseBuild ? "Release" : "Debug")}");
        Console.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {ApplicationInformation.Current.FrameworkDescription}");
        Console.WriteLine($"{nameof(RuntimeInformation.OSDescription)}: {ApplicationInformation.Current.OSDescription}");
        Console.WriteLine($"{nameof(RuntimeInformation.OSArchitecture)}: {ApplicationInformation.Current.OSArchitecture}");
        Console.WriteLine($"{nameof(RuntimeInformation.ProcessArchitecture)}: {ApplicationInformation.Current.ProcessArchitecture}");
        Console.WriteLine($"{nameof(GCSettings.IsServerGC)}: {ApplicationInformation.Current.IsServerGC}");
        Console.WriteLine($"{nameof(Environment.ProcessorCount)}: {ApplicationInformation.Current.ProcessorCount}");
        Console.WriteLine($"{nameof(Debugger)}.{nameof(Debugger.IsAttached)}: {ApplicationInformation.Current.IsAttached}");
        Console.WriteLine($"{nameof(MagicOnionSerializerProvider)}.{nameof(MagicOnionSerializerProvider.Default)}: {MagicOnionSerializerProvider.Default}");
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
