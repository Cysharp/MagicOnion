using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MemoryPack;
using MagicOnion.Serialization.MessagePack;
using PerformanceTest.Client;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

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
    bool debug = false,
    string? tags = null
)
{
    var config = new ScenarioConfiguration(url, protocol, clientauth, warmup, duration, streams, channels, verbose);
    var datadog = DatadogMetricsRecorder.Create(tags, validate: validate);

    PrintStartupInformation();

    WriteLog($"Scenario: {scenario}");
    WriteLog($"Url: {config.Url}");
    WriteLog($"Protocol: {config.Protocol}");
    WriteLog($"ClientAuth: {clientauth}");
    WriteLog($"Warmup: {config.Warmup} s");
    WriteLog($"Duration: {config.Duration} s");
    WriteLog($"Streams: {config.Streams}");
    WriteLog($"Channels: {config.Channels}");
    WriteLog($"Rounds: {rounds}");
    WriteLog($"Verbose: {verbose}");
    WriteLog($"Serialization: {serialization}");
    WriteLog($"Debug: {debug}");
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

    // Setup threadpool
    // SetThreads(config.Channels * config.Streams);

    // Create a control channel
    using var channelControl = config.CreateChannel();
    var controlServiceClient = MagicOnionClient.Create<IPerfTestControlService>(channelControl);
    await controlServiceClient.SetMemoryProfilerCollectAllocationsAsync(true);
    (DatadogMetricsRecorder.MagicOnionVersions, DatadogMetricsRecorder.EnableLatestTag) = await controlServiceClient.ExchangeMagicOnionVersionTagAsync(ApplicationInformation.Current.TagMagicOnionVersion, ApplicationInformation.Current.IsLatestMagicOnion); // keep in Client

    ServerInformation serverInfo;
    WriteLog("Gathering the server information...");
    {
        serverInfo = await controlServiceClient.GetServerInformationAsync();
        WriteLog($"Benchmarker {serverInfo.BenchmarkerVersion}");
        WriteLog($"MagicOnion {serverInfo.MagicOnionVersion}");
        WriteLog($"grpc-dotnet {serverInfo.GrpcNetVersion}");
        WriteLog($"{nameof(ApplicationInformation.OSDescription)}: {serverInfo.OSDescription}");
        WriteLog($"IsLatestMagicOnion {serverInfo.IsLatestMagicOnion}");
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
                results = new List<PerformanceResult>(10000);
                resultsByScenario[scenario2] = results;
            }
            var result = await RunScenarioAsync(scenario2, config, config.ChannelList, controlServiceClient);
            results.Add(result);

            WriteLog($"Interval 1s for next scenario...");
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing); // interval
        }
    }
    WriteLog($"All scenario complete");

    WriteLog($"Saving metrics...");
    foreach (var (s, results) in resultsByScenario)
    {
        await datadog.PutClientBenchmarkMetricsAsync(s, ApplicationInformation.Current, results);
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
        writer.WriteLine($"Benchmarker {serverInfo.BenchmarkerVersion}");
        writer.WriteLine($"MagicOnion {serverInfo.MagicOnionVersion}");
        writer.WriteLine($"grpc-dotnet {serverInfo.GrpcNetVersion}");
        writer.WriteLine($"MessagePack {serverInfo.MessagePackVersion}");
        writer.WriteLine($"MemoryPack {serverInfo.MemoryPackVersion}");
        writer.WriteLine($"Build Configuration: {(serverInfo.IsReleaseBuild ? "Release" : "Debug")}");
        writer.WriteLine($"FrameworkDescription: {serverInfo.FrameworkDescription}");
        writer.WriteLine($"OSDescription: {serverInfo.OSDescription}");
        writer.WriteLine($"OSArchitecture: {serverInfo.OSArchitecture}");
        writer.WriteLine($"ProcessArchitecture : {serverInfo.ProcessArchitecture}");
        writer.WriteLine($"CpuModelName : {serverInfo.CpuModelName}");
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
                writer.WriteLine($"Max CPU Usage      : {result.hardware.MaxCpuUsagePercent:0.00} %");
                writer.WriteLine($"Avg CPU Usage      : {result.hardware.AvgCpuUsagePercent:0.00} %");
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

    WriteLog($"Benchmark completed");
}

