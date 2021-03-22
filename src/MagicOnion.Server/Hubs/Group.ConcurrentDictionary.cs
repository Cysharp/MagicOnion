using MagicOnion.Utils;
using MessagePack;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Hubs
{
    public class ConcurrentDictionaryGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository(MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
        {
            return new ConcurrentDictionaryGroupRepository(serializerOptions, logger);
        }
    }

    public class ConcurrentDictionaryGroupRepository : IGroupRepository
    {
        MessagePackSerializerOptions serializerOptions;
        IMagicOnionLogger logger;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public ConcurrentDictionaryGroupRepository(MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
        {
            this.serializerOptions = serializerOptions;
            this.factory = CreateGroup;
            this.logger = logger;
        }

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, factory);
        }

        IGroup CreateGroup(string groupName)
        {
            return new ConcurrentDictionaryGroup(groupName, this, serializerOptions, logger);
        }

        public bool TryGet(string groupName, [NotNullWhen(true)] out IGroup? group)
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
        // ConcurrentDictionary.Count is slow, use external counter.
        int approximatelyLength;

        readonly object gate = new object();

        readonly IGroupRepository parent;
        readonly MessagePackSerializerOptions serializerOptions;
        readonly IMagicOnionLogger logger;

        ConcurrentDictionary<Guid, ServiceContext> members;
        IInMemoryStorage? inmemoryStorage;

        public string GroupName { get; }

        public ConcurrentDictionaryGroup(string groupName, IGroupRepository parent, MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
        {
            this.GroupName = groupName;
            this.parent = parent;
            this.serializerOptions = serializerOptions;
            this.logger = logger;
            this.members = new ConcurrentDictionary<Guid, ServiceContext>();
        }

        public ValueTask<int> GetMemberCountAsync()
        {
            return new ValueTask<int>(approximatelyLength);
        }

        public IInMemoryStorage<T> GetInMemoryStorage<T>()
            where T : class
        {
            lock (gate)
            {
                if (inmemoryStorage == null)
                {
                    inmemoryStorage = new DefaultInMemoryStorage<T>();
                }
                else if (!(inmemoryStorage is IInMemoryStorage<T>))
                {
                    throw new ArgumentException("already initialized inmemory-storage by another type, inmemory-storage only use single type");
                }

                return (IInMemoryStorage<T>)inmemoryStorage;
            }
        }

        public ValueTask AddAsync(ServiceContext context)
        {
            if (members.TryAdd(context.ContextId, context))
            {
                Interlocked.Increment(ref approximatelyLength);
            }
            return default(ValueTask);
        }

        public ValueTask<bool> RemoveAsync(ServiceContext context)
        {
            if (members.TryRemove(context.ContextId, out _))
            {
                Interlocked.Decrement(ref approximatelyLength);
                if (inmemoryStorage != null)
                {
                    inmemoryStorage.Remove(context.ContextId);
                }
            }

            if (members.Count == 0)
            {
                if (parent.TryRemove(GroupName))
                {
                    return new ValueTask<bool>(true);
                }
            }
            return new ValueTask<bool>(false);
        }

        // broadcast: [methodId, [argument]]

        public Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);

            if (fireAndForget)
            {
                var writeCount = 0;
                foreach (var item in members)
                {
                    item.Value.QueueResponseStreamWrite(message);
                    writeCount++;
                }
                logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                return TaskEx.CompletedTask;
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);
            if (fireAndForget)
            {
                var writeCount = 0;
                foreach (var item in members)
                {
                    if (item.Value.ContextId != connectionId)
                    {
                        item.Value.QueueResponseStreamWrite(message);
                        writeCount++;
                    }
                }
                logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                return TaskEx.CompletedTask;
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);
            if (fireAndForget)
            {
                var writeCount = 0;
                foreach (var item in members)
                {
                    foreach (var item2 in connectionIds)
                    {
                        if (item.Value.ContextId == item2)
                        {
                            goto NEXT;
                        }
                    }
                    item.Value.QueueResponseStreamWrite(message);
                    writeCount++;
                    NEXT:
                    continue;
                }
                logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                return TaskEx.CompletedTask;
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }

        public Task WriteToAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);
            if (fireAndForget)
            {
                if (members.TryGetValue(connectionId, out var context))
                {
                    context.QueueResponseStreamWrite(message);
                    logger.InvokeHubBroadcast(GroupName, message.Length, 1);
                }
                return TaskEx.CompletedTask;
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }

        public Task WriteToAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);
            if (fireAndForget)
            {
                var writeCount = 0;
                foreach (var item in connectionIds)
                {
                    if (members.TryGetValue(item, out var context))
                    {
                        context.QueueResponseStreamWrite(message);
                        writeCount++;
                    }
                }
                logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                return TaskEx.CompletedTask;
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }

        public Task WriteExceptRawAsync(ArraySegment<byte> msg, Guid[] exceptConnectionIds, bool fireAndForget)
        {
            // oh, copy is bad but current gRPC interface only accepts byte[]...
            var message = new byte[msg.Count];
            Array.Copy(msg.Array!, msg.Offset, message, 0, message.Length);
            if (fireAndForget)
            {
                if (exceptConnectionIds == null)
                {
                    var writeCount = 0;
                    foreach (var item in members)
                    {
                        item.Value.QueueResponseStreamWrite(message);
                        writeCount++;
                    }
                    logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                    return TaskEx.CompletedTask;
                }
                else
                {
                    var writeCount = 0;
                    foreach (var item in members)
                    {
                        foreach (var item2 in exceptConnectionIds)
                        {
                            if (item.Value.ContextId == item2)
                            {
                                goto NEXT;
                            }
                        }
                        item.Value.QueueResponseStreamWrite(message);
                        writeCount++;
                        NEXT:
                        continue;
                    }
                    logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                    return TaskEx.CompletedTask;
                }
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }



        public Task WriteToRawAsync(ArraySegment<byte> msg, Guid[] connectionIds, bool fireAndForget)
        {
            // oh, copy is bad but current gRPC interface only accepts byte[]...
            var message = new byte[msg.Count];
            Array.Copy(msg.Array!, msg.Offset, message, 0, message.Length);
            if (fireAndForget)
            {
                if (connectionIds != null)
                {
                    var writeCount = 0;
                    foreach (var item in connectionIds)
                    {
                        if (members.TryGetValue(item, out var context))
                        {
                            context.QueueResponseStreamWrite(message);
                            writeCount++;
                        }
                    }

                    logger.InvokeHubBroadcast(GroupName, message.Length, writeCount);
                }

                return TaskEx.CompletedTask;
            }
            else
            {
                throw new NotSupportedException("The write operation must be called with Fire and Forget option");
            }
        }

        byte[] BuildMessage<T>(int methodId, T value)
        {
            using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
            {
                var writer = new MessagePackWriter(buffer);
                writer.WriteArrayHeader(2);
                writer.WriteInt32(methodId);
                MessagePackSerializer.Serialize(ref writer, value, serializerOptions);
                writer.Flush();
                return buffer.WrittenSpan.ToArray();
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