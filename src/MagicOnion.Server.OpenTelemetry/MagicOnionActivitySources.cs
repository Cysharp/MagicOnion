using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// ActivitySource for MagicOnion OpenTelemetry
    /// </summary>
    public class MagicOnionActivitySources
    {
        private readonly ActivitySource activitySource;

        /// <summary>
        /// <see cref="System.Diagnostics.ActivitySource"/> used for MagicOnion
        /// </summary>
        public ActivitySource Current => activitySource;

        public MagicOnionActivitySources(ActivitySource activitySource)
        {
            this.activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        }
    }
}
