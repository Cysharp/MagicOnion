using MagicOnion.Utils;
using System;
using System.Collections.Generic;

namespace MagicOnion.Server.Hubs;

// Global cache of Streaming Handler
internal static class StreamingHubHandlerRepository
{
    static Dictionary<MethodHandler, UniqueHashDictionary<StreamingHubHandler>> cache
        = new Dictionary<MethodHandler, UniqueHashDictionary<StreamingHubHandler>>(new MethodHandler.UniqueEqualityComparer());

    static Dictionary<MethodHandler, IGroupRepository> cacheGroup
        = new Dictionary<MethodHandler, IGroupRepository>(new MethodHandler.UniqueEqualityComparer());

    public static void RegisterHandler(MethodHandler parent, StreamingHubHandler[] hubHandlers)
    {
        var handlers = VerifyDuplicate(hubHandlers);
        var hashDict = new UniqueHashDictionary<StreamingHubHandler>(handlers);

        lock (cache)
        {
            cache.Add(parent, hashDict);
        }
    }

    public static UniqueHashDictionary<StreamingHubHandler> GetHandlers(MethodHandler parent)
    {
        return cache[parent];
    }

    static (int, StreamingHubHandler)[] VerifyDuplicate(StreamingHubHandler[] hubHandlers)
    {
        var list = new List<(int, StreamingHubHandler)>();
        var map = new Dictionary<int, StreamingHubHandler>();
        foreach (var item in hubHandlers)
        {
            var hash = item.MethodId;
            if (map.ContainsKey(hash))
            {
                throw new InvalidOperationException($"StreamingHubHandler.MethodName found duplicate hashCode name. Please rename or use [MethodId] to avoid conflict. {map[hash]} and {item.MethodInfo.Name}");
            }
            map.Add(hash, item);
            list.Add((hash, item));
        }

        return list.ToArray();
    }

    public static void AddGroupRepository(MethodHandler parent, IGroupRepository repository)
    {
        lock (cacheGroup)
        {
            cacheGroup.Add(parent, repository);
        }
    }

    public static IGroupRepository GetGroupRepository(MethodHandler parent)
    {
        return cacheGroup[parent];
    }
}
