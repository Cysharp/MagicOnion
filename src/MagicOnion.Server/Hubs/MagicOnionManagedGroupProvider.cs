using Cysharp.Runtime.Multicast;

namespace MagicOnion.Server.Hubs;

internal class MagicOnionManagedGroupProvider
{
    readonly Dictionary<(string GroupName, Type ReceiverType), (object Group, Counter Counter)> groups = new();
    readonly IMulticastGroupProvider underlyingGroupProvider;
    readonly SemaphoreSlim @lock = new(1);

    public MagicOnionManagedGroupProvider(IMulticastGroupProvider underlyingGroupProvider)
    {
        this.underlyingGroupProvider = underlyingGroupProvider;
    }

    public async ValueTask<Group<T>> GetOrAddAsync<T>(string groupName, Guid contextId, T client)
    {
        var added = false;
        var underlyingGroup = default(IMulticastAsyncGroup<T>);

        await @lock.WaitAsync();
        try
        {
            underlyingGroup = underlyingGroupProvider.GetOrAddGroup<T>(groupName);

            if (!groups.TryGetValue((groupName, typeof(T)), out var groupAndCounter))
            {
                groupAndCounter = groups[(groupName, typeof(T))] = (new Group<T>(underlyingGroup, groupName), new Counter());
            }

            await underlyingGroup.AddAsync(contextId, client);
            added = true;
            groupAndCounter.Counter.CurrentValue += 1;

            return (Group<T>)groupAndCounter.Group;
        }
        finally
        {
            if (!added && groups.TryGetValue((groupName, typeof(T)), out var groupAndCounter) && groupAndCounter.Counter.CurrentValue == 0)
            {
                // When failed to add a member to the group, and the group has no member.
                // We need to remove the group from groups.
                groups.Remove((groupName, typeof(T)));
                underlyingGroup?.Dispose();
            }

            @lock.Release();
        }
    }

    public async ValueTask RemoveAsync<T>(Group<T> group, Guid contextId)
    {
        await @lock.WaitAsync();
        try
        {
            var underlyingGroup = underlyingGroupProvider.GetOrAddGroup<T>(group.Name);

            if (groups.TryGetValue((group.Name, typeof(T)), out var groupAndCounter))
            {
                await underlyingGroup.RemoveAsync(contextId);
                groupAndCounter.Counter.CurrentValue -= 1;
            }

            if (groupAndCounter.Counter.CurrentValue == 0)
            {
                groups.Remove((group.Name, typeof(T)));
                underlyingGroup.Dispose();
            }
        }
        finally
        {
            @lock.Release();
        }
    }

    class Counter
    {
        public int CurrentValue;
    }
}