async Task<PerformanceResult> RunScenarioAsync(ScenarioType scenario, ScenarioConfiguration config, IReadOnlyList<GrpcChannel> channels, IPerfTestControlService controlService)
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
        ScenarioType.ServerStreaming => () => new ServerStreamingScenario(),
        ScenarioType.Broadcast => () => new BroadcastScenario(),
        _ => throw new Exception($"Unknown Scenario: {scenario}"),
    };

    var ctx = new PerformanceTestRunningContext(connectionCount: config.Channels, serverTimeout: (config.Warmup, config.Duration));
    using var cts = new CancellationTokenSource();
    var cleanIndex = 0;
    var threadBefore = ThreadPool.ThreadCount;

    WriteLog($"Starting scenario '{scenario}'");
    var tasks = new List<Task>();
    var scenarios = new List<IScenario>();
    for (var i = 0; i < config.Channels; i++)
    {
        for (var j = 0; j < config.Streams; j++)
        {
            if (config.Verbose) WriteLog($"Channel[{i}] - Stream[{j}]: Run");
            var (connectionId, streamId) = (i, j);
            tasks.Add(Task.Run(async () =>
            {
                var scenarioRunner = scenarioFactory();
                scenarios.Add(scenarioRunner);
                await scenarioRunner.PrepareAsync(channels[connectionId]);
                await scenarioRunner.RunAsync(connectionId, ctx, cts.Token);
                if (config.Verbose) WriteLog($"Complete: {connectionId}:{streamId}");
            }));
        }
    }

    await controlService.CreateMemoryProfilerSnapshotAsync("Before Warmup");
    WriteLog($"Warming up...");
    await Task.Delay(TimeSpan.FromSeconds(config.Warmup));
    ctx.Ready();
    await controlService.CreateMemoryProfilerSnapshotAsync("After Warmup/Run");
    WriteLog("Warmup completed");

    WriteLog($"Running...");
    cts.CancelAfter(TimeSpan.FromSeconds(config.Duration));
    await Task.WhenAll(tasks);
    ctx.Complete();
    var threadAfter = ThreadPool.ThreadCount;
    WriteLog("Completed");

    WriteLog("Cleaning up...");
    foreach (var s in scenarios.Chunk(Math.Clamp(scenarios.Count / 3, 1, scenarios.Count)))
    {
        WriteLog($"...Cleaning up ({cleanIndex++})");
        try
        {
            await Task.WhenAll(s.Select(x => x.CompleteAsync()));
        }
        catch (NullReferenceException)
        {
            // ignore
        }
    }
    await controlService.CreateMemoryProfilerSnapshotAsync("Completed");
    WriteLog("Cleanup completed");

    var result = ctx.GetResult();
    WriteLog($"Requests per Second: {result.RequestsPerSecond:0.000} rps");
    WriteLog($"Duration: {result.Duration.TotalSeconds} s");
    WriteLog($"Total Requests: {result.TotalRequests} requests");
    WriteLog($"Total Errors: {result.errors} errors");
    WriteLog($"Mean latency: {result.Latency.Mean:0.###} ms");
    WriteLog($"Max latency: {result.Latency.Max:0.###} ms");
    WriteLog($"p50 latency: {result.Latency.P50:0.###} ms");
    WriteLog($"p75 latency: {result.Latency.P75:0.###} ms");
    WriteLog($"p90 latency: {result.Latency.P90:0.###} ms");
    WriteLog($"p99 latency: {result.Latency.P99:0.###} ms");

    WriteLog($"Max CPU Usage: {result.hardware.MaxCpuUsagePercent:0.000} %");
    WriteLog($"Avg CPU Usage: {result.hardware.AvgCpuUsagePercent:0.000} %");
    WriteLog($"Max Memory Usage: {result.hardware.MaxMemoryUsageMB} MB");
    WriteLog($"Avg Memory Usage: {result.hardware.AvgMemoryUsageMB} MB");

    WriteLog($"Threads (diff): {threadAfter} ({(threadAfter - threadBefore):+#;-#;0})");

    return result;
}

