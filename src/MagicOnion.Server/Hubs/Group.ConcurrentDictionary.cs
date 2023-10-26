using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MagicOnion.Internal.Buffers;

namespace MagicOnion.Server.Hubs;

public class ConcurrentDictionaryGroupRepositoryFactory : IGroupRepositoryFactory
{
    readonly ILogger logger;

    public ConcurrentDictionaryGroupRepositoryFactory(ILogger<ConcurrentDictionaryGroup> logger)
    {
        this.logger = logger;
    }

    public IGroupRepository CreateRepository(IMagicOnionSerializer messageSerializer)
    {
        return new ConcurrentDictionaryGroupRepository(messageSerializer, logger);
    }
}

public class ConcurrentDictionaryGroupRepository : IGroupRepository
{
    readonly IMagicOnionSerializer messageSerializer;
    readonly ILogger logger;

    readonly Func<string, IGroup> factory;
    ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

    public ConcurrentDictionaryGroupRepository(IMagicOnionSerializer messageSerializer, ILogger logger)
    {
        this.messageSerializer = messageSerializer;
        this.factory = CreateGroup;
        this.logger = logger;
    }

    public IGroup GetOrAdd(string groupName)
    {
        return dictionary.GetOrAdd(groupName, factory);
    }

    IGroup CreateGroup(string groupName)
    {
        return new ConcurrentDictionaryGroup(groupName, this, messageSerializer, logger);
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
    readonly IMagicOnionSerializer messageSerializer;
    readonly ILogger logger;

    ConcurrentDictionary<Guid, IServiceContextWithResponseStream<byte[]>> members;
    IInMemoryStorage? inmemoryStorage;

    public string GroupName { get; }

    public ConcurrentDictionaryGroup(string groupName, IGroupRepository parent, IMagicOnionSerializer messageSerializer, ILogger logger)
    {
        this.GroupName = groupName;
        this.parent = parent;
        this.messageSerializer = messageSerializer;
        this.logger = logger;
        this.members = new ConcurrentDictionary<Guid, IServiceContextWithResponseStream<byte[]>>();
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
        if (members.TryAdd(context.ContextId, (IServiceContextWithResponseStream<byte[]>)context))
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
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
            return Task.CompletedTask;
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
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
            return Task.CompletedTask;
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
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
            return Task.CompletedTask;
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
                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, 1);
            }
            return Task.CompletedTask;
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
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
            return Task.CompletedTask;
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
                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
                return Task.CompletedTask;
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
                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
                return Task.CompletedTask;
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

                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
            }

            return Task.CompletedTask;
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
            writer.Flush();
            messageSerializer.Serialize(buffer, value);
            return buffer.WrittenSpan.ToArray();
        }
    }
}
