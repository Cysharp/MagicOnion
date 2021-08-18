using System;
using OpenTelemetry.Trace;
using MagicOnion.Server.OpenTelemetry.Internal;

namespace MagicOnion.Server.OpenTelemetry
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddMagicOnionInstrumentation(this TracerProviderBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddSource(MagicOnionInstrumentation.ActivitySourceName);
        }
    }
}
