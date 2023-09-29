using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;

namespace MagicOnion.Server.Hubs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class GroupConfigurationAttribute : Attribute
{
    public Type FactoryType { get; }

    public GroupConfigurationAttribute(Type groupRepositoryFactoryType)
    {
        if (!typeof(IGroupRepositoryFactory).IsAssignableFrom(groupRepositoryFactoryType) && (groupRepositoryFactoryType.IsAbstract || groupRepositoryFactoryType.IsInterface))
        {
            throw new ArgumentException("A Group repository factory must implement IGroupRepositoryFactory interface and must be a concrete class.");
        }

        this.FactoryType = groupRepositoryFactoryType;
    }
}

public interface IGroupRepositoryFactory
{
    IGroupRepository CreateRepository(IMagicOnionSerializer messageSerializer);
}

public interface IGroupRepository
{
    IGroup GetOrAdd(string groupName);
    bool TryGet(string groupName, [NotNullWhen(true)] out IGroup? group);
    bool TryRemove(string groupName);
}

public class HubGroupRepository
{
    readonly ServiceContext serviceContext;
    readonly IGroupRepository repository;
    readonly ConcurrentBag<IGroup> addedGroups = new ConcurrentBag<IGroup>();

    public IGroupRepository RawGroupRepository => repository;


    public HubGroupRepository(ServiceContext serviceContext, IGroupRepository repository)
    {
        this.serviceContext = serviceContext;
        this.repository = repository;
    }

    /// <summary>
    /// Add to group.
    /// </summary>
    public async ValueTask<IGroup> AddAsync(string groupName)
    {
        var group = repository.GetOrAdd(groupName);
        await group.AddAsync(serviceContext).ConfigureAwait(false);
        addedGroups.Add(group);
        return group;
    }

    /// <summary>
    /// Add to group and use some limitations.
    /// </summary>
    public async ValueTask<(bool, IGroup?)> TryAddAsync(string groupName, int incluciveLimitCount, bool createIfEmpty)
    {
        // Note: require lock but currently not locked...
        if (repository.TryGet(groupName, out var group))
        {
            var memberCount = await group.GetMemberCountAsync().ConfigureAwait(false);
            if (memberCount >= incluciveLimitCount)
            {
                return (false, null);
            }
            else
            {
                await group.AddAsync(serviceContext).ConfigureAwait(false);
                addedGroups.Add(group);
                return (true, group);
            }
        }

        if (createIfEmpty)
        {
            group = repository.GetOrAdd(groupName);
            await group.AddAsync(serviceContext).ConfigureAwait(false);
            addedGroups.Add(group);
            return (true, group);
        }
        else
        {
            return (false, null);
        }
    }

    /// <summary>
    /// Add to group and add data to inmemory storage per group.
    /// </summary>
    public async ValueTask<(IGroup, IInMemoryStorage<TStorage>)> AddAsync<TStorage>(string groupName, TStorage data)
        where TStorage : class
    {
        var group = repository.GetOrAdd(groupName);
        await group.AddAsync(serviceContext).ConfigureAwait(false);
        addedGroups.Add(group);

        var storage = group.GetInMemoryStorage<TStorage>();
        storage.Set(serviceContext.ContextId, data);

        return (group, storage);
    }

    /// <summary>
    /// Add to group(with use some limitations) and add data to inmemory storage per group.
    /// </summary>
    public async ValueTask<(bool, IGroup?, IInMemoryStorage<TStorage>?)> TryAddAsync<TStorage>(string groupName, int incluciveLimitCount, bool createIfEmpty, TStorage data)
        where TStorage : class
    {
        // Note: require lock but currently not locked...
        if (repository.TryGet(groupName, out var group))
        {
            var memberCount = await group.GetMemberCountAsync();
            if (memberCount >= incluciveLimitCount)
            {
                return (false, null, null);
            }
            else
            {
                await group.AddAsync(serviceContext).ConfigureAwait(false);
                addedGroups.Add(group);
                var storage = group.GetInMemoryStorage<TStorage>();
                storage.Set(serviceContext.ContextId, data);
                return (true, group, storage);
            }
        }

        if (createIfEmpty)
        {
            var (repo, stor) = await AddAsync(groupName, data);
            return (true, repo, stor);
        }
        else
        {
            return (false, null, null);
        }
    }

    internal async ValueTask DisposeAsync()
    {
        foreach (var item in addedGroups)
        {
            await item.RemoveAsync(serviceContext);
        }
    }
}

public interface IGroup
{
    string GroupName { get; }
    IInMemoryStorage<T> GetInMemoryStorage<T>() where T : class;
    ValueTask<int> GetMemberCountAsync();
    ValueTask AddAsync(ServiceContext context);
    /// <summary>Note: return bool is `removed from parent`.</summary>
    ValueTask<bool> RemoveAsync(ServiceContext context);
    Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget);
    Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget);
    Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget);
    Task WriteExceptRawAsync(ArraySegment<byte> message, Guid[] exceptConnectionIds, bool fireAndForget);
    Task WriteToAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget);
    Task WriteToAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget);
    Task WriteToRawAsync(ArraySegment<byte> message, Guid[] connectionIds, bool fireAndForget);
}

public interface IInMemoryStorage
{
    void Remove(Guid connectionId);
}

public interface IInMemoryStorage<T> : IInMemoryStorage
    where T : class
{
    ICollection<T> AllValues { get; }
    void Set(Guid connectionId, T value);
    [return: MaybeNull]
    T Get(Guid connectionId);
}

public class DefaultInMemoryStorage<T> : IInMemoryStorage<T>
    where T : class
{
    readonly ConcurrentDictionary<Guid, T> storage = new ConcurrentDictionary<Guid, T>();

    public ICollection<T> AllValues => storage.Values;

    public void Set(Guid id, T value)
    {
        storage[id] = value;
    }

    [return: MaybeNull]
    public T Get(Guid id)
    {
        return storage.TryGetValue(id, out var value)
            ? value
            : null;
    }

    public void Remove(Guid id)
    {
        storage.TryRemove(id, out _);
    }
}

public static class GroupBroadcastExtensions
{
    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to all clients.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcaster<TReceiver>(this IGroup group)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType;
        return (TReceiver) Activator.CreateInstance(type, group)!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to all clients excepts one.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="except"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterExcept<TReceiver>(this IGroup group, Guid except)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptOne;
        return (TReceiver) Activator.CreateInstance(type, new object[] {group, except})!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to all clients excepts some clients.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="excepts"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterExcept<TReceiver>(this IGroup group, Guid[] excepts)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptMany;
        return (TReceiver) Activator.CreateInstance(type, new object[] {group, excepts})!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to one client.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="toConnectionId"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterTo<TReceiver>(this IGroup group, Guid toConnectionId)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToOne;
        return (TReceiver) Activator.CreateInstance(type, new object[] { group, toConnectionId })!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to some clients.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="toConnectionIds"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterTo<TReceiver>(this IGroup group, Guid[] toConnectionIds)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToMany;
        return (TReceiver) Activator.CreateInstance(type, new object[] { group, toConnectionIds })!;
    }
}
