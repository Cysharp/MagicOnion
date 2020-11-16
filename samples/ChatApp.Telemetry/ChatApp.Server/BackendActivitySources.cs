using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChatApp.Server
{
    public class BackendActivitySources
    {
        private readonly Dictionary<string, ActivitySource> activitySourceCache;

        public BackendActivitySources(ActivitySource[] activitySources)
        {
            if (activitySources == null)
                throw new ArgumentNullException(nameof(activitySources));

            activitySourceCache = new Dictionary<string, ActivitySource>();
            foreach (var activitySource in activitySources)
            {
                activitySourceCache.TryAdd(activitySource.Name, activitySource);
            }
        }

        public ActivitySource Get(string name)
        {
            return activitySourceCache.TryGetValue(name, out var activitySource)
                ? activitySource
                : null;
        }
    }
}
