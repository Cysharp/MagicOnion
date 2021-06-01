using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
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
            return AddOpenTelemetry(builder, options, null);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerFactory,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(builder, configurationName);
            return AddOpenTelemetry(builder, options, configureTracerFactory);
        }
        /// <summary>add MagicOnion Telemetry.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            MagicOnionOpenTelemetryOptions options,
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerProvider)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            builder.Services.AddSingleton(options);

            var serviceProvider = builder.Services.BuildServiceProvider();

            // configure TracerFactory
            if (configureTracerProvider != null)
            {
                var activityName = options.MagicOnionActivityName ?? throw new ArgumentNullException(nameof(options.MagicOnionActivityName));

                builder.Services.AddOpenTelemetryTracing(configure =>
                {
                    // ActivitySourceName must match to TracerName.
                    configure.AddSource(activityName);
                    configureTracerProvider?.Invoke(options, serviceProvider, configure);
                });

                // Avoid directly register ActivitySource to Singleton for easier identification.
                var activitySource = new ActivitySource(activityName);
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
