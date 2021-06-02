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
            services.AddGrpc(); // MagicOnion depends on ASP.NET Core gRPC service.
            services.AddMagicOnion(options =>
                {
                    options.GlobalFilters.Add(new MagicOnionOpenTelemetryTracerFilterFactoryAttribute());
                    options.GlobalStreamingHubFilters.Add(new MagicOnionOpenTelemetryStreamingTracerFilterFactoryAttribute());

                    // Exception Filter is inside telemetry
                    options.GlobalFilters.Add(new ExceptionFilterFactoryAttribute());
                })
                .AddOpenTelemetry((options, provider, tracerBuilder) =>
                {
                    // Switch between Jaeger/Zipkin by setting UseExporter in appsettings.json.
                    var exporter = this.Configuration.GetValue<string>("UseExporter").ToLowerInvariant();
                    switch (exporter)
                    {
                        case "jaeger":
                            tracerBuilder
                                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(options.ServiceName))
                                .AddAspNetCoreInstrumentation()
                                .AddJaegerExporter();
                            // https://github.com/open-telemetry/opentelemetry-dotnet/blob/21c1791e8e2bdb292ff87b044d2b92e9851dbab9/src/OpenTelemetry.Exporter.Jaeger/JaegerExporterOptions.cs
                            services.Configure<OpenTelemetry.Exporter.JaegerExporterOptions>(Configuration.GetSection("Jaeger"));
                            break;
                        case "zipkin":
                            tracerBuilder
                                .AddSource(BackendActivitySources.ExtraActivitySourceNames)
                                .AddAspNetCoreInstrumentation()
                                .AddZipkinExporter();
                            // https://github.com/open-telemetry/opentelemetry-dotnet/blob/21c1791e8e2bdb292ff87b044d2b92e9851dbab9/src/OpenTelemetry.Exporter.Zipkin/ZipkinExporterOptions.cs
                            services.Configure<OpenTelemetry.Exporter.ZipkinExporterOptions>(this.Configuration.GetSection("Zipkin"));
                            break;
                        default:
                            // ConsoleExporter will show current tracer activity
                            tracerBuilder
                                .AddSource(BackendActivitySources.ExtraActivitySourceNames)
                                .AddAspNetCoreInstrumentation()
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
        public static void AddAdditionalTracer(this IServiceCollection services, IConfiguration configuration)
        {
            var exporter = configuration.GetValue<string>("UseExporter").ToLowerInvariant();
            foreach (var service in BackendActivitySources.ExtraActivitySourceNames)
            {
                // TODO: Known issue for Linux
                // - Windows work as expected.
                // - Linux has issues.
                //     - Linux could not identify ServiceName on Zipkin.
                //     - Linux can not co-work with Sdk.CreateTracerProviderBuilder() and IServiceCollection.AddOpenTelemetryTracing(). Only AddOpenTelemetryTracing() work.
                //          - work around, if you can accept same service name, IServiceCollection.AddOpenTelemetryTracing().AddSource(mysql, redis and other) instread of Sdk.CreateTracerProviderBuilder().
                switch (exporter)
                {
                    case "jaeger":
                        Sdk.CreateTracerProviderBuilder()
                            .AddSource(service)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(service))
                            .AddJaegerExporter()
                            .Build();
                        break;
                    case "zipkin":
                        Sdk.CreateTracerProviderBuilder()
                            .AddSource(service)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(service))
                            .AddZipkinExporter()
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
