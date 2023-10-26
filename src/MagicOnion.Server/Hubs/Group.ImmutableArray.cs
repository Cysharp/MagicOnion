using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Utils;
using MessagePack;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

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

    ImmutableArray<IServiceContextWithResponseStream<byte[]>> members;
    IInMemoryStorage? inmemoryStorage;

    public string GroupName { get; }

    public ImmutableArrayGroup(string groupName, IGroupRepository parent, IMagicOnionSerializer messageSerializer, ILogger logger)
    {
        this.GroupName = groupName;
        this.parent = parent;
        this.messageSerializer = messageSerializer;
        this.logger = logger;
        this.members = ImmutableArray<IServiceContextWithResponseStream<byte[]>>.Empty;
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
            members = members.Add((IServiceContextWithResponseStream<byte[]>)context);
        }
        return default(ValueTask);
    }

    public ValueTask<bool> RemoveAsync(ServiceContext context)
    {
        lock (gate)
        {
            if (!members.IsEmpty)
            {
                members = members.Remove((IServiceContextWithResponseStream<byte[]>)context);
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
                source[i].QueueResponseStreamWrite(message);
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
                    source[i].QueueResponseStreamWrite(message);
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

                source[i].QueueResponseStreamWrite(message);
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
                    source[i].QueueResponseStreamWrite(message);
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
                        source[i].QueueResponseStreamWrite(message);
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
            MagicOnionServerLog.InvokeHubBroadcast(logger, GroupName, message.Length, writeCount);
            return Task.CompletedTask;
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
