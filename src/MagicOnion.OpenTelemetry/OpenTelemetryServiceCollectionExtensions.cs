using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MagicOnion.OpenTelemetry
{
    /// <summary>MagicOnion extensions for Microsoft.Extensions.Hosting classes</summary>
    public static class OpenTelemetryServiceCollectionExtensions
    {
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOpenTelemetry(this IServiceCollection services, 
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(services, configurationName);
            return AddMagicOnionOpenTelemetry(services, options);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOpenTelemetry(this IServiceCollection services, 
            MagicOnionOpenTelemetryOptions options)
        {
            return AddMagicOnionOpenTelemetry(services, options, null, null);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOpenTelemetry(this IServiceCollection services, 
            Action<MagicOnionOpenTelemetryOptions, MagicOnionOpenTelemetryMeterFactoryOption> configureMeterFactory, 
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerFactory,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(services, configurationName);
            return AddMagicOnionOpenTelemetry(services, options, configureMeterFactory, configureTracerFactory);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOpenTelemetry(this IServiceCollection services, 
            MagicOnionOpenTelemetryOptions options, 
            Action<MagicOnionOpenTelemetryOptions, MagicOnionOpenTelemetryMeterFactoryOption> configureMeterProvider, 
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.ServiceName)) throw new ArgumentNullException($"{nameof(options)}.{nameof(options.ServiceName)}");

            services.AddSingleton(options);

            // configure MeterFactory
            if (configureMeterProvider != null)
            {
                var meterFactoryOption = new MagicOnionOpenTelemetryMeterFactoryOption();
                configureMeterProvider(options, meterFactoryOption);

                MeterProvider.SetDefault(Sdk.CreateMeterProvider(builder =>
                {
                    builder.SetMetricProcessor(meterFactoryOption.MetricProcessor);
                    builder.SetMetricExporter(meterFactoryOption.MetricExporter);
                    builder.SetMetricPushInterval(meterFactoryOption.MetricPushInterval);
                }));

                services.AddSingleton(meterFactoryOption.MetricExporter);
                services.AddSingleton(MeterProvider.Default);
            }

            // configure TracerFactory
            if (configureTracerFactory != null)
            {
                var tracerFactory = services.AddOpenTelemetry((provider, builder) => {
                    if (!string.IsNullOrEmpty(options.ActivitySourceName))
                    {
                        builder.AddActivitySource(options.ActivitySourceName);
                    }
                    configureTracerFactory(options, provider, builder);
                });
                services.AddSingleton(tracerFactory);
            }

            return services;
        }

        private static MagicOnionOpenTelemetryOptions BindMagicOnionOpenTelemetryOptions(IServiceCollection services, string configurationName)
        {
            var name = !string.IsNullOrEmpty(configurationName) ? configurationName : "MagicOnion:OpenTelemetry";
            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>();
            var options = new MagicOnionOpenTelemetryOptions();
            config.GetSection(name).Bind(options);
            return options;
        }
    }
}
