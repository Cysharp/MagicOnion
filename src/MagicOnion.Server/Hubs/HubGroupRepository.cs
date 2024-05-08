using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Multicaster;
using Multicaster.Remoting;

namespace MagicOnion.Server.Hubs;

public class HubGroupRepository<T>
{
    readonly StreamingServiceContext<byte[], byte[]> streamingContext;
    readonly IMulticastGroupProvider groupProvider;
    readonly ConcurrentBag<IMulticastAsyncGroup<T>> addedGroups = new();
    readonly T client;

    internal HubGroupRepository(T remoteClient, StreamingServiceContext<byte[], byte[]> streamingContext, IMulticastGroupProvider multicastGroupProvider)
    {
        Debug.Assert(remoteClient is IRemoteDirectReceiverWriterAccessor directReceiverWriterAccessor && directReceiverWriterAccessor.TryGetDirectReceiverWriter(out _));

        this.client = remoteClient;
        this.streamingContext = streamingContext;
        this.groupProvider = new WrappedGroupProvider(multicastGroupProvider, streamingContext.MethodHandler.ToString());
    }

    /// <summary>
    /// Add to group.
    /// </summary>
    public async ValueTask<IGroup<T>> AddAsync(string groupName)
    {
        var group = groupProvider.GetOrAddGroup<T>(groupName);
        await group.AddAsync(streamingContext.ContextId, client).ConfigureAwait(false);
        addedGroups.Add(group);
        return new Group<T>(group);
    }

    internal async ValueTask DisposeAsync()
    {
        foreach (var item in addedGroups)
        {
            await item.RemoveAsync(streamingContext.ContextId);
        }
    }

    class WrappedGroupProvider : IMulticastGroupProvider
    {
        readonly IMulticastGroupProvider inner;
        readonly string prefix;

        public WrappedGroupProvider(IMulticastGroupProvider inner, string prefix)
        {
            this.inner = inner;
            this.prefix = prefix;
        }

        // NOTE: Add a prefix between each SteramingHubs to separate the groups.
        public IMulticastAsyncGroup<TReceiver> GetOrAddGroup<TReceiver>(string name)
            => inner.GetOrAddGroup<TReceiver>($"{prefix}/{name}");

        public IMulticastSyncGroup<TReceiver> GetOrAddSynchronousGroup<TReceiver>(string name)
            => inner.GetOrAddSynchronousGroup<TReceiver>($"{prefix}/{name}");
    }
}
