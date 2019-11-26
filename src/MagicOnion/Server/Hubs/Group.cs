using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;

namespace MagicOnion.Server.Hubs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GroupConfigurationAttribute : Attribute
    {
        Type type;

        public GroupConfigurationAttribute(Type groupRepositoryFactoryType)
        {
            this.type = groupRepositoryFactoryType;
        }

        public IGroupRepositoryFactory Create()
        {
            return (IGroupRepositoryFactory)Activator.CreateInstance(type);
        }
    }

    public interface IGroupRepositoryFactory
    {
        IGroupRepository CreateRepository(IFormatterResolver formatterResolver, IMagicOnionLogger logger, IServiceLocator serviceLocator);
    }

    public interface IGroupRepository
    {
        IGroup GetOrAdd(string groupName);
        bool TryGet(string groupName, out IGroup group);
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
        public async ValueTask<(bool, IGroup)> TryAddAsync(string groupName, int incluciveLimitCount, bool createIfEmpty)
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
        public async ValueTask<(bool, IGroup, IInMemoryStorage<TStorage>)> TryAddAsync<TStorage>(string groupName, int incluciveLimitCount, bool createIfEmpty, TStorage data)
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
}
