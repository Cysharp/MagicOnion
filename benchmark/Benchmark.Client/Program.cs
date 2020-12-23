using Benchmark.Client;
using Benchmark.Client.Converters;
using Benchmark.Client.Reports;
using Benchmark.Client.Scenarios;
using Benchmark.Client.Storage;
using ConsoleAppFramework;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ZLogger;

var builder = Host.CreateDefaultBuilder()
    .ConfigureLogging((hostContext, logging) =>
    {
        logging.ClearProviders();
        logging.AddZLoggerConsole(configure => configure.EnableStructuredLogging = false);
        logging.SetMinimumLevel(LogLevel.Trace);
    });
if (Environment.GetEnvironmentVariable("BENCHCLIENT_RUNASWEB") == "true")
{
    var hostAddress = Environment.GetEnvironmentVariable("BENCHCLIENT_HOSTADDRESS");    
    await builder.RunConsoleAppFrameworkWebHostingAsync(hostAddress ?? "http://localhost:8080");
}
else
{
    await builder.RunConsoleAppFrameworkAsync<Main>(args);
}

public class Main : ConsoleAppBase
{
    private readonly string _path;
    public Main()
    {
        _path = Environment.GetEnvironmentVariable("BENCHCLIENT_S3BUCKET") ?? "bench-magiconion-s3-bucket-5c7e45b";
    }

    public async Task BenchUnary(string hostAddress = "http://localhost:5000", string reportId = "", bool debug = false)
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();

        var executeId = Guid.NewGuid().ToString();
        Context.Logger.LogInformation($"reportId: {reportId}, executeId: {executeId}");

        var reporter = new BenchReporter(reportId, executeId, Dns.GetHostName());
        var itelations = new[] { 256, 1024, 4096, 16384, 20000 };
        reporter.Begin();
        {
            foreach (var itelation in itelations)
            {
                // Connect to the server using gRPC channel.
                var channel = GrpcChannel.ForAddress(hostAddress);

                // Unary
                Context.Logger.LogInformation($"Begin unary {itelation} requests.");
                var unary = new UnaryBenchmarkScenario(channel, reporter);
                await unary.Run(itelation);
            }
        }
        reporter.End();

        // output
        var benchJson = reporter.ToJson();
        if (debug)
        {
            Context.Logger.LogInformation(benchJson);
        }

        // put json to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save("bench-magiconion-s3-bucket-5c7e45b", $"reports/{reporter.ReportId}", reporter.Name + ".json", benchJson, ct: Context.CancellationToken);
    }

    public async Task BenchHub(string hostAddress = "http://localhost:5000", string reportId = "", bool debug = false)
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();

        var executeId = Guid.NewGuid().ToString();
        Context.Logger.LogInformation($"reportId: {reportId}, executeId: {executeId}");

        var reporter = new BenchReporter(reportId, executeId, Dns.GetHostName());
        var itelations = new[] { 256, 1024, 4096, 16384, 20000 };
        reporter.Begin();
        {
            foreach (var itelation in itelations)
            {
                // Connect to the server using gRPC channel.
                var channel = GrpcChannel.ForAddress(hostAddress);

                // StreamingHub
                Context.Logger.LogInformation($"Begin Streaming requests.");
                await using var hub = new HubBenchmarkScenario(channel, reporter);
                await hub.Run(itelation);
            }
        }
        reporter.End();

        // output
        var benchJson = reporter.ToJson();
        if (debug)
        {
            Context.Logger.LogInformation(benchJson);
        }

        // put json to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save("bench-magiconion-s3-bucket-5c7e45b", $"reports/{reporter.ReportId}", reporter.Name + ".json", benchJson, ct: Context.CancellationToken);
    }

    public async Task BenchAll(string hostAddress = "http://localhost:5000", int itelation = 10000, string reportId = "")
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();

        var executeId = Guid.NewGuid().ToString();
        Context.Logger.LogInformation($"reportId: {reportId}, executeId: {executeId}");

        var reporter = new BenchReporter(reportId, executeId, Dns.GetHostName());

        // Connect to the server using gRPC channel.
        var channel = GrpcChannel.ForAddress(hostAddress);

        reporter.Begin();
        {
            // Unary
            Context.Logger.LogInformation($"Begin unary requests.");
            var unary = new UnaryBenchmarkScenario(channel, reporter);
            await unary.Run(itelation);
            Context.Logger.LogInformation($"Completed all unary requests.");

            // StreamingHub
            Context.Logger.LogInformation($"Begin Streaming requests.");
            await using var hub = new HubBenchmarkScenario(channel, reporter);
            await hub.Run(itelation);
            Context.Logger.LogInformation($"Completed Streaming requests.");
        }
        reporter.End();

        // output
        var benchJson = reporter.ToJson();
        Context.Logger.LogInformation(benchJson);

        // put json to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save("bench-magiconion-s3-bucket-5c7e45b", $"reports/{reporter.ReportId}", reporter.Name + ".json", benchJson, ct: Context.CancellationToken);
    }

    public async Task<string[]> ListReports(string prefix, string path = "")
    {
        if (string.IsNullOrEmpty(path))
            path = _path;

        // access s3 and List json from prefix
        var storage = StorageFactory.Create(Context.Logger);
        var reports = await storage.List(path, $"reports/{prefix}", Context.CancellationToken);
        foreach (var report in reports)
        {
            Context.Logger.LogInformation(report);
        }
        return reports;
    }

    public async Task<BenchReport[]> GetReports(string prefix, string path = "")
    {
        if (string.IsNullOrEmpty(path))
            path = _path;

        // access s3 and get jsons from prefix
        var storage = StorageFactory.Create(Context.Logger);
        var reportJsons = await storage.Get(path, $"reports/{prefix}", Context.CancellationToken);
        var reports = new List<BenchReport>();
        foreach (var json in reportJsons)
        {
            var report = JsonConvert.Deserialize<BenchReport>(json);
            reports.Add(report);
        }
        return reports.ToArray();
    }

    public async Task GenerateHtml(string prefix, string path = "", string htmlFileName = "index.html")
    {
        if (string.IsNullOrEmpty(path))
            path = _path;

        // access s3 and download json from prefix
        var reports = await GetReports(prefix, path);

        // generate html based on json data
        var htmlReporter = new HtmlBenchReporter();
        var htmlReport = htmlReporter.CreateReport(reports);
        var page = new BenchmarkReportPageTemplate()
        {
            Report = htmlReport,
        };
        var content = NormalizeNewLineLf(page.TransformText());

        // upload html report to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save("bench-magiconion-s3-bucket-5c7e45b", $"html/{htmlReport.Summary.Id}", htmlFileName, content, overwrite: true, Context.CancellationToken);
    }

    public async Task ListClients()
    {
        // todo: call ssm to list up client instanceids
        throw new NotImplementedException();
    }

    public async Task RunAllClient()
    {
        // todo: call ssm to execute Clients via CLI mode.
        // todo: call GenerateHtml to gene report
        throw new NotImplementedException();
    }

    public async Task UpdateServerBinary()
    {
        // todo: call ssm to update server binary
        // todo: start server via ssm
        throw new NotImplementedException();
    }

    private static string NormalizeNewLine(string content)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("\n", Environment.NewLine, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeNewLineLf(string content)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase);
    }
}
