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

            services.AddMagicOnionOpenTelemetry((options, meterOptions) =>
                {
                    // open-telemetry with Prometheus exporter
                    meterOptions.MetricExporter = new PrometheusExporter(new PrometheusExporterOptions() { Url = options.MetricsExporterEndpoint });
                },
                (options, provider, tracerBuilder) =>
                {
                    // open-telemetry with Zipkin exporter
                    tracerBuilder.AddZipkinExporter(o =>
                    {
                        o.ServiceName = "ChatApp.Server";
                        o.Endpoint = new Uri(options.TracerExporterEndpoint);
                    });
                    // ConsoleExporter will show current tracer activity
                    tracerBuilder.AddConsoleExporter();
                });

            services.AddHostedService<PrometheusExporterMetricsService>();

            var meterProvider = services.BuildServiceProvider().GetService<MeterProvider>();
            services.AddMagicOnion(options =>
            {
                options.GlobalFilters.Add(new OpenTelemetryCollectorFilterFactoryAttribute());
                options.GlobalStreamingHubFilters.Add(new OpenTelemetryHubCollectorFilterFactoryAttribute());
                options.MagicOnionLogger = new OpenTelemetryCollectorLogger(meterProvider, version: "0.5.0-beta.2");
            });
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
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService();
            });
        }
    }
}
