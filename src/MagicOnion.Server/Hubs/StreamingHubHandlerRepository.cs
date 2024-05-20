#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using MagicOnion.Server.Internal;
using Multicaster;

namespace MagicOnion.Server.Hubs;

// Global cache of Streaming Handler
internal class StreamingHubHandlerRepository
{
    bool frozen;
    readonly IDictionary<MethodHandler, UniqueHashDictionary<StreamingHubHandler>> handlersCache = new(MethodHandler.UniqueEqualityComparer.Instance);
    readonly IDictionary<MethodHandler, IMulticastGroupProvider> groupCache = new(MethodHandler.UniqueEqualityComparer.Instance);

    public void RegisterHandler(MethodHandler parent, StreamingHubHandler[] hubHandlers)
    {
        ThrowIfFrozen();

        var handlers = VerifyDuplicate(hubHandlers);
        var hashDict = new UniqueHashDictionary<StreamingHubHandler>(handlers);

        handlersCache.Add(parent, hashDict);
    }

    public UniqueHashDictionary<StreamingHubHandler> GetHandlers(MethodHandler parent)
        => handlersCache[parent];
   

    public void Freeze()
    {
        ThrowIfFrozen();
        frozen = true;

#if NET8_0_OR_GREATER
        handlersCache = handlersCache.ToFrozenDictionary(MethodHandler.UniqueEqualityComparer.Instance);
        groupCache = groupCache.ToFrozenDictionary(MethodHandler.UniqueEqualityComparer.Instance);
#endif
    }

    void ThrowIfFrozen()
    {
        if (frozen) throw new InvalidOperationException($"Cannot modify the {nameof(StreamingHubHandlerRepository)}. The instance is already frozen.");
    }

    public void RegisterGroupProvider(MethodHandler methodHandler, IMulticastGroupProvider groupProvider)
    {
        ThrowIfFrozen();
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
