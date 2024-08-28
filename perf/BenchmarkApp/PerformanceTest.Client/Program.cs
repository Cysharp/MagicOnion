using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MemoryPack;
using MagicOnion.Serialization.MessagePack;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

var app = ConsoleApp.Create(args);
app.AddRootCommand(Main);
app.Run();

async Task Main(
    [Option("s")] ScenarioType scenario,
    [Option("u")] string url,
    [Option("p")] string protocol = "h2c",
    bool clientauth = false,
    [Option("w")] int warmup = 10,
    [Option("d")] int duration = 10,
    [Option("t")] int streams = 10,
    [Option("c")] int channels = 10,
    [Option("r")] string? report = null,
    uint rounds = 1,
    [Option("v")] bool verbose = false,
    SerializationType serialization = SerializationType.MessagePack,
    bool validate = false,
    string? tags = null
)
{
    var config = new ScenarioConfiguration(url, protocol, clientauth, warmup, duration, streams, channels, verbose);
    var datadog = DatadogMetricsRecorder.Create(tags, validate: validate);

    PrintStartupInformation();

    WriteLog($"Scenario: {scenario}");
    WriteLog($"Url: {config.Url}");
    WriteLog($"Protocol: {config.Protocol}");
    WriteLog($"Warmup: {config.Warmup} s");
    WriteLog($"Duration: {config.Duration} s");
    WriteLog($"Streams: {config.Streams}");
    WriteLog($"Channels: {config.Channels}");
    WriteLog($"Rounds: {rounds}");
    WriteLog($"Serialization: {serialization}");
    WriteLog($"Tags: {tags}");

    // Setup serializer
    switch (serialization)
    {
        case SerializationType.MessagePack:
            MagicOnionSerializerProvider.Default = MessagePackMagicOnionSerializerProvider.Default;
            break;
        case SerializationType.MemoryPack:
            MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;
            break;
    }

    // Create a control channel
    using var channelControl = config.CreateChannel();
    var controlServiceClient = MagicOnionClient.Create<IPerfTestControlService>(channelControl);
    await controlServiceClient.SetMemoryProfilerCollectAllocationsAsync(true);

    ServerInformation serverInfo;
    WriteLog("Gathering the server information...");
    {
        serverInfo = await controlServiceClient.GetServerInformationAsync();
        WriteLog($"MagicOnion {serverInfo.MagicOnionVersion}");
        WriteLog($"grpc-dotnet {serverInfo.GrpcNetVersion}");
        WriteLog($"{nameof(ApplicationInformation.OSDescription)}: {serverInfo.OSDescription}");
    }

    var resultsByScenario = new Dictionary<ScenarioType, List<PerformanceResult>>();
    var runScenarios = GetRunScenarios(scenario);
    for (var i = 1; i <= rounds; i++)
    {
        WriteLog($"Round: {i}");
        foreach (var scenario2 in runScenarios)
        {
            if (!resultsByScenario.TryGetValue(scenario2, out var results))
            {
                results = new List<PerformanceResult>();
                resultsByScenario[scenario2] = results;
            }
            var result = await RunScenarioAsync(scenario2, config, controlServiceClient);
            results.Add(result);
            await datadog.PutClientBenchmarkMetricsAsync(scenario2, ApplicationInformation.Current, serialization, result);
        }
    }

    if (!string.IsNullOrWhiteSpace(report))
    {
        WriteLog($"Write report to '{report}'");
        using var writer = File.CreateText(report);
        writer.WriteLine($"Created at {DateTime.Now}");
        writer.WriteLine($"========================================");
        PrintStartupInformation(writer);
        writer.WriteLine($"========================================");
        writer.WriteLine($"Server Information:");
        writer.WriteLine($"MagicOnion {serverInfo.MagicOnionVersion}");
        writer.WriteLine($"grpc-dotnet {serverInfo.GrpcNetVersion}");
        writer.WriteLine($"MessagePack {serverInfo.MessagePackVersion}");
        writer.WriteLine($"MemoryPack {serverInfo.MemoryPackVersion}");
        writer.WriteLine($"Build Configuration: {(serverInfo.IsReleaseBuild ? "Release" : "Debug")}");
        writer.WriteLine($"FrameworkDescription: {serverInfo.FrameworkDescription}");
        writer.WriteLine($"OSDescription: {serverInfo.OSDescription}");
        writer.WriteLine($"OSArchitecture: {serverInfo.OSArchitecture}");
        writer.WriteLine($"ProcessArchitecture : {serverInfo.ProcessArchitecture}");
        writer.WriteLine($"IsServerGC: {serverInfo.IsServerGC}");
        writer.WriteLine($"ProcessorCount: {serverInfo.ProcessorCount}");
        writer.WriteLine($"========================================");
        writer.WriteLine($"Scenario     : {scenario}");
        writer.WriteLine($"Url          : {config.Url}");
        writer.WriteLine($"Protocol     : {config.Protocol}");
        writer.WriteLine($"Warmup       : {config.Warmup} s");
        writer.WriteLine($"Duration     : {config.Duration} s");
        writer.WriteLine($"Streams      : {config.Streams}");
        writer.WriteLine($"Channels     : {config.Channels}");
        writer.WriteLine($"Serialization: {serialization}");
        writer.WriteLine($"========================================");
        foreach (var (s, results) in resultsByScenario)
        {
            writer.WriteLine($"Scenario           : {s}");
            foreach (var (result, round) in results.Select((x, i) => (x, i)))
            {
                writer.WriteLine($"Round              : {round}");
                writer.WriteLine($"Requests per Second: {result.RequestsPerSecond:0.000} rps");
                writer.WriteLine($"Duration           : {result.Duration.TotalSeconds} s");
                writer.WriteLine($"Total Requests     : {result.TotalRequests} requests");
                writer.WriteLine($"Mean latency       : {result.Latency.Mean:0.###} ms");
                writer.WriteLine($"Max latency        : {result.Latency.Max:0.###} ms");
                writer.WriteLine($"p50 latency        : {result.Latency.P50:0.###} ms");
                writer.WriteLine($"p90 latency        : {result.Latency.P90:0.###} ms");
                writer.WriteLine($"p99 latency        : {result.Latency.P99:0.###} ms");
                writer.WriteLine($"Max CPU Usage      : {result.hardware.MaxCpuUsage:0.00} %");
                writer.WriteLine($"Avg CPU Usage      : {result.hardware.AvgCpuUsage:0.00} %");
                writer.WriteLine($"Max Memory Usage   : {result.hardware.MaxMemoryUsageMB} MB");
                writer.WriteLine($"Avg Memory Usage   : {result.hardware.AvgMemoryUsageMB} MB");
                writer.WriteLine($"========================================");
            }
        }

        writer.WriteLine($"Scenario\t{string.Join("\t", Enumerable.Range(1, (int)rounds).Select(x => $"Requests/s ({x})"))}\tRequests/s (Avg)");
        foreach (var (s, results) in resultsByScenario)
        {
            writer.WriteLine($"{s}\t{string.Join("\t", results.Select(x => x.RequestsPerSecond.ToString("0.000")))}\t{results.Average(x => x.RequestsPerSecond):0.000}");
        }
    }
}

