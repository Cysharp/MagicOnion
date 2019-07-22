using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Hosting;
using MagicOnion.OpenTelemetry;
using MagicOnion.Server;
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

            var exporter = new PrometheusExporter(
                new PrometheusExporterOptions()
                {
                    Url = "http://localhost:9185/metrics/",  // "+" is a wildcard used to listen to all hostnames
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
                    new ServerPort("localhost", 12345, ServerCredentials.Insecure))
                .RunConsoleAsync();
        }
    }
}
