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
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(builder, configurationName);
            return AddOpenTelemetry(builder, options);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            MagicOnionOpenTelemetryOptions options)
        {
            return AddOpenTelemetry(builder, options, null, null);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            Action<MagicOnionOpenTelemetryOptions, MagicOnionOpenTelemetryMeterFactoryOption> configureMeterFactory,
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerFactory,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(builder, configurationName);
            return AddOpenTelemetry(builder, options, configureMeterFactory, configureTracerFactory);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            MagicOnionOpenTelemetryOptions options,
            Action<MagicOnionOpenTelemetryOptions, MagicOnionOpenTelemetryMeterFactoryOption> configureMeterProvider,
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerProvider)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            builder.Services.AddSingleton(options);

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

                builder.Services.AddSingleton(meterFactoryOption.MetricExporter);
                if (meterFactoryOption.MeterLogger != null)
                {
                    builder.Services.AddSingleton<IMagicOnionLogger>(meterFactoryOption.MeterLogger.Invoke(MeterProvider.Default));
                }
            }

            // configure TracerFactory
            if (configureTracerProvider != null)
            {
                if (string.IsNullOrEmpty(options.MagicOnionActivityName))
                {
                    throw new NullReferenceException(nameof(options.MagicOnionActivityName));
                }

                var tracerFactory = builder.Services.AddOpenTelemetryTracing((provider, builder) =>
                {
                    // ActivitySourceName must match to TracerName.
                    builder.AddSource(options.MagicOnionActivityName);
                    configureTracerProvider?.Invoke(options, provider, builder);
                });
                builder.Services.AddSingleton(tracerFactory);

                // Avoid directly register ActivitySource to Singleton for easier identification.
                var activitySource = new ActivitySource(options.MagicOnionActivityName);
                var magicOnionActivitySources = new MagicOnionActivitySources(activitySource);
                builder.Services.AddSingleton(magicOnionActivitySources);
            }

            return builder;
        }

        private static MagicOnionOpenTelemetryOptions BindMagicOnionOpenTelemetryOptions(IMagicOnionServerBuilder builder, string configurationName)
        {
            var name = !string.IsNullOrEmpty(configurationName) ? configurationName : "MagicOnion:OpenTelemetry";
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>();
            var options = new MagicOnionOpenTelemetryOptions();
            config.GetSection(name).Bind(options);
            return options;
        }
    }
}
