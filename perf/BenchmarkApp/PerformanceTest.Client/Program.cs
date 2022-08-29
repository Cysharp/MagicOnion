using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;

var app = ConsoleApp.Create(args);
app.AddRootCommand(Main);
app.Run();

async Task Main(
    [Option("s")]ScenarioType scenario,
    [Option("u")]string url,
    [Option("w")]int warmup = 10,
    [Option("d")]int duration = 10,
    [Option("t")]int streams = 10,
    [Option("c")]int channels = 10,
    [Option("v")]bool verbose = false
)
{
    var config = new ScenarioConfiguration(url, warmup, duration, streams, channels, verbose);

    PrintStartupInformation();

    WriteLog($"Scenario: {scenario}");
    WriteLog($"Url: {config.Url}");
    WriteLog($"Warmup: {config.Warmup} s");
    WriteLog($"Duration: {config.Duration} s");
    WriteLog($"Streams: {config.Streams}");
    WriteLog($"Channels: {config.Channels}");

    Func<IScenario> scenarioFactory = scenario switch
    {
        ScenarioType.Unary => () => new UnaryScenario(),
        ScenarioType.UnaryLargePayload1K => () => new UnaryLargePayload1KScenario(),
        ScenarioType.UnaryLargePayload2K => () => new UnaryLargePayload2KScenario(),
        ScenarioType.UnaryLargePayload4K => () => new UnaryLargePayload4KScenario(),
        ScenarioType.UnaryLargePayload8K => () => new UnaryLargePayload8KScenario(),
        ScenarioType.UnaryLargePayload16K => () => new UnaryLargePayload16KScenario(),
        ScenarioType.UnaryLargePayload32K => () => new UnaryLargePayload32KScenario(),
        ScenarioType.UnaryLargePayload64K => () => new UnaryLargePayload64KScenario(),
        ScenarioType.StreamingHub => () => new StreamingHubScenario(),
        ScenarioType.StreamingHubLargePayload1K => () => new StreamingHubLargePayload1KScenario(),
        ScenarioType.StreamingHubLargePayload2K => () => new StreamingHubLargePayload2KScenario(),
        ScenarioType.StreamingHubLargePayload4K => () => new StreamingHubLargePayload4KScenario(),
        ScenarioType.StreamingHubLargePayload8K => () => new StreamingHubLargePayload8KScenario(),
        ScenarioType.StreamingHubLargePayload16K => () => new StreamingHubLargePayload16KScenario(),
        ScenarioType.StreamingHubLargePayload32K => () => new StreamingHubLargePayload32KScenario(),
        ScenarioType.StreamingHubLargePayload64K => () => new StreamingHubLargePayload64KScenario(),
        _ => throw new Exception("Unknown Scenario"),
    };

    var ctx = new PerformanceTestRunningContext();
    var cts = new CancellationTokenSource();

    WriteLog("Starting scenario...");
    var tasks = new List<Task>();
    for (var i = 0; i < config.Channels; i++)
    {
        var channel = GrpcChannel.ForAddress(config.Url);
        for (var j = 0; j < config.Streams; j++)
        {
            if (config.Verbose) WriteLog($"Channel[{i}] - Stream[{j}]: Run");
            tasks.Add(Task.Run(async () =>
            {
                var scenarioRunner = scenarioFactory();
                await scenarioRunner.PrepareAsync(channel);
                await scenarioRunner.RunAsync(ctx, cts.Token);
            }));
        }
    }
    WriteLog("Warming up...");
    await Task.Delay(TimeSpan.FromSeconds(config.Warmup));
    ctx.Ready();
    WriteLog("Warmup completed");
    
    WriteLog("Running...");
    cts.CancelAfter(TimeSpan.FromSeconds(config.Duration));
    await Task.WhenAll(tasks);
    ctx.Complete();
    WriteLog("Completed.");

    var result = ctx.GetResult();
    WriteLog($"Requests per Second: {result.RequestsPerSecond:0.000} rps");
    WriteLog($"Duration: {result.Duration.TotalSeconds} s");
    WriteLog($"Total Requests: {result.TotalRequests} requests");
}

void PrintStartupInformation()
{
    Console.WriteLine($"MagicOnion {typeof(MagicOnionClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
    Console.WriteLine($"grpc-dotnet {typeof(Grpc.Net.Client.GrpcChannel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
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
    Console.WriteLine($"{nameof(Debugger)}.{nameof(Debugger.IsAttached)}: {Debugger.IsAttached}");
    Console.WriteLine();
}

void WriteLog(string value)
{
    Console.WriteLine($"[{DateTime.Now:s}] {value}");
}

public record ScenarioConfiguration(string Url, int Warmup, int Duration, int Streams, int Channels, bool Verbose);
