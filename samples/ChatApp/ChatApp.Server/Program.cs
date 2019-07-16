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

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var options = new PrometheusExporterOptions { Url = "http://localhost:9182/metrics/" };
            var exporter = new PrometheusExporter(options, Stats.ViewManager);
            exporter.Start();

            await MagicOnionHost.CreateDefaultBuilder(useSimpleConsoleLogger: true)
                .ConfigureServices(collection =>
                {
                    collection.AddSingleton<ITracer>(Tracing.Tracer);
                })
                .UseMagicOnion(
                    new MagicOnionOptions()
                    {
                        GlobalFilters = new[] { new OpenTelemetryCollectorFilter(null) },
                        MagicOnionLogger = new OpenTelemetryCollectorLogger(Stats.StatsRecorder, Tags.Tagger)
                    },
                    new ServerPort("localhost", 12345, ServerCredentials.Insecure))
                .RunConsoleAsync();
        }
    }
}
