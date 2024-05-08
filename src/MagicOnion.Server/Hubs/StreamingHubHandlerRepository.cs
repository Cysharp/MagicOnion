#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Hubs;

// Global cache of Streaming Handler
internal class StreamingHubHandlerRepository
{
    bool frozen;

    IDictionary<MethodHandler, UniqueHashDictionary<StreamingHubHandler>> cache
        = new Dictionary<MethodHandler, UniqueHashDictionary<StreamingHubHandler>>(MethodHandler.UniqueEqualityComparer.Instance);

    IDictionary<MethodHandler, IGroupRepository> cacheGroup
        = new Dictionary<MethodHandler, IGroupRepository>(MethodHandler.UniqueEqualityComparer.Instance);

    public void RegisterHandler(MethodHandler parent, StreamingHubHandler[] hubHandlers)
    {
        ThrowIfFrozen();

        var handlers = VerifyDuplicate(hubHandlers);
        var hashDict = new UniqueHashDictionary<StreamingHubHandler>(handlers);

        cache.Add(parent, hashDict);
    }

    public UniqueHashDictionary<StreamingHubHandler> GetHandlers(MethodHandler parent)
        => cache[parent];

    public void AddGroupRepository(MethodHandler parent, IGroupRepository repository)
    {
        ThrowIfFrozen();
        cacheGroup.Add(parent, repository);
    }

    public IGroupRepository GetGroupRepository(MethodHandler parent)
        => cacheGroup[parent];

    public void Freeze()
    {
        ThrowIfFrozen();
        frozen = true;

#if NET8_0_OR_GREATER
        cache = cache.ToFrozenDictionary(MethodHandler.UniqueEqualityComparer.Instance);
        cacheGroup = cacheGroup.ToFrozenDictionary(MethodHandler.UniqueEqualityComparer.Instance);
#endif
    }

    void ThrowIfFrozen()
    {
        if (frozen) throw new InvalidOperationException($"Cannot modify the {nameof(StreamingHubHandlerRepository)}. The instance is already frozen.");
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
