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
        public IGroupRepository CreateRepository(IFormatterResolver formatterResolver, IMagicOnionLogger logger, IServiceLocator serviceLocator)
        {
            return new ConcurrentDictionaryGroupRepository(formatterResolver, logger);
        }
    }

    public class ConcurrentDictionaryGroupRepository : IGroupRepository
    {
        IFormatterResolver resolver;
        IMagicOnionLogger logger;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public ConcurrentDictionaryGroupRepository(IFormatterResolver resolver, IMagicOnionLogger logger)
        {
            this.resolver = resolver;
            this.factory = CreateGroup;
            this.logger = logger;
        }

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, factory);
        }

        IGroup CreateGroup(string groupName)
        {
            return new ConcurrentDictionaryGroup(groupName, this, resolver, logger);
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
        // ConcurrentDictionary.Count is slow, use external counter.
        int approximatelyLength;

        readonly object gate = new object();

        readonly IGroupRepository parent;
        readonly IFormatterResolver resolver;
        readonly IMagicOnionLogger logger;

        ConcurrentDictionary<Guid, ServiceContext> members;
        IInMemoryStorage inmemoryStorage;

        public string GroupName { get; }

        public ConcurrentDictionaryGroup(string groupName, IGroupRepository parent, IFormatterResolver resolver, IMagicOnionLogger logger)
        {
            this.GroupName = groupName;
            this.parent = parent;
            this.resolver = resolver;
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
                    WriteInAsyncLockVoid(item.Value, message);
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
                        WriteInAsyncLockVoid(item.Value, message);
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
                    WriteInAsyncLockVoid(item.Value, message);
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
            throw new NotImplementedException();
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
                        WriteInAsyncLockVoid(context, message);
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
            Array.Copy(msg.Array, msg.Offset, message, 0, message.Length);
            if (fireAndForget)
            {
                if (exceptConnectionIds == null)
                {
                    var writeCount = 0;
                    foreach (var item in members)
                    {
                        WriteInAsyncLockVoid(item.Value, message);
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
                        WriteInAsyncLockVoid(item.Value, message);
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
            Array.Copy(msg.Array, msg.Offset, message, 0, message.Length);
            if (fireAndForget)
            {
                if (connectionIds != null)
                {
                    var writeCount = 0;
                    foreach (var item in connectionIds)
                    {
                        if (members.TryGetValue(item, out var context))
                        {
                            WriteInAsyncLockVoid(context, message);
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

        static async ValueTask WriteInAsyncLock(ServiceContext context, byte[] value)
        {
            using (await context.AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.ResponseStream.WriteAsync(value).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Grpc.Core.GrpcEnvironment.Logger?.Error(ex, "error occured on write to client, but keep to write other clients.");
                }
            }
        }

        static async void WriteInAsyncLockVoid(ServiceContext context, byte[] value)
        {
            using (await context.AsyncWriterLock.LockAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.ResponseStream.WriteAsync(value).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Grpc.Core.GrpcEnvironment.Logger?.Error(ex, "error occured on write to client, but keep to write other clients.");
                }
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