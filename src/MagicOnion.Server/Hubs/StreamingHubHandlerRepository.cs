using MagicOnion.Server.Internal;
using Multicaster;

namespace MagicOnion.Server.Hubs;

// Global cache of Streaming Handler
internal class StreamingHubHandlerRepository
{
    readonly Dictionary<MethodHandler, UniqueHashDictionary<StreamingHubHandler>> handlersCache = new(new MethodHandler.UniqueEqualityComparer());
    readonly Dictionary<MethodHandler, IMulticastGroupProvider> groupCache = new(new MethodHandler.UniqueEqualityComparer());

    public void RegisterHandler(MethodHandler parent, StreamingHubHandler[] hubHandlers)
    {
        var handlers = VerifyDuplicate(hubHandlers);
        var hashDict = new UniqueHashDictionary<StreamingHubHandler>(handlers);

        handlersCache.Add(parent, hashDict);
    }

    public UniqueHashDictionary<StreamingHubHandler> GetHandlers(MethodHandler parent)
    {
        return handlersCache[parent];
    }

    public void RegisterGroupProvider(MethodHandler methodHandler, IMulticastGroupProvider groupProvider)
    {
        groupCache[methodHandler] = groupProvider;
    }

    public IMulticastGroupProvider GetGroupProvider(MethodHandler methodHandler)
    {
        return groupCache[methodHandler];
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
}
