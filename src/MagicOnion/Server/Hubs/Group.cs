using System;
using System.Threading.Tasks;

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
        IGroupRepository CreateRepository(IServiceLocator serviceLocator);
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

        public IGroupRepository RawGroupRepository => repository;

        public HubGroupRepository(ServiceContext serviceContext, IGroupRepository repository)
        {
            this.serviceContext = serviceContext;
            this.repository = repository;
        }

        public async ValueTask<IGroup> AddAsync(string groupName)
        {
            var group = repository.GetOrAdd(groupName);
            await group.AddAsync(serviceContext).ConfigureAwait(false);
            return group;
        }
    }

    public interface IGroup
    {
        string GroupName { get; }
        ValueTask<int> GetMemberCountAsync();
        ValueTask AddAsync(ServiceContext context);
        // Return Bool is removed from parent.
        ValueTask<bool> RemoveAsync(ServiceContext context);
        Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget);
        Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget);
        Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget);
        Task WriteRawAsync(ArraySegment<byte> message, Guid[] exceptConnectionIds, bool fireAndForget);
    }
}
