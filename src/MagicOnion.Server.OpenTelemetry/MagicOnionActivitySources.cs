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
        /// MagicOnion's <see cref="System.Diagnostics.ActivitySource"/>
        /// </summary>
        public ActivitySource Current => activitySource;

        public MagicOnionActivitySources(ActivitySource activitySource)
        {
            this.activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        }
    }
}
