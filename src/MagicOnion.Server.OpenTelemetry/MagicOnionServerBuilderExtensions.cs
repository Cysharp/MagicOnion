using System;
using System.Diagnostics;
using MagicOnion.Server.OpenTelemetry.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>MagicOnion extensions for Microsoft.Extensions.Hosting classes</summary>
    public static class MagicOnionServerBuilderExtensions
    {
        /// <summary>
        /// Configures OpenTelemetry to listen for the Activities created by the MagicOnion Filter.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configurationName"></param>
        /// <returns></returns>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder, string configurationName = "MagicOnion:OpenTelemetry")
        {
            var options = CreateDefaultOptions(builder, configurationName);
            return AddOpenTelemetry(builder, options);
        }

        /// <summary>
        /// Configures OpenTelemetry to listen for the Activities created by the MagicOnion Filter.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IMagicOnionServerBuilder AddOpenTelemetry(this IMagicOnionServerBuilder builder, MagicOnionOpenTelemetryOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            // listen ActivitySource
            var activitySource = new ActivitySource(MagicOnionInstrumentation.ActivitySourceName, MagicOnionInstrumentation.Version.ToString());

            // DI
            builder.Services.TryAddSingleton(options);
            builder.Services.TryAddSingleton(new MagicOnionActivitySources(activitySource));

            return builder;
        }

        /// <summary>
        /// Generate MagicOnionTelemetryOptions and configure if configuration exists.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configurationName"></param>
        /// <returns></returns>
        private static MagicOnionOpenTelemetryOptions CreateDefaultOptions(IMagicOnionServerBuilder builder, string configurationName)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>();
            var options = new MagicOnionOpenTelemetryOptions();

            var section = config.GetSection(configurationName);
            if (section == null)
            {
                throw new ArgumentOutOfRangeException($"{configurationName} not exists in {nameof(IConfiguration)}.");
            }
            section.Bind(options);
            return options;
        }
    }
}
