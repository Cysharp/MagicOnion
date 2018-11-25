using MagicOnion.Utils;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class ImmutableArrayGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository(IFormatterResolver resolver)
        {
            return new ImmutableArrayGroupRepository(resolver);
        }
    }


    public class ImmutableArrayGroupRepository : IGroupRepository
    {
        IFormatterResolver resolver;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public ImmutableArrayGroupRepository(IFormatterResolver resolver)
        {
            this.resolver = resolver;
            this.factory = CreateGroup;
        }

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, factory);
        }

        IGroup CreateGroup(string groupName)
        {
            return new ImmutableArrayGroup(groupName, this, resolver);
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
        readonly IGroupRepository parent;
        readonly IFormatterResolver resolver;

        ImmutableArray<ServiceContext> members;

        public string GroupName { get; }

        public ImmutableArrayGroup(string groupName, IGroupRepository parent, IFormatterResolver resolver)
        {
            this.GroupName = groupName;
            this.parent = parent;
            this.resolver = resolver;
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
            var promise = new ReservedWhenAllPromise(source.Length);
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
            var promise = new ReservedWhenAllPromise(source.Length);
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
            var promise = new ReservedWhenAllPromise(source.Length);
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
            var rent = System.Buffers.ArrayPool<byte>.Shared.Rent(ushort.MaxValue);
            var buffer = rent;
            try
            {
                var offset = 0;
                offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 2);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, methodId);
                offset += LZ4MessagePackSerializer.SerializeToBlock<T>(ref buffer, offset, value, resolver);
                var result = MessagePackBinary.FastCloneWithResize(buffer, offset);
                return result;
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(rent);
            }
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