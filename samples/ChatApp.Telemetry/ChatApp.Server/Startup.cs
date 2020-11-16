using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ChatApp.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(); // MagicOnion depends on ASP.NET Core gRPC service.
            services.AddMagicOnion(options =>
            {
                options.GlobalFilters.Add(new OpenTelemetryCollectorFilterFactoryAttribute());
                options.GlobalStreamingHubFilters.Add(new OpenTelemetryHubCollectorFilterFactoryAttribute());
            });
            services.AddSingleton<IMagicOnionLogger>(sp => new OpenTelemetryCollectorLogger(sp.GetRequiredService<MeterProvider>(), version: "0.8.0-beta.1"));

            services.AddMagicOnionOpenTelemetry((options, meterOptions) =>
                {
                    // open-telemetry with Prometheus exporter
                    meterOptions.MetricExporter = new PrometheusExporter(new PrometheusExporterOptions() { Url = options.MetricsExporterEndpoint });
                },
                (options, provider, tracerBuilder) =>
                {
                    // Switch between Jaeger/Zipkin by setting UseExporter in appsettings.json.
                    var exporter = this.Configuration.GetValue<string>("UseExporter").ToLowerInvariant();
                    switch (exporter)
                    {
                        case "jaeger":
                            tracerBuilder
                                .AddAspNetCoreInstrumentation()
                                .AddJaegerExporter(jaegerOptions =>
                                {
                                    jaegerOptions.ServiceName = options.ServiceName;
                                    jaegerOptions.AgentHost = this.Configuration.GetValue<string>("Jaeger:Host");
                                    jaegerOptions.AgentPort = this.Configuration.GetValue<int>("Jaeger:Port");
                                });
                            break;
                        case "zipkin":
                            tracerBuilder
                                .AddAspNetCoreInstrumentation()
                                .AddZipkinExporter(zipkinOptions =>
                                {
                                    zipkinOptions.ServiceName = options.ServiceName;
                                    zipkinOptions.Endpoint = new Uri(this.Configuration.GetValue<string>("Zipkin:Endpoint"));
                                });
                            break;
                        default:
                            // ConsoleExporter will show current tracer activity
                            tracerBuilder
                                .AddAspNetCoreInstrumentation()
                                .AddConsoleExporter();
                            break;
                    }
                });

            services.AddHostedService<PrometheusExporterMetricsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService();
            });
        }
    }
}
