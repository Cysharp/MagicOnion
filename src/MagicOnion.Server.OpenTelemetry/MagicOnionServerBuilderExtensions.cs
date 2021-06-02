using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using System.Diagnostics;
using MagicOnion.Server.OpenTelemetry.Internal;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>MagicOnion extensions for Microsoft.Extensions.Hosting classes</summary>
    public static class MagicOnionServerBuilderExtensions
    {
        /// <summary>Configures OpenTelemetry to listen for the Activities created by the MagicOnion Filter.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(builder, configurationName);
            return AddOpenTelemetry(builder, options);
        }
        /// <summary>Configures OpenTelemetry to listen for the Activities created by the MagicOnion Filter.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            MagicOnionOpenTelemetryOptions options)
        {
            return AddOpenTelemetry(builder, options, null);
        }
        /// <summary>Configures OpenTelemetry to listen for the Activities created by the MagicOnion Filter.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerFactory,
            string configurationName = "")
        {
            var options = BindMagicOnionOpenTelemetryOptions(builder, configurationName);
            return AddOpenTelemetry(builder, options, configureTracerFactory);
        }
        /// <summary>Configures OpenTelemetry to listen for the Activities created by the MagicOnion Filter.</summary>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder,
            MagicOnionOpenTelemetryOptions options,
            Action<MagicOnionOpenTelemetryOptions, IServiceProvider, TracerProviderBuilder> configureTracerProvider)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var serviceProvider = builder.Services.BuildServiceProvider();
            var activityName = MagicOnionInstrumentation.ActivitySourceName;

            // Configure OpenTelemetry Tracer
            if (configureTracerProvider != null)
            {
                builder.Services.AddOpenTelemetryTracing(configure =>
                {
                    // ActivitySourceName must match to TracerName.
                    configure.AddSource(activityName);
                    configureTracerProvider?.Invoke(options, serviceProvider, configure);
                });
            }

            // auto listen ActivitySource
            var activitySource = new ActivitySource(activityName, MagicOnionInstrumentation.Version.ToString());

            // DI
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton(new MagicOnionActivitySources(activitySource));

            return builder;
        }

        /// <summary>
        /// Generate MagicOnionTelemetryOptions and configure if configuration exists.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configurationName"></param>
        /// <returns></returns>
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
