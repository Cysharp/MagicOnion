using Benchmark.Client;
using Benchmark.Client.Reports;
using ConsoleAppFramework;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
    public async Task BenchAll(string hostAddress = "http://localhost:5000", int itelation = 10000, string id = "")
    {
        if (string.IsNullOrEmpty(id))
            id = DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();

        // Connect to the server using gRPC channel.
        var channel = GrpcChannel.ForAddress(hostAddress);
        var reporter = new BenchReporter(id);

        await Task.Delay(TimeSpan.FromSeconds(3));
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

        // output
        var benchJson = reporter.OutputJson();
        Context.Logger.LogInformation(benchJson);
    }
}
