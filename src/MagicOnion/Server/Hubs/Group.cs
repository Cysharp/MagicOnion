using MagicOnion.Utils;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public interface IGroupRepositoryFactory
    {
        IGroupRepository CreateRepository();
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

    public class ImmutableArrayGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository()
        {
            return new ImmutableArrayGroupRepository();
        }
    }


    public class ImmutableArrayGroupRepository : IGroupRepository
    {
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, x => new ImmutableArrayGroup(x, this, null)); // avoid capture?
        }

        public bool TryGet(string groupName, out IGroup group)
        {
            return dictionary.TryGetValue(groupName, out group);
        }

        public bool TryRemove(string groupName)
        {
            return dictionary.TryRemove(groupName, out _);
        }
    }


    public class ImmutableArrayGroup : IGroup
    {
        readonly object gate = new object();
        ImmutableArray<ServiceContext> members;
        readonly IGroupRepository parent;
        // IFormatterResolver resolver;

        public string GroupName { get; }

        public ImmutableArrayGroup(string groupName, IGroupRepository parent, IFormatterResolver resolver)
        {
            this.GroupName = groupName;
            this.parent = parent;
            // this.resolver = resolver;
            this.members = ImmutableArray<ServiceContext>.Empty;
        }

        public void Add(ServiceContext context)
        {
            lock (gate)
            {
                members = members.Add(context);
            }
        }

        public void Remove(ServiceContext context)
        {
            lock (gate)
            {
                members = members.Remove(context);
                if (members.Length == 0)
                {
                    parent.TryRemove(GroupName);
                }
            }
        }

        // broadcast: [methodId, [argument]]

        public async Task WriteAllAsync<T>(int methodId, T value)
        {
            var message = BuildMessage(methodId, value);

            var source = members;
            var promise = new ReservingWhenAllPromise(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                promise.Add(WriteInAsyncLock(source[i], message));
            }

            await promise.AsValueTask().ConfigureAwait(false);
        }

        public async Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId)
        {
            var message = BuildMessage(methodId, value);

            var source = members;
            var promise = new ReservingWhenAllPromise(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].ContextId == connectionId)
                {
                    promise.Add(default(ValueTask));
                }
                else
                {
                    promise.Add(WriteInAsyncLock(source[i], message));
                }
            }

            await promise.AsValueTask().ConfigureAwait(false);
        }

        public async Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds)
        {
            var message = BuildMessage(methodId, value);

            var source = members;
            var promise = new ReservingWhenAllPromise(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                foreach (var item in connectionIds)
                {
                    if (source[i].ContextId == item)
                    {
                        promise.Add(default(ValueTask));
                        goto NEXT;
                    }
                }

                promise.Add(WriteInAsyncLock(source[i], message));
            NEXT:
                continue;
            }

            await promise.AsValueTask().ConfigureAwait(false);
        }

        byte[] BuildMessage<T>(int methodId, T value)
        {
            // TODO:use memory pool.
            byte[] buffer = null;
            var offset = 0;
            offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 2);
            offset += MessagePackBinary.WriteInt32(ref buffer, offset, methodId);
            offset += LZ4MessagePackSerializer.SerializeToBlock<T>(ref buffer, offset, value, null); // TODO:resolver.
            var result = MessagePackBinary.FastCloneWithResize(buffer, offset);
            return result;
        }

        async ValueTask WriteInAsyncLock(ServiceContext context, byte[] value)
        {
            using (await context.AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                await context.ResponseStream.WriteAsync(value).ConfigureAwait(false);
            }
        }
    }
}
