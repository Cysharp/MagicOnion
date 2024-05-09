using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Internal;
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

    ConcurrentDictionary<Guid, IServiceContextWithResponseStream<StreamingHubPayload>> members;
    IInMemoryStorage? inmemoryStorage;

    public string GroupName { get; }

    public ConcurrentDictionaryGroup(string groupName, IGroupRepository parent, IMagicOnionSerializer messageSerializer, ILogger logger)
    {
        this.GroupName = groupName;
        this.parent = parent;
        this.messageSerializer = messageSerializer;
        this.logger = logger;
        this.members = new ConcurrentDictionary<Guid, IServiceContextWithResponseStream<StreamingHubPayload>>();
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
        if (members.TryAdd(context.ContextId, (IServiceContextWithResponseStream<StreamingHubPayload>)context))
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
                var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                item.Value.QueueResponseStreamWrite(payload);
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
                    var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                    item.Value.QueueResponseStreamWrite(payload);
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
                var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                item.Value.QueueResponseStreamWrite(payload);
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
                var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                context.QueueResponseStreamWrite(payload);
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
                    var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                    context.QueueResponseStreamWrite(payload);
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
        if (fireAndForget)
        {
            if (exceptConnectionIds == null)
            {
                var writeCount = 0;
                foreach (var item in members)
                {
                    var payload = StreamingHubPayloadPool.Shared.RentOrCreate(msg.AsMemory());
                    item.Value.QueueResponseStreamWrite(payload);
                    writeCount++;
                }
                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, msg.Count, writeCount);
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
                    var payload = StreamingHubPayloadPool.Shared.RentOrCreate(msg.AsMemory());
                    item.Value.QueueResponseStreamWrite(payload);
                    writeCount++;
                    NEXT:
                    continue;
                }
                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, msg.Count, writeCount);
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
        if (fireAndForget)
        {
            if (connectionIds != null)
            {
                var writeCount = 0;
                foreach (var item in connectionIds)
                {
                    if (members.TryGetValue(item, out var context))
                    {
                        var payload = StreamingHubPayloadPool.Shared.RentOrCreate(msg.AsMemory());
                        context.QueueResponseStreamWrite(payload);
                        writeCount++;
                    }
                }

                MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, msg.Count, writeCount);
            }

            return Task.CompletedTask;
        }
        else
        {
            throw new NotSupportedException("The write operation must be called with Fire and Forget option");
        }
    }

    ReadOnlyMemory<byte> BuildMessage<T>(int methodId, T value)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteBroadcastMessage(buffer, methodId, value, messageSerializer);
        return buffer.WrittenSpan.ToArray();
    }
}
