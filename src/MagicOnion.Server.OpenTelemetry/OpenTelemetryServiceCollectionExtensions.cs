using System;
using OpenTelemetry.Metrics.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace.Configuration;
using Microsoft.Extensions.Configuration;

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
            Action<MagicOnionOpenTelemetryOptions, TracerBuilder> configureTracerFactory,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(services, configurationName);
            return AddMagicOnionOpenTelemetry(services, options, configureMeterFactory, configureTracerFactory);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IServiceCollection AddMagicOnionOpenTelemetry(this IServiceCollection services, 
            MagicOnionOpenTelemetryOptions options, 
            Action<MagicOnionOpenTelemetryOptions, MagicOnionOpenTelemetryMeterFactoryOption> configureMeterFactory, 
            Action<MagicOnionOpenTelemetryOptions, TracerBuilder> configureTracerFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.ServiceName)) throw new ArgumentNullException($"{nameof(options)}.{nameof(options.ServiceName)}");

            services.AddSingleton(options);

            // configure MeterFactory
            if (configureMeterFactory != null)
            {
                var meterFactoryOption = new MagicOnionOpenTelemetryMeterFactoryOption();
                configureMeterFactory(options, meterFactoryOption);

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
                var tracerFactory = TracerFactory.Create(tracerBuilder => configureTracerFactory(options, tracerBuilder));
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
