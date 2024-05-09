using System.Buffers;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Internal;
using Microsoft.Extensions.Logging;
using MagicOnion.Internal.Buffers;

namespace MagicOnion.Server.Hubs;

public class ImmutableArrayGroupRepositoryFactory : IGroupRepositoryFactory
{
    readonly ILogger logger;

    public ImmutableArrayGroupRepositoryFactory(ILogger<ImmutableArrayGroup> logger)
    {
        this.logger = logger;
    }

    public IGroupRepository CreateRepository(IMagicOnionSerializer messageSerializer)
    {
        return new ImmutableArrayGroupRepository(messageSerializer, logger);
    }
}

public class ImmutableArrayGroupRepository : IGroupRepository
{
    readonly IMagicOnionSerializer messageSerializer;
    readonly ILogger logger;

    readonly Func<string, IGroup> factory;
    ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

    public ImmutableArrayGroupRepository(IMagicOnionSerializer messageSerializer, ILogger logger)
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
        return new ImmutableArrayGroup(groupName, this, messageSerializer, logger);
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
    readonly IMagicOnionSerializer messageSerializer;
    readonly ILogger logger;

    ImmutableArray<IServiceContextWithResponseStream<StreamingHubPayload>> members;
    IInMemoryStorage? inmemoryStorage;

    public string GroupName { get; }

    public ImmutableArrayGroup(string groupName, IGroupRepository parent, IMagicOnionSerializer messageSerializer, ILogger logger)
    {
        this.GroupName = groupName;
        this.parent = parent;
        this.messageSerializer = messageSerializer;
        this.logger = logger;
        this.members = ImmutableArray<IServiceContextWithResponseStream<StreamingHubPayload>>.Empty;
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
            members = members.Add((IServiceContextWithResponseStream<StreamingHubPayload>)context);
        }
        return default(ValueTask);
    }

    public ValueTask<bool> RemoveAsync(ServiceContext context)
    {
        lock (gate)
        {
            if (!members.IsEmpty)
            {
                members = members.Remove((IServiceContextWithResponseStream<StreamingHubPayload>)context);
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
                var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                source[i].QueueResponseStreamWrite(payload);
            }
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, source.Length);
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

        var source = members;
        if (fireAndForget)
        {
            var writeCount = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].ContextId != connectionId)
                {
                    var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                    source[i].QueueResponseStreamWrite(payload);
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

                var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                source[i].QueueResponseStreamWrite(payload);
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

        var source = members;

        if (fireAndForget)
        {
            var writeCount = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].ContextId == connectionId)
                {
                    var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                    source[i].QueueResponseStreamWrite(payload);
                    writeCount++;
                    break;
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
                        var payload = StreamingHubPayloadPool.Shared.RentOrCreate(message);
                        source[i].QueueResponseStreamWrite(payload);
                        writeCount++;
                        goto NEXT;
                    }
                }

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

    public Task WriteExceptRawAsync(ArraySegment<byte> msg, Guid[] exceptConnectionIds, bool fireAndForget)
    {
        var source = members;
        if (fireAndForget)
        {
            var writeCount = 0;
            if (exceptConnectionIds == null)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    var messagePayload = StreamingHubPayloadPool.Shared.RentOrCreate(msg.AsMemory());
                    source[i].QueueResponseStreamWrite(messagePayload);
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
                    var messagePayload = StreamingHubPayloadPool.Shared.RentOrCreate(msg.AsMemory());
                    source[i].QueueResponseStreamWrite(messagePayload);
                    writeCount++;
                    NEXT:
                    continue;
                }
            }
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, msg.Count, writeCount);
            return Task.CompletedTask;
        }
        else
        {
            throw new NotSupportedException("The write operation must be called with Fire and Forget option");
        }
    }

    public Task WriteToRawAsync(ArraySegment<byte> msg, Guid[] connectionIds, bool fireAndForget)
    {
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

                    var message = StreamingHubPayloadPool.Shared.RentOrCreate(msg.AsMemory());
                    source[i].QueueResponseStreamWrite(message);
                    writeCount++;
                    NEXT:
                    continue;
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
        return buffer.WrittenMemory.ToArray();
    }
}
