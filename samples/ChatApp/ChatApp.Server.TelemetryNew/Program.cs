using Grpc.Core;
using MagicOnion.Hosting;
using MagicOnion.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics;
using OpenTelemetry.Metrics.Configuration;
using OpenTelemetry.Metrics.Export;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var exporterHost = config.GetValue<string>("PROMETHEUS_EXPORTER_HOST", "localhost");
            var exporterPort = config.GetValue<string>("PROMETHEUS_EXPORTER_PORT", "9181");
            var exporter = new PrometheusExporter(
                new PrometheusExporterOptions()
                {
                    Url = $"http://{exporterHost}:{exporterPort}/metrics/",
                });
            var metricsServer = new PrometheusExporterMetricsHttpServer(exporter);
            metricsServer.Start();

            // prepare
            var processor = new UngroupedBatcher();
            var spanContext = default(SpanContext);
            var factory = MeterFactory.Create(mb => 
            {
                mb.SetMetricProcessor(processor);
                mb.SetMetricExporter(exporter);
                mb.SetMetricPushInterval(TimeSpan.FromSeconds(10));
            });
            var meter = factory.GetMeter("MyMeter");
            var counter = meter.CreateInt64Counter("MyCounter");

            var labels1 = new List<KeyValuePair<string, string>>();
            labels1.Add(new KeyValuePair<string, string>("dim1", "value1"));

            // collect
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalMinutes < 10)
            {
                counter.Add(spanContext, 100, meter.GetLabelSet(labels1));

                await Task.Delay(1000);
                var remaining = (10 * 60) - sw.Elapsed.TotalSeconds;
                Console.WriteLine("Running and emitting metrics. Remaining time:" + (int)remaining + " seconds");
            }

            metricsServer.Stop();

            await MagicOnionHost.CreateDefaultBuilder()
                //.ConfigureServices(collection =>
                //{
                //    collection.AddSingleton<ITracer>(Tracing.Tracer);
                //    collection.AddSingleton<ISampler>(Samplers.AlwaysSample);
                //})
                .UseMagicOnion()
                //.ConfigureServices((hostContext, services) =>
                //{
                //    services.Configure<MagicOnionHostingOptions>(options =>
                //    {
                //        options.Service.GlobalFilters.Add(new OpenTelemetryCollectorFilterAttribute());
                //        options.Service.GlobalStreamingHubFilters.Add(new OpenTelemetryHubCollectorFilterAttribute());
                //        options.Service.MagicOnionLogger = new OpenTelemetryCollectorLogger(Stats.StatsRecorder, Tags.Tagger, null);
                //    });
                //})
                .RunConsoleAsync();
        }
    }
}