void PrintStartupInformation(TextWriter? writer = null)
{
    writer ??= Console.Out;

    writer.WriteLine($"Benchmarker {ApplicationInformation.Current.BenchmarkerVersion}");
    writer.WriteLine($"MagicOnion {ApplicationInformation.Current.MagicOnionVersion}");
    writer.WriteLine($"grpc-dotnet {ApplicationInformation.Current.GrpcNetVersion}");
    writer.WriteLine($"MessagePack {ApplicationInformation.Current.MessagePackVersion}");
    writer.WriteLine($"MemoryPack {ApplicationInformation.Current.MemoryPackVersion}");
    writer.WriteLine($"IsLatestMagicOnion {ApplicationInformation.Current.IsLatestMagicOnion}");
    writer.WriteLine();

    writer.WriteLine("Configurations:");
    writer.WriteLine($"Build Configuration: {(ApplicationInformation.Current.IsReleaseBuild ? "Release" : "Debug")}");
    writer.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {ApplicationInformation.Current.FrameworkDescription}");
    writer.WriteLine($"{nameof(RuntimeInformation.OSDescription)}: {ApplicationInformation.Current.OSDescription}");
    writer.WriteLine($"{nameof(RuntimeInformation.OSArchitecture)}: {ApplicationInformation.Current.OSArchitecture}");
    writer.WriteLine($"{nameof(RuntimeInformation.ProcessArchitecture)}: {ApplicationInformation.Current.ProcessArchitecture}");
    writer.WriteLine($"{nameof(ApplicationInformation.Current.CpuModelName)}: {ApplicationInformation.Current.CpuModelName}");
    writer.WriteLine($"{nameof(GCSettings.IsServerGC)}: {ApplicationInformation.Current.IsServerGC}");
    writer.WriteLine($"{nameof(ApplicationInformation.Current.ProcessorCount)}: {ApplicationInformation.Current.ProcessorCount}");
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
        // Do not include ServerStreaming to CI. ServerStreaming consume a lot of resource and will impact later scenarios.
        ScenarioType.CI => Enum.GetValues<ScenarioType>().Where(x => x == ScenarioType.Unary || x == ScenarioType.StreamingHub),
        _ => [scenario],
    };
}

