using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public interface IGroupRepositoryFactory
    {
        IGroupRepository CreateRepository(IFormatterResolver resolver);
    }

    public interface IGroupRepository
    {
        IGroup GetOrAdd(string groupName);
        bool TryGet(string groupName, out IGroup group);
        bool TryRemove(string groupName);
    }

    public interface IGroup
    {
        string GroupName { get; }
        void Add(ServiceContext context);
        void Remove(ServiceContext context);
        Task WriteAllAsync<T>(int methodId, T value);
        Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId);
        Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds);
    }

    public static class GroupRepositoryExtensions
    {
        public static IGroup Add(this IGroupRepository repository, string groupName, ServiceContext context)
        {
            var group = repository.GetOrAdd(groupName);
            group.Add(context);
            return group; 
        }
    }
}
