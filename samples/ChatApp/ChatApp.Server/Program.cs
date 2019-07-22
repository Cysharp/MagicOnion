using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Hosting;
using MagicOnion.OpenTelemetry;
using MagicOnion.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Stats;
using OpenTelemetry.Tags;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Sampler;

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var exporterHost = config.GetValue<string>("PROMETHEUS_EXPORTER_HOST", "localhost");
            var exporterPort = config.GetValue<string>("PROMETHEUS_EXPORTER_PORT", "9182");
            var exporter = new PrometheusExporter(
                new PrometheusExporterOptions()
                {
                    // put exporterHost "+" to listen to all hostnames and 0.0.0.0.
                    Url = $"http://{exporterHost}:{exporterPort}/metrics/",
                },
                Stats.ViewManager);
            exporter.Start();

            await MagicOnionHost.CreateDefaultBuilder(useSimpleConsoleLogger: true)
                .ConfigureServices(collection =>
                {
                    collection.AddSingleton<ITracer>(Tracing.Tracer);
                    collection.AddSingleton<ISampler>(Samplers.AlwaysSample);
                })
                .UseMagicOnion(
                    new MagicOnionOptions()
                    {
                        GlobalFilters = new[] { new OpenTelemetryCollectorFilter(null) },
                        GlobalStreamingHubFilters = new[] { new OpenTelemetryHubCollectorFilter(null) },
                        MagicOnionLogger = new OpenTelemetryCollectorLogger(Stats.StatsRecorder, Tags.Tagger)
                    },
                    new ServerPort(config.GetValue<string>("MAGICONION_HOST", "127.0.0.1"), 12345, ServerCredentials.Insecure))
                .RunConsoleAsync();
        }
    }
}