async Task<PerformanceResult> RunScenarioAsync(ScenarioType scenario, ScenarioConfiguration config, IPerfTestControlService controlService)
{
    Func<IScenario> scenarioFactory = scenario switch
    {
        ScenarioType.Unary => () => new UnaryScenario(),
        ScenarioType.UnaryComplex => () => new UnaryComplexScenario(),
        ScenarioType.UnaryLargePayload1K => () => new UnaryLargePayload1KScenario(),
        ScenarioType.UnaryLargePayload2K => () => new UnaryLargePayload2KScenario(),
        ScenarioType.UnaryLargePayload4K => () => new UnaryLargePayload4KScenario(),
        ScenarioType.UnaryLargePayload8K => () => new UnaryLargePayload8KScenario(),
        ScenarioType.UnaryLargePayload16K => () => new UnaryLargePayload16KScenario(),
        ScenarioType.UnaryLargePayload32K => () => new UnaryLargePayload32KScenario(),
        ScenarioType.UnaryLargePayload64K => () => new UnaryLargePayload64KScenario(),
        ScenarioType.StreamingHub => () => new StreamingHubScenario(),
        ScenarioType.StreamingHubValueTask => () => new StreamingHubValueTaskScenario(),
        ScenarioType.StreamingHubComplex => () => new StreamingHubComplexScenario(),
        ScenarioType.StreamingHubComplexValueTask => () => new StreamingHubComplexValueTaskScenario(),
        ScenarioType.StreamingHubLargePayload1K => () => new StreamingHubLargePayload1KScenario(),
        ScenarioType.StreamingHubLargePayload2K => () => new StreamingHubLargePayload2KScenario(),
        ScenarioType.StreamingHubLargePayload4K => () => new StreamingHubLargePayload4KScenario(),
        ScenarioType.StreamingHubLargePayload8K => () => new StreamingHubLargePayload8KScenario(),
        ScenarioType.StreamingHubLargePayload16K => () => new StreamingHubLargePayload16KScenario(),
        ScenarioType.StreamingHubLargePayload32K => () => new StreamingHubLargePayload32KScenario(),
        ScenarioType.StreamingHubLargePayload64K => () => new StreamingHubLargePayload64KScenario(),
        ScenarioType.PingpongStreamingHub => () => new PingpongStreamingHubScenario(),
        ScenarioType.PingpongCachedStreamingHub => () => new PingpongCachedStreamingHubScenario(),
        _ => throw new Exception($"Unknown Scenario: {scenario}"),
    };

    var ctx = new PerformanceTestRunningContext(connectionCount: config.Channels);
    var cts = new CancellationTokenSource();

    WriteLog($"Starting scenario '{scenario}'...");
    var tasks = new List<Task>();
    for (var i = 0; i < config.Channels; i++)
    {
        var channel = config.CreateChannel();
        for (var j = 0; j < config.Streams; j++)
        {
            if (config.Verbose) WriteLog($"Channel[{i}] - Stream[{j}]: Run");
            var connectionId = i;
            tasks.Add(Task.Run(async () =>
            {
                var scenarioRunner = scenarioFactory();
                await scenarioRunner.PrepareAsync(channel);
                await scenarioRunner.RunAsync(connectionId, ctx, cts.Token);
            }));
        }
    }

    await controlService.CreateMemoryProfilerSnapshotAsync("Before Warmup");
    WriteLog("Warming up...");
    await Task.Delay(TimeSpan.FromSeconds(config.Warmup));
    ctx.Ready();
    await controlService.CreateMemoryProfilerSnapshotAsync("After Warmup/Run");
    WriteLog("Warmup completed");

    WriteLog("Running...");
    cts.CancelAfter(TimeSpan.FromSeconds(config.Duration));
    await Task.WhenAll(tasks);
    ctx.Complete();
    WriteLog("Completed.");
    await controlService.CreateMemoryProfilerSnapshotAsync("Completed");

    var result = ctx.GetResult();
    WriteLog($"Requests per Second: {result.RequestsPerSecond:0.000} rps");
    WriteLog($"Duration: {result.Duration.TotalSeconds} s");
    WriteLog($"Total Requests: {result.TotalRequests} requests");
    WriteLog($"Mean latency: {result.Latency.Mean:0.###} ms");
    WriteLog($"Max latency: {result.Latency.Max:0.###} ms");
    WriteLog($"p50 latency: {result.Latency.P50:0.###} ms");
    WriteLog($"p75 latency: {result.Latency.P75:0.###} ms");
    WriteLog($"p90 latency: {result.Latency.P90:0.###} ms");
    WriteLog($"p99 latency: {result.Latency.P99:0.###} ms");

    WriteLog($"Max CPU Usage: {result.hardware.MaxCpuUsage:0.000} %");
    WriteLog($"Avg CPU Usage: {result.hardware.AvgCpuUsage:0.000} %");
    WriteLog($"Max Memory Usage: {result.hardware.MaxMemoryUsageMB} MB");
    WriteLog($"Avg Memory Usage: {result.hardware.AvgMemoryUsageMB} MB");

    return result;
}

