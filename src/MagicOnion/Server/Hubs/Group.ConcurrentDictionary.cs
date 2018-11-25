using MagicOnion.Utils;
using MessagePack;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class ConcurrentDictionaryGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository(IFormatterResolver resolver)
        {
            return new ConcurrentDictionaryGroupRepository(resolver);
        }
    }


    public class ConcurrentDictionaryGroupRepository : IGroupRepository
    {
        IFormatterResolver resolver;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public ConcurrentDictionaryGroupRepository(IFormatterResolver resolver)
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
            return new ConcurrentDictionaryGroup(groupName, this, resolver);
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


    public class ConcurrentDictionaryGroup : IGroup
    {
        int approximatelyLength;

        readonly IGroupRepository parent;
        readonly IFormatterResolver resolver;

        ConcurrentDictionary<Guid, ServiceContext> members;

        public string GroupName { get; }

        public ConcurrentDictionaryGroup(string groupName, IGroupRepository parent, IFormatterResolver resolver)
        {
            this.GroupName = groupName;
            this.parent = parent;
            this.resolver = resolver;
            this.members = new ConcurrentDictionary<Guid, ServiceContext>();
        }

        public void Add(ServiceContext context)
        {
            if (members.TryAdd(context.ContextId, context))
            {
                Interlocked.Increment(ref approximatelyLength);
            }
        }

        public void Remove(ServiceContext context)
        {
            if (members.TryRemove(context.ContextId, out _))
            {
                Interlocked.Decrement(ref approximatelyLength);
            }
            if (members.Count == 0)
            {
                parent.TryRemove(GroupName);
            }
        }

        // broadcast: [methodId, [argument]]

        public async Task WriteAllAsync<T>(int methodId, T value)
        {
            var message = BuildMessage(methodId, value);

            var rent = ArrayPool<ValueTask>.Shared.Rent(approximatelyLength);
            try
            {
                var buffer = rent;
                var index = 0;
                foreach (var item in members)
                {
                    if (buffer.Length < index)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }
                    buffer[index++] = WriteInAsyncLock(item.Value, message);
                }

                await ToPromise(buffer, index).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<ValueTask>.Shared.Return(rent, true);
            }
        }

        public async Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId)
        {
            var message = BuildMessage(methodId, value);
            var rent = ArrayPool<ValueTask>.Shared.Rent(approximatelyLength);
            try
            {
                var buffer = rent;
                var index = 0;
                foreach (var item in members)
                {
                    if (buffer.Length < index)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }
                    if (item.Value.ContextId == connectionId)
                    {
                        buffer[index++] = default(ValueTask);
                    }
                    else
                    {
                        buffer[index++] = WriteInAsyncLock(item.Value, message);
                    }
                }

                await ToPromise(buffer, index).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<ValueTask>.Shared.Return(rent, true);
            }
        }

        public async Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds)
        {
            var message = BuildMessage(methodId, value);
            var rent = ArrayPool<ValueTask>.Shared.Rent(approximatelyLength);
            try
            {
                var buffer = rent;
                var index = 0;
                foreach (var item in members)
                {
                    if (buffer.Length < index)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }

                    foreach (var item2 in connectionIds)
                    {
                        if (item.Value.ContextId == item2)
                        {
                            buffer[index++] = default(ValueTask);
                            goto NEXT;
                        }
                    }
                    buffer[index++] = WriteInAsyncLock(item.Value, message);
                    
                    NEXT:
                    continue;
                }

                await ToPromise(buffer, index).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<ValueTask>.Shared.Return(rent, true);
            }

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

        ValueTask ToPromise(ValueTask[] whenAll, int index)
        {
            var promise = new ReservedWhenAllPromise(index);
            for (int i = 0; i < index; i++)
            {
                promise.Add(whenAll[i]);
            }
            return promise.AsValueTask();
        }
    }
}