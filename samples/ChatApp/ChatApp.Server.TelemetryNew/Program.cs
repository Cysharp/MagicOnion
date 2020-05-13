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
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Export;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var exporterHost = config.GetValue<string>("PROMETHEUS_EXPORTER_HOST", "localhost");
            var exporterPort = config.GetValue<int>("PROMETHEUS_EXPORTER_PORT", 9181);
            var tracerHost = config.GetValue<string>("ZIPKIN_EXPORTER_HOST", "localhost");
            var tracerPort = config.GetValue<int>("ZIPKIN_EXPORTER_PORT", 9411);

            // MetricsServer for Prometheus pull model
            var exporterUrl = $"http://{exporterHost}:{exporterPort}/metrics/";
            var exporter = new PrometheusExporter(new PrometheusExporterOptions() { Url = exporterUrl });
            var metricsServer = new PrometheusExporterMetricsHttpServer(exporter);
            metricsServer.Start();
            Console.WriteLine($"Started Metrics Server on {exporterUrl}");

            // TracerServer for Zipkin push model
            var traceServer = TestHttpServer.RunServer(ProcessServerRequest, tracerHost, tracerPort);
            Console.WriteLine($"Started Tracer Server on http://{tracerHost}:{tracerPort}");

            // Metrics
            var processor = new UngroupedBatcher();
            var spanContext = default(SpanContext);
            var meterFactory = MeterFactory.Create(mb =>
            {
                mb.SetMetricProcessor(processor);
                mb.SetMetricExporter(exporter);
                mb.SetMetricPushInterval(TimeSpan.FromSeconds(10));
            });
            var meter = meterFactory.GetMeter("MyMeter");
            var counter = meter.CreateInt64Counter("MyCounter");

            var labels1 = new List<KeyValuePair<string, string>>();
            labels1.Add(new KeyValuePair<string, string>("dim1", "value1"));


            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalMinutes < 10)
            {
                // execute metrics
                counter.Add(spanContext, 100, meter.GetLabelSet(labels1));

                // Tracer           
                var requestId = Guid.NewGuid();
                using (var tracerFactory = TracerFactory.Create(builder => builder.UseZipkin(o =>
                {
                    o.ServiceName = "test-zipkin";
                    o.Endpoint = new Uri($"http://{tracerHost}:{tracerPort}/api/v2/spans?requestId={requestId}");
                })))
                {

                    var tracer = tracerFactory.GetTracer("zipkin-test");
                    
                    // execute tracer
                    using (tracer.StartActiveSpan($"parent", out var parent))
                    {
                        tracer.CurrentSpan.SetAttribute("key", 123);
                        tracer.CurrentSpan.AddEvent("test-event");

                        using (tracer.StartActiveSpan("child", out var child))
                        {
                            child.SetAttribute("key", "value");
                        }
                    }

                    await Task.Delay(1000);
                    var remaining = (10 * 60) - sw.Elapsed.TotalSeconds;
                    Console.WriteLine("Running and emitting metrics. Remaining time:" + (int)remaining + " seconds");
                }
            }

            traceServer.Dispose();
            metricsServer.Stop();

            //await MagicOnionHost.CreateDefaultBuilder()
            //    .ConfigureServices(collection =>
            //    {
            //        collection.AddSingleton<ITracer>(Tracing.Tracer);
            //        collection.AddSingleton<ISampler>(Samplers.AlwaysSample);
            //    })
            //    .UseMagicOnion()
            //    .ConfigureServices((hostContext, services) =>
            //    {
            //        services.Configure<MagicOnionHostingOptions>(options =>
            //        {
            //            options.Service.GlobalFilters.Add(new OpenTelemetryCollectorFilterAttribute());
            //            options.Service.GlobalStreamingHubFilters.Add(new OpenTelemetryHubCollectorFilterAttribute());
            //            options.Service.MagicOnionLogger = new OpenTelemetryCollectorLogger(Stats.StatsRecorder, Tags.Tagger, null);
            //        });
            //    })
            //    .RunConsoleAsync();
        }

        private static readonly ConcurrentDictionary<Guid, string> Responses = new ConcurrentDictionary<Guid, string>();

        static void ProcessServerRequest(HttpListenerContext context)
        {
            context.Response.StatusCode = 200;

            if (context.Request.QueryString.Count == 0)
            {
                foreach (var response in Responses)
                {
                    var body = Encoding.UTF8.GetBytes($"{response.Key}: {response.Value}");
                    context.Response.ContentType = "application/json";
                    context.Response.OutputStream.Write(body, 0, body.Length);
                }
                context.Response.OutputStream.Close();
                return;
            }

            using (var readStream = new StreamReader(context.Request.InputStream))
            {
                string requestContent = readStream.ReadToEnd();
                Responses.TryAdd(
                    Guid.Parse(context.Request.QueryString["requestId"]),
                    requestContent);

                context.Response.OutputStream.Close();
            }
        }

        internal class TestHttpServer
        {
            private class RunningServer : IDisposable
            {
                private readonly Task httpListenerTask;
                private readonly HttpListener listener;
                private readonly AutoResetEvent initialized = new AutoResetEvent(false);

                public RunningServer(Action<HttpListenerContext> action, string host, int port)
                {
                    this.listener = new HttpListener();

                    this.listener.Prefixes.Add($"http://{host}:{port}/");
                    this.listener.Start();

                    this.httpListenerTask = new Task(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var ctxTask = this.listener.GetContextAsync();

                                this.initialized.Set();

                                action(await ctxTask.ConfigureAwait(false));
                            }
                            catch (Exception ex)
                            {
                                if (ex is ObjectDisposedException // Listener was closed before we got into GetContextAsync.
                                    || (ex is HttpListenerException httpEx && httpEx.ErrorCode == 995)) // Listener was closed while we were in GetContextAsync.
                                {
                                    break;
                                }
                                throw;
                            }
                        }
                    });
                }

                public void Start()
                {
                    this.httpListenerTask.Start();
                    this.initialized.WaitOne();
                }

                public void Dispose()
                {
                    try
                    {
                        this.listener?.Stop();
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this exception just in case
                    }
                }
            }

            public static IDisposable RunServer(Action<HttpListenerContext> action, string host, int port)
            {
                RunningServer server = null;

                var retryCount = 5;
                while (retryCount > 0)
                {
                    try
                    {
                        server = new RunningServer(action, host, port);
                        server.Start();
                        break;
                    }
                    catch (HttpListenerException)
                    {
                        retryCount--;
                    }
                }

                return server;
            }
        }
    }
}