void PrintStartupInformation(TextWriter? writer = null)
{
    writer ??= Console.Out;

    writer.WriteLine($"MagicOnion {ApplicationInformation.Current.MagicOnionVersion}");
    writer.WriteLine($"grpc-dotnet {ApplicationInformation.Current.GrpcNetVersion}");
    writer.WriteLine($"MessagePack {ApplicationInformation.Current.MessagePackVersion}");
    writer.WriteLine($"MemoryPack {ApplicationInformation.Current.MemoryPackVersion}");
    writer.WriteLine();

    writer.WriteLine("Configurations:");
    writer.WriteLine($"Build Configuration: {(ApplicationInformation.Current.IsReleaseBuild ? "Release" : "Debug")}");
    writer.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {ApplicationInformation.Current.FrameworkDescription}");
    writer.WriteLine($"{nameof(RuntimeInformation.OSDescription)}: {ApplicationInformation.Current.OSDescription}");
    writer.WriteLine($"{nameof(RuntimeInformation.OSArchitecture)}: {ApplicationInformation.Current.OSArchitecture}");
    writer.WriteLine($"{nameof(RuntimeInformation.ProcessArchitecture)}: {ApplicationInformation.Current.ProcessArchitecture}");
    writer.WriteLine($"{nameof(GCSettings.IsServerGC)}: {ApplicationInformation.Current.IsServerGC}");
    writer.WriteLine($"{nameof(Environment.ProcessorCount)}: {ApplicationInformation.Current.ProcessorCount}");
    writer.WriteLine($"{nameof(Debugger)}.{nameof(Debugger.IsAttached)}: {ApplicationInformation.Current.IsAttached}");
    writer.WriteLine();
}

