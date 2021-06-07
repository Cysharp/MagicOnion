using MagicOnion.Server;
using MagicOnion.Server.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
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
            // OpenTelemetry Require set of Listener + Instrumentation.
            // - Listener will add via services.AddMagicOnion().UseOpenTelemetry()
            // - Instrumentation will add via TraceProviderBuilder.AddMagicOnionInstrumentation()
            services.AddGrpc(); // MagicOnion depends on ASP.NET Core gRPC service.
            services.AddMagicOnion(options =>
                {
                    options.GlobalFilters.Add(new MagicOnionOpenTelemetryTracerFilterFactoryAttribute());
                    options.GlobalStreamingHubFilters.Add(new MagicOnionOpenTelemetryStreamingTracerFilterFactoryAttribute());

                    // Exception Filter enable Telemetry to know which gRPC Error happen.
                    options.GlobalFilters.Add(new ExceptionFilterFactoryAttribute());
                })
                .AddOpenTelemetry(); // Listen OpenTelemetry Activity

            // Configure OpenTelemetry as usual.
            services.AddOpenTelemetryTracing(configure =>
            {
                var exporter = this.Configuration.GetValue<string>("UseExporter").ToLowerInvariant();
                switch (exporter)
                {
                    // Switch between Jaeger/Zipkin by setting UseExporter in appsettings.json.
                    case "jaeger":
                        var jo = new OpenTelemetry.Exporter.JaegerExporterOptions();
                        Configuration.GetSection("Jaeger").Bind(jo);
                        configure
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ChatApp.Server"))
                            .AddAspNetCoreInstrumentation()
                            .AddMagicOnionInstrumentation() // enable MagicOnion instrumentation
                            .AddJaegerExporter();
                        services.Configure<OpenTelemetry.Exporter.JaegerExporterOptions>(Configuration.GetSection("Jaeger"));
                        break;
                    case "zipkin":
                        var zo = new OpenTelemetry.Exporter.ZipkinExporterOptions();
                        Configuration.GetSection("Zipkin").Bind(zo);
                        configure
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ChatApp.Server"))
                            .AddAspNetCoreInstrumentation()
                            .AddMagicOnionInstrumentation() // enable MagicOnion instrumentation
                            .AddZipkinExporter();
                        services.Configure<OpenTelemetry.Exporter.ZipkinExporterOptions>(this.Configuration.GetSection("Zipkin"));
                        break;
                    default:
                        // ConsoleExporter will show current tracer activity
                        configure
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ChatApp.Server"))
                            .AddAspNetCoreInstrumentation()
                            .AddMagicOnionInstrumentation() // enable MagicOnion instrumentation
                            .AddConsoleExporter();
                        services.Configure<OpenTelemetry.Instrumentation.AspNetCore.AspNetCoreInstrumentationOptions>(this.Configuration.GetSection("AspNetCoreInstrumentation"));
                        services.Configure<OpenTelemetry.Instrumentation.AspNetCore.AspNetCoreInstrumentationOptions>(options =>
                        {
                            options.Filter = (req) => req.Request?.Host != null;
                        });
                        break;
                }
            });

            // additional Tracer for user's own service.
            // This means we cannot use AddOpenTelemetryTracing(), but require Sdk.CreateTracerProviderBuilder() instead.
            services.AddAdditionalTracer(Configuration);
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

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }

    public static class ServiceCollectionExtentions
    {
        // If you want use multiple trace service, Do not use AddOpenTelemetryTracing, but use multiple Sdk.CreateTracerProviderBuilder() instead.
        // ref: https://github.com/open-telemetry/opentelemetry-dotnet/issues/2040
        public static void AddAdditionalTracer(this IServiceCollection services, IConfiguration configuration)
        {
            var exporter = configuration.GetValue<string>("UseExporter").ToLowerInvariant();
            foreach (var service in BackendActivitySources.ExtraActivitySourceNames)
            {
                switch (exporter)
                {
                    case "jaeger":
                        var jo = new OpenTelemetry.Exporter.JaegerExporterOptions();
                        configuration.GetSection("Jaeger").Bind(jo);
                        Sdk.CreateTracerProviderBuilder()
                            .AddSource(service)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(service))
                            .AddJaegerExporter(o =>
                            {
                                o.AgentHost = jo.AgentHost;
                                o.AgentPort = jo.AgentPort;
                            })
                            .Build();
                        break;
                    case "zipkin":
                        var zo = new OpenTelemetry.Exporter.ZipkinExporterOptions();
                        configuration.GetSection("Zipkin").Bind(zo);
                        Sdk.CreateTracerProviderBuilder()
                            .AddSource(service)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(service))
                            .AddZipkinExporter(o => o.Endpoint = zo.Endpoint)
                            .Build();
                        break;
                    default:
                        // ConsoleExporter will show current tracer activity
                        Sdk.CreateTracerProviderBuilder()
                            .AddSource(service)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(service))
                            .AddConsoleExporter()
                            .Build();
                        break;
                }
            }
        }
    }
}