static class DatadogMetricsRecorderExtensions
{
    /// <summary>
    /// Put Client Benchmark metrics average to background. 
    /// </summary>
    /// <param name="recorder"></param>
    /// <param name="scenario"></param>
    /// <param name="applicationInfo"></param>
    /// <param name="results"></param>
    public static async Task PutClientBenchmarkMetricsAsync(this DatadogMetricsRecorder recorder, ScenarioType scenario, ApplicationInformation applicationInfo, IReadOnlyList<PerformanceResult> results)
    {
        var filtered = RemoveOutlinerByIQR(results);

        Post(recorder, scenario, applicationInfo, filtered, false);
        if (DatadogMetricsRecorder.EnableLatestTag)
        {
            Post(recorder, scenario, applicationInfo, filtered, true);
        }

        // wait until send complete
        await recorder.WaitSaveAsync();

        static void Post(DatadogMetricsRecorder recorder, ScenarioType scenario, ApplicationInformation applicationInfo, IReadOnlyList<PerformanceResult> filtered, bool enableLatestTag)
        {
            var magicOnionTag = enableLatestTag ? recorder.TagLatestMagicOnion : recorder.TagMagicOnion;
            var tags = MetricsTagCache.Get((recorder.TagBranch, recorder.TagLegend, recorder.TagStreams, recorder.TagProtocol, recorder.TagSerialization, magicOnionTag, scenario, applicationInfo), static x => [
                $"legend:{x.scenario.ToString().ToLower()}-{x.TagLegend}{x.TagStreams}",
                $"branch:{x.TagBranch}",
                $"magiconion:{x.magicOnionTag}",
                $"protocol:{x.TagProtocol}",
                $"process_arch:{x.applicationInfo.ProcessArchitecture}",
                $"process_count:{x.applicationInfo.ProcessorCount}",
                $"scenario:{x.scenario}",
                $"serialization:{x.TagSerialization}",
                $"streams:{x.TagStreams}",
            ]);

            // Don't want to await each put. Let's send it to queue and await when benchmark ends.
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.rps", filtered.Select(x => x.RequestsPerSecond).Average(), DatadogMetricsType.Rate, tags, "request"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.total_requests", filtered.Select(x => x.TotalRequests).Average(), DatadogMetricsType.Gauge, tags, "request"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.latency_mean", filtered.Select(x => x.Latency.Mean).Average(), DatadogMetricsType.Gauge, tags, "millisecond"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.cpu_usage_max", filtered.Select(x => x.hardware.MaxCpuUsagePercent).Average(), DatadogMetricsType.Gauge, tags, "percent"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.cpu_usage_avg", filtered.Select(x => x.hardware.AvgCpuUsagePercent).Average(), DatadogMetricsType.Gauge, tags, "percent"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.memory_usage_max", filtered.Select(x => x.hardware.MaxMemoryUsageMB).Average(), DatadogMetricsType.Gauge, tags, "megabyte"));
            recorder.Record(recorder.SendAsync("benchmark.magiconion.client.memory_usage_avg", filtered.Select(x => x.hardware.AvgMemoryUsageMB).Average(), DatadogMetricsType.Gauge, tags, "megabyte"));
        }

        // Remove Outliner by IQR
        static IReadOnlyList<PerformanceResult> RemoveOutlinerByIQR(IReadOnlyList<PerformanceResult> data)
        {
            // sort rps, latency.mean
            var rps = data.Select(x => x.RequestsPerSecond).OrderBy(x => x).ToArray();
            var mean = data.Select(x => x.Latency.Mean).OrderBy(x => x).ToArray();

            // get outliner for rps
            var lowerBoundRps = OutlinerHelper.GetLowerBound(rps);
            var upperBoundRps = OutlinerHelper.GetUpperBound(rps);

            // get outliner for mean
            var lowerBoundMean = OutlinerHelper.GetLowerBound(mean);
            var upperBoundMean = OutlinerHelper.GetUpperBound(mean);

            // compute tuple in range
            var filteredData = data
                .Where(x => x.RequestsPerSecond >= lowerBoundRps && x.RequestsPerSecond <= upperBoundRps)
                .Where(x => x.Latency.Mean >= lowerBoundMean && x.Latency.Mean <= upperBoundMean)
                .ToArray();

            return filteredData;
        }
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

    private readonly bool clientAuth;
    private bool useHttp3;

    public IReadOnlyList<GrpcChannel> ChannelList => channelList;
    private readonly List<GrpcChannel> channelList;

    private record TlsFile(string PfxFileName, string Password)
    {
        public static TlsFile Default = new TlsFile("Certs/client.pfx", "1111");
    }

    public ScenarioConfiguration(string url, string protocol, bool clientAuth, int warmup, int duration, int streams, int channels, bool verbose)
    {
        bool useHttp3 = false;
        switch (protocol)
        {
            case "h2c":
                {
                    url = url.Replace("https://", "http://");
                    break;
                }
            case "h2":
                {
                    url = url.Replace("http://", "https://");
                    break;
                }
            case "h3":
                {
                    url = url.Replace("http://", "https://");
                    useHttp3 = true;
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
        this.clientAuth = clientAuth;
        this.useHttp3 = useHttp3;

        channelList = new List<GrpcChannel>(channels);
        for (var i = 0; i < Channels; i++)
        {
            channelList.Add(CreateChannel());
        }
    }

    public GrpcChannel CreateChannel()
    {
        var httpClientHandler = new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            EnableMultipleHttp2Connections = true,
        };

        // allow server's self cert
        httpClientHandler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
        };

        if (clientAuth)
        {
            var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
            var certPath = Path.Combine(basePath!, TlsFile.Default.PfxFileName);
            var clientCertificates = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12CollectionFromFile(certPath, TlsFile.Default.Password);
            httpClientHandler.SslOptions.ClientCertificates = clientCertificates;
        }

        HttpMessageHandler httpMessageHandler = httpClientHandler;
        if (useHttp3)
        {
            // HTTP/3 should never call this? But this is required for channel.State handling :(
            //httpClientHandler.ConnectCallback = (_, _) => throw new InvalidOperationException("Should never be called for H3");
            httpMessageHandler = new Http3Handler(httpClientHandler);
        }

        return GrpcChannel.ForAddress(Url, new GrpcChannelOptions
        {
            HttpHandler = httpMessageHandler,
        });
    }

    // temporary solution for .NET8 and lower
    public class Http3Handler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Force H3 on all requests.
            request.Version = System.Net.HttpVersion.Version30;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