void WriteLog(string value)
{
    Console.WriteLine($"[{DateTime.Now:s}] {value}");
}

IEnumerable<ScenarioType> GetRunScenarios(ScenarioType scenario)
{
    return scenario switch
    {
        ScenarioType.All => Enum.GetValues<ScenarioType>().Where(x => x != ScenarioType.All && x != ScenarioType.CI),
        ScenarioType.CI => Enum.GetValues<ScenarioType>().Where(x => x == ScenarioType.Unary || x == ScenarioType.StreamingHub || x == ScenarioType.PingpongStreamingHub),
        _ => [scenario],
    };
}

static class DatadogMetricsRecorderExtensions
{
    /// <summary>
    /// Put Client Benchmark metrics to background. 
    /// </summary>
    /// <param name="recorder"></param>
    /// <param name="scenario"></param>
    /// <param name="applicationInfo"></param>
    /// <param name="serialization"></param>
    /// <param name="result"></param>
    public static async Task PutClientBenchmarkMetricsAsync(this DatadogMetricsRecorder recorder, ScenarioType scenario, ApplicationInformation applicationInfo, SerializationType serialization, PerformanceResult result)
    {
        var tags = MetricsTagCache.Get((recorder.TagBranch, recorder.TagLegend, recorder.TagStreams, scenario, applicationInfo, serialization), static x => [
            $"legend:{x.scenario.ToString().ToLower()}-{x.TagLegend}{x.TagStreams}",
            $"branch:{x.TagBranch}",
            $"streams:{x.TagStreams}",
            $"process_arch:{x.applicationInfo.ProcessArchitecture}",
            $"process_count:{x.applicationInfo.ProcessorCount}",
            $"scenario:{x.scenario}",
            $"serialization:{x.serialization}"
        ]);

        // Don't want to await each put. Let's send it to queue and await when benchmark ends.
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.rps", result.RequestsPerSecond, DatadogMetricsType.Rate, tags, "request"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.total_requests", result.TotalRequests, DatadogMetricsType.Gauge, tags, "request"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.latency_mean", result.Latency.Mean, DatadogMetricsType.Gauge, tags, "millisecond"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.cpu_usage_max", result.hardware.MaxCpuUsage, DatadogMetricsType.Gauge, tags, "percent"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.cpu_usage_avg", result.hardware.AvgCpuUsage, DatadogMetricsType.Gauge, tags, "percent"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.memory_usage_max", result.hardware.MaxMemoryUsageMB, DatadogMetricsType.Gauge, tags, "megabyte"));
        recorder.Record(recorder.SendAsync("benchmark.magiconion.client.memory_usage_avg", result.hardware.AvgMemoryUsageMB, DatadogMetricsType.Gauge, tags, "megabyte"));

        // wait until send complete
        await recorder.WaitSaveAsync();
    }
}

