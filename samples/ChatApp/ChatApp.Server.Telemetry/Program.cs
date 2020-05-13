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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            // open-telemetry with Prometheus exporter
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var exporterHost = config.GetValue<string>("PROMETHEUS_EXPORTER_HOST", "127.0.0.1");
            var exporterPort = config.GetValue<string>("PROMETHEUS_EXPORTER_PORT", "9181");
            var exporter = new PrometheusExporter(
                new PrometheusExporterOptions()
                {
                    Url = $"http://{exporterHost}:{exporterPort}/metrics/",
                },
                Stats.ViewManager);
            exporter.Start();

            await MagicOnionHost.CreateDefaultBuilder()
                .ConfigureServices(collection =>
                {
                    collection.AddSingleton<ITracer>(Tracing.Tracer);
                    collection.AddSingleton<ISampler>(Samplers.AlwaysSample);
                })
                .UseMagicOnion()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        options.Service.GlobalFilters.Add(new OpenTelemetryCollectorFilterAttribute());
                        options.Service.GlobalStreamingHubFilters.Add(new OpenTelemetryHubCollectorFilterAttribute());
                        options.Service.MagicOnionLogger = new OpenTelemetryCollectorLogger(Stats.StatsRecorder, Tags.Tagger, null);
                    });
                })
                .RunConsoleAsync();
        }
    }
}
