using Benchmark.ClientLib;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
    var hostAddress = Environment.GetEnvironmentVariable("BENCHCLIENT_HOSTADDRESS") ?? "http://localhost:8080";
    await builder.RunConsoleAppFrameworkWebHostingAsync(hostAddress);
}
else
{
    await builder.RunConsoleAppFrameworkAsync(args);
}

public class BenchmarkRunner : ConsoleAppBase
{
    private readonly string _path;
    public BenchmarkRunner()
    {
        _path = Environment.GetEnvironmentVariable("BENCHCLIENT_S3BUCKET") ?? throw new ArgumentNullException("Environment variable 'BENCHCLIENT_S3BUCKET' is not defined.");
    }

    /// <summary>
    /// Run Unary and Hub Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchAll(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, iter, Context.Logger, Context.CancellationToken);
        await benchmarker.BenchAll(hostAddress, reportId);
    }

    /// <summary>
    /// Run Unary Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchUnary(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, iter, Context.Logger, Context.CancellationToken);
        await benchmarker.BenchUnary(hostAddress, reportId);
    }

    /// <summary>
    /// Run Hub Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchHub(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, iter, Context.Logger, Context.CancellationToken);
        await benchmarker.BenchHub(hostAddress, reportId);
    }

    /// <summary>
    /// Run Hub Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchLongRunHub(int waitMilliseconds, string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, iter, Context.Logger, Context.CancellationToken);
        await benchmarker.BenchLongRunHub(waitMilliseconds, true, hostAddress, reportId);
    }

    /// <summary>
    /// Run Grpc Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchGrpc(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, iter, Context.Logger, Context.CancellationToken);
        await benchmarker.BenchGrpc(hostAddress, reportId);
    }

    /// <summary>
    /// List Reports
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task ListReports(string reportId)
    {
        var benchmarker = new Benchmarker(_path, null, Context.Logger, Context.CancellationToken);
        await benchmarker.ListReports(reportId);
    }

    /// <summary>
    /// Get Report
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task GetReports(string reportId)
    {
        var benchmarker = new Benchmarker(_path, null, Context.Logger, Context.CancellationToken);
        await benchmarker.GetReports(reportId);
    }

    /// <summary>
    /// Generate Report Html
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="htmlFileName"></param>
    /// <returns></returns>
    public async Task GenerateHtml(string reportId, bool generateDetail, string htmlFileName = "index.html")
    {
        var benchmarker = new Benchmarker(_path, null, Context.Logger, Context.CancellationToken);
        await benchmarker.GenerateHtml(reportId, generateDetail, htmlFileName);
    }

    public async Task ListClients()
    {
        var benchmarker = new Benchmarker(_path, null, Context.Logger, Context.CancellationToken);
        await benchmarker.ListClients();
    }

    public async Task RunAllClient(int processCount, string iterations = "256,1024,4096,16384", string benchCommand = "benchall", string hostAddress = "http://localhost:5000", string reportId = "")
    {
        var benchmarker = new Benchmarker(_path, null, Context.Logger, Context.CancellationToken);
        await benchmarker.RunAllClient(processCount, iterations, benchCommand, hostAddress, reportId);
    }

    public async Task CancelCommands()
    {
        var benchmarker = new Benchmarker(_path, null, Context.Logger, Context.CancellationToken);
        await benchmarker.CancelCommands();
    }
}