public class ScenarioConfiguration
{
    public string Url { get; }
    public string Protocol { get; }
    public int Warmup { get; }
    public int Duration { get; }
    public int Streams { get; }
    public int Channels { get; }
    public bool Verbose { get; }

    private readonly HttpMessageHandler httpHandler;

    private record TlsFile(string PfxFileName, string Password)
    {
        public static TlsFile Default = new TlsFile("Certs/client.pfx", "1111");
    }

    public ScenarioConfiguration(string url, string protocol, bool clientauth, int warmup, int duration, int streams, int channels, bool verbose)
    {
        var handler = new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
        };
        handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true; // allow self cert
        if (clientauth)
        {
            var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
            var certPath = Path.Combine(basePath!, TlsFile.Default.PfxFileName);
            // .NET 9 API....
            //var clientCertificates = X509CertificateLoader.LoadPkcs12CollectionFromFile(certPath, TlsFile.Default.Password);
            var clientCertificates = new System.Security.Cryptography.X509Certificates.X509Certificate2Collection(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, TlsFile.Default.Password));
            handler.SslOptions.ClientCertificates = clientCertificates;
        }

        switch (protocol)
        {
            case "h2c":
                {
                    url = url.Replace("https://", "http://");
                    httpHandler = handler;
                    break;
                }
            case "h2":
                {
                    url = url.Replace("http://", "https://");
                    httpHandler = handler;
                    break;
                }
            case "h3":
                {
                    url = url.Replace("http://", "https://");

                    handler.ConnectCallback = (_, _) => throw new InvalidOperationException("Should never be called for H3");
                    httpHandler = new Http3Handler(handler);
                    break;
                }
            default:
                throw new NotImplementedException(protocol);
        };

        Url = url;
        Protocol = protocol;
        Warmup = warmup;
        Duration = duration;
        Streams = streams;
        Channels = channels;
        Verbose = verbose;
    }

    public GrpcChannel CreateChannel()
    {
        return Protocol switch
        {
            "h2c" => GrpcChannel.ForAddress(Url),
            "h2" => GrpcChannel.ForAddress(Url, new GrpcChannelOptions
            {
                HttpHandler = httpHandler,
            }),
            // h3 can use from Windows 11 Build 22000+, or Linux with libmsquic. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-8.0
            "h3" => GrpcChannel.ForAddress(Url, new GrpcChannelOptions
            {
                HttpHandler = httpHandler,
                // .NET 9 API....
                // HttpVersion = new Version(3, 0), // Force H3 on all requests
            }),
            _ => throw new NotImplementedException(Protocol),
        };
    }

    // temporary solution for .NET8 and lower
    public class Http3Handler : DelegatingHandler
    {
        public Http3Handler() { }
        public Http3Handler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Force H3 on all requests.
            request.Version = System.Net.HttpVersion.Version30;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
