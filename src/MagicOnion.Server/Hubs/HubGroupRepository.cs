using System.Collections.Concurrent;
using System.Diagnostics;
using Cysharp.Runtime.Multicast;
using Cysharp.Runtime.Multicast.Remoting;
using MagicOnion.Internal;

namespace MagicOnion.Server.Hubs;

public class HubGroupRepository<T>
{
    readonly StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> streamingContext;
    readonly string prefix;
    readonly MagicOnionManagedGroupProvider autoDisposeGroupProvider;
    readonly ConcurrentBag<Group<T>> addedGroups = new();
    readonly T client;

    internal HubGroupRepository(T remoteClient, StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> streamingContext, MagicOnionManagedGroupProvider autoDisposeGroupProvider)
    {
        Debug.Assert(remoteClient is IRemoteProxy remoteProxy && remoteProxy.TryGetDirectWriter(out _));

        this.client = remoteClient;
        this.streamingContext = streamingContext;
        this.autoDisposeGroupProvider = autoDisposeGroupProvider;
        this.prefix = $"{streamingContext.ServiceName}/{streamingContext.MethodName}";
    }

    /// <summary>
    /// Add to group.
    /// </summary>
    public async ValueTask<IGroup<T>> AddAsync(string groupName)
    {
        var group = await autoDisposeGroupProvider.GetOrAddAsync($"{prefix}/{groupName}", streamingContext.ContextId, client);
        addedGroups.Add(group);
        return group;
    }

    internal async ValueTask DisposeAsync()
    {
        foreach (var item in addedGroups)
        {
            await autoDisposeGroupProvider.RemoveAsync(item, streamingContext.ContextId);
        }
    }
}
