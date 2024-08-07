namespace PerformanceTest.Shared.Reporting;

public class MetricsTagCache<T>(int maxCacheSizePerType = MetricsTagCache<T>.MaxCacheSizePerType, int hotEntryThresholds = MetricsTagCache<T>.HotEntryThresholds)
    where T : notnull
{
#pragma warning disable IDE1006 // Naming Styles
    private const int MaxCacheSizePerType = 100;
    private const int HotEntryThresholds = 10;
#pragma warning restore IDE1006 // Naming Styles

    public static MetricsTagCache<T> Current { get; } = new MetricsTagCache<T>();

    // A two-tier approach with capacity to prevent infinite caching when a key is missed, and immutability.
    private readonly LimitedCapacityMetricsTagCache limitedCache = new LimitedCapacityMetricsTagCache(maxCacheSizePerType);
    private readonly LockMetricsTagCache hotCache = new LockMetricsTagCache();
    private readonly int hotEntryThresholds = hotEntryThresholds;

    public string[] Get(T value, Func<T, string[]> tagsFactory)
        => Get(value, tagsFactory, out _, out _);

    public string[] Get(T value, Func<T, string[]> tagsFactory, out bool isHotCached, out bool isCacheHit)
    {
        if (hotCache.TryGet(value, out var tags))
        {
            isCacheHit = true;
            isHotCached = true;
            return tags;
        }

        // lock during get and save
        isCacheHit = false;
        isHotCached = false;
        lock (limitedCache)
        {
            tags = limitedCache.Get(value, tagsFactory, out isCacheHit, out var cacheHitCount);

            // put to hot-cache if cache hit is above threshold
            if (isCacheHit && cacheHitCount >= hotEntryThresholds)
            {
                hotCache.TryAdd(value, tags);
                limitedCache.Remove(value);
            }
        }

        return tags;
    }

    private class LockMetricsTagCache
    {
        private readonly object syncObj = new object();
        private Dictionary<T, string[]> cache = new Dictionary<T, string[]>();

        public bool TryGet(T value, out string[] tags)
        {
            // no need lock for read
            return cache.TryGetValue(value, out tags!);
        }

        public bool TryAdd(T value, string[] tags)
        {
            // need lock as concurrent add may call
            lock (syncObj)
            {
                if (cache.ContainsKey(value)) return false;

                var newCache = new Dictionary<T, string[]>(cache);
                newCache.Add(value, tags);
                cache = newCache;

                return true;
            }
        }
    }

    private class LimitedCapacityMetricsTagCache
    {
        private class CacheEntry
        {
            public string[] Tags { get; }
            public int HitCount { get; set; }

            public CacheEntry(string[] tags)
            {
                Tags = tags;
                HitCount = 0;
            }
        }

        private readonly Dictionary<T, CacheEntry> cache;
        private readonly List<T> keys;

        public LimitedCapacityMetricsTagCache(int maxCacheSizePerType)
        {
            cache = new Dictionary<T, CacheEntry>(maxCacheSizePerType);
            keys = new List<T>(maxCacheSizePerType);
        }

        public string[] Get(T value, Func<T, string[]> tagsFactory, out bool cacheHit, out int cacheHitCount)
        {
            if (cache.TryGetValue(value, out var tagsEntry))
            {
                cacheHit = true;
                cacheHitCount = ++tagsEntry.HitCount;
                return tagsEntry.Tags;
            }

            var tags = tagsFactory(value);
            cache[value] = new CacheEntry(tags);

            // delete oldest if fullfilled
            if (keys.Count >= MaxCacheSizePerType)
            {
                var removeKey = keys[0];
                keys.RemoveAt(0);
                cache.Remove(removeKey);
            }

            keys.Add(value);

            cacheHit = false;
            cacheHitCount = 0;
            return tags;
        }

        public void Remove(T value)
        {
            cache.Remove(value);
            keys.Remove(value);
        }
    }

}

public static class MetricsTagCache
{
    public static string[] Get<T>(T value, Func<T, string[]> tagsFactory)
        where T : notnull
        => MetricsTagCache<T>.Current.Get(value, tagsFactory);
}
