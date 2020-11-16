using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MagicOnion.Server.OpenTelemetry
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
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerProvider)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            services.AddSingleton(options);

            // configure MeterFactory
            if (configureMeterProvider != null)
            {
                var meterFactoryOption = new MagicOnionOpenTelemetryMeterFactoryOption();
                configureMeterProvider(options, meterFactoryOption);

                MeterProvider.SetDefault(Sdk.CreateMeterProviderBuilder()
                    .SetProcessor(meterFactoryOption.MetricProcessor)
                    .SetExporter(meterFactoryOption.MetricExporter)
                    .SetPushInterval(meterFactoryOption.MetricPushInterval)
                    .Build());

                services.AddSingleton(meterFactoryOption.MetricExporter);
                services.AddSingleton(MeterProvider.Default);
            }

            // configure TracerFactory
            if (configureTracerProvider != null)
            {
                if (string.IsNullOrEmpty(options.ServiceName))
                {
                    throw new NullReferenceException(nameof(options.ServiceName));
                }

                var tracerFactory = services.AddOpenTelemetryTracing((provider, builder) =>
                {
                    // ActivitySourceName must match to TracerName.
                    builder.AddSource(options.ServiceName);
                    configureTracerProvider?.Invoke(options, provider, builder);
                });
                services.AddSingleton(tracerFactory);
                services.AddSingleton(new ActivitySource(options.ServiceName));
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
