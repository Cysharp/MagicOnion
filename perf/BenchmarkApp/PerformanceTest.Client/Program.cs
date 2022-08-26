using Grpc.Core;
using Grpc.Net.Client;

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

    WriteLog($"Scenario: {scenario}");
    WriteLog($"Url: {config.Url}");
    WriteLog($"Warmup: {config.Warmup}");
    WriteLog($"Duration: {config.Duration}");
    WriteLog($"Streams: {config.Streams}");
    WriteLog($"Channels: {config.Channels}");

    Func<IScenario> scenarioFactory = scenario switch
    {
        ScenarioType.Unary => () => new UnaryScenario(),
        ScenarioType.StreamingHub => () => new StreamingHubScenario(),
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
    WriteLog("Warmup Completed");
    
    WriteLog("Running...");
    cts.CancelAfter(TimeSpan.FromSeconds(config.Duration));
    await Task.WhenAll(tasks);
    ctx.Complete();
    WriteLog("Completed.");

    var result = ctx.GetResult();
    WriteLog($"Requests per Second: {result.RequestsPerSecond:0.000} rps");
    WriteLog($"Duration: {result.Duration.TotalSeconds} sec");
    WriteLog($"Requests: {result.TotalRequests} requests");
}

void WriteLog(string value)
{
    Console.WriteLine($"[{DateTime.Now:s}] {value}");
}

public record ScenarioConfiguration(string Url, int Warmup, int Duration, int Streams, int Channels, bool Verbose);
