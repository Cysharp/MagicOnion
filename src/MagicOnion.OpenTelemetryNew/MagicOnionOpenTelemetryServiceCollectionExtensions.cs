using Microsoft.Extensions.Hosting;
using System;
using OpenTelemetry.Metrics.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace.Configuration;
using Microsoft.Extensions.Options;

namespace MagicOnion.OpenTelemetry
{
    /// <summary>MagicOnion extensions for Microsoft.Extensions.Hosting classes</summary>
    public static class MagicOnionOpenTelemetryServiceCollectionExtensions
    {
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOptnTelemetry(this IServiceCollection services)
        {
            return AddMagicOnionOptnTelemetry(services, new MagicOnionOpenTelemetryOptions());
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOptnTelemetry(this IServiceCollection services, MagicOnionOpenTelemetryOptions options)
        {
            return AddMagicOnionOptnTelemetry(services, options, null, null);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOptnTelemetry(this IServiceCollection services, Action<MagicOnionOpenTelemetryMeterFactoryOption> configureMeterFactory, Action<TracerBuilder> configureTracerFactory)
        {
            return AddMagicOnionOptnTelemetry(services, new MagicOnionOpenTelemetryOptions(), configureMeterFactory, configureTracerFactory);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOptnTelemetry(this IServiceCollection services, MagicOnionOpenTelemetryOptions options, Action<MagicOnionOpenTelemetryMeterFactoryOption> configureMeterFactory, Action<TracerBuilder> configureTracerFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.ServiceName)) throw new ArgumentNullException($"{nameof(options)}.{nameof(options.ServiceName)}");

            // configure MeterFactory
            if (configureMeterFactory != null)
            {
                var meterFactoryOption = new MagicOnionOpenTelemetryMeterFactoryOption();
                configureMeterFactory(meterFactoryOption);

                var meterFactory = MeterFactory.Create(mb =>
                {
                    mb.SetMetricProcessor(meterFactoryOption.MetricProcessor);
                    mb.SetMetricExporter(meterFactoryOption.MetricExporter);
                    mb.SetMetricPushInterval(meterFactoryOption.MetricPushInterval);
                });

                services.AddSingleton(meterFactoryOption.MetricExporter);
                services.AddSingleton(meterFactory);
            }

            // configure TracerFactory
            if (configureTracerFactory != null)
            {
                var tracerFactory = TracerFactory.Create(tracerBuilder => configureTracerFactory(tracerBuilder));
                services.AddSingleton(tracerFactory);
            }

            services.AddSingleton(options);

            return services;
        }
    }
}
