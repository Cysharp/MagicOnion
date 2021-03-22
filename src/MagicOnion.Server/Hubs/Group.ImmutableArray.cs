using MagicOnion.Utils;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace MagicOnion.Server.Hubs
{
    public class ImmutableArrayGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository(MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
        {
            return new ImmutableArrayGroupRepository(serializerOptions, logger);
        }
    }

    public class ImmutableArrayGroupRepository : IGroupRepository
    {
        MessagePackSerializerOptions serializerOptions;
        IMagicOnionLogger logger;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public ImmutableArrayGroupRepository(MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
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
            return new ImmutableArrayGroup(groupName, this, serializerOptions, logger);
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

    public class ImmutableArrayGroup : IGroup
    {
        readonly object gate = new object();
        readonly IGroupRepository parent;
        readonly MessagePackSerializerOptions serializerOptions;
        readonly IMagicOnionLogger logger;

        ImmutableArray<ServiceContext> members;
        IInMemoryStorage? inmemoryStorage;

        public string GroupName { get; }

        public ImmutableArrayGroup(string groupName, IGroupRepository parent, MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
        {
            this.GroupName = groupName;
            this.parent = parent;
            this.serializerOptions = serializerOptions;
            this.logger = logger;
            this.members = ImmutableArray<ServiceContext>.Empty;
        }

        public ValueTask<int> GetMemberCountAsync()
        {
            return new ValueTask<int>(members.Length);
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
            lock (gate)
            {
                members = members.Add(context);
            }
            return default(ValueTask);
        }

        public ValueTask<bool> RemoveAsync(ServiceContext context)
        {
            lock (gate)
            {
                members = members.Remove(context);
                if (inmemoryStorage != null)
                {
                    inmemoryStorage.Remove(context.ContextId);
                }

                if (members.Length == 0)
                {
                    if (parent.TryRemove(GroupName))
                    {
                        return new ValueTask<bool>(true);
                    }
                }

                return new ValueTask<bool>(false);
            }
        }

        // broadcast: [methodId, [argument]]

        public Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);

            var source = members;

            if (fireAndForget)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    source[i].QueueResponseStreamWrite(message);
                }
                logger.InvokeHubBroadcast(GroupName, message.Length, source.Length);
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

            var source = members;
            if (fireAndForget)
            {
                var writeCount = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].ContextId != connectionId)
                    {
                        source[i].QueueResponseStreamWrite(message);
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

            var source = members;
            if (fireAndForget)
            {
                var writeCount = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    foreach (var item in connectionIds)
                    {
                        if (source[i].ContextId == item)
                        {
                            goto NEXT;
                        }
                    }

                    source[i].QueueResponseStreamWrite(message);
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

            var source = members;

            if (fireAndForget)
            {
                var writeCount = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].ContextId == connectionId)
                    {
                        source[i].QueueResponseStreamWrite(message);
                        writeCount++;
                        break;
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

        public Task WriteToAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);

            var source = members;
            if (fireAndForget)
            {
                var writeCount = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    foreach (var item in connectionIds)
                    {
                        if (source[i].ContextId == item)
                        {
                            source[i].QueueResponseStreamWrite(message);
                            writeCount++;
                            goto NEXT;
                        }
                    }

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

        public Task WriteExceptRawAsync(ArraySegment<byte> msg, Guid[] exceptConnectionIds, bool fireAndForget)
        {
            // oh, copy is bad but current gRPC interface only accepts byte[]...
            var message = new byte[msg.Count];
            Array.Copy(msg.Array!, msg.Offset, message, 0, message.Length);

            var source = members;
            if (fireAndForget)
            {
                var writeCount = 0;
                if (exceptConnectionIds == null)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        source[i].QueueResponseStreamWrite(message);
                        writeCount++;
                    }
                }
                else
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        foreach (var item in exceptConnectionIds)
                        {
                            if (source[i].ContextId == item)
                            {
                                goto NEXT;
                            }
                        }
                        source[i].QueueResponseStreamWrite(message);
                        writeCount++;
                    NEXT:
                        continue;
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

        public Task WriteToRawAsync(ArraySegment<byte> msg, Guid[] connectionIds, bool fireAndForget)
        {
            // oh, copy is bad but current gRPC interface only accepts byte[]...
            var message = new byte[msg.Count];
            Array.Copy(msg.Array!, msg.Offset, message, 0, message.Length);

            var source = members;
            if (fireAndForget)
            {
                var writeCount = 0;
                if (connectionIds != null)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        foreach (var item in connectionIds)
                        {
                            if (source[i].ContextId != item)
                            {
                                goto NEXT;
                            }
                        }
                        source[i].QueueResponseStreamWrite(message);
                        writeCount++;
                    NEXT:
                        continue;
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
    }
}