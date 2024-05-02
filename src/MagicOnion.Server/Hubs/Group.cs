using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Multicaster;
using Multicaster.InMemory;
using Multicaster.Remoting;
using SerializationContext = Multicaster.Remoting.SerializationContext;

namespace MagicOnion.Server.Hubs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class GroupConfigurationAttribute : Attribute
{
    public Type FactoryType { get; }

    public GroupConfigurationAttribute(Type groupRepositoryFactoryType)
    {
        if (!typeof(IGroupRepositoryFactory).IsAssignableFrom(groupRepositoryFactoryType) && (groupRepositoryFactoryType.IsAbstract || groupRepositoryFactoryType.IsInterface))
        {
            throw new ArgumentException("A Group repository factory must implement IGroupRepositoryFactory interface and must be a concrete class.");
        }

        this.FactoryType = groupRepositoryFactoryType;
    }
}

public interface IGroupRepositoryFactory
{
    IGroupRepository CreateRepository(IMagicOnionSerializer messageSerializer);
}

public interface IGroupRepository
{
    IGroup GetOrAdd(string groupName);
    bool TryGet(string groupName, [NotNullWhen(true)] out IGroup? group);
    bool TryRemove(string groupName);
}

internal class MagicOnionRemoteReceiverWriter : IRemoteReceiverWriter
{
    readonly StreamingServiceContext<byte[], byte[]> writer;

    public MagicOnionRemoteReceiverWriter(StreamingServiceContext<byte[], byte[]> writer)
    {
        this.writer = writer;
    }

    public void Write(ReadOnlyMemory<byte> payload)
    {
        writer.QueueResponseStreamWrite(payload.ToArray());
    }
}

internal class MagicOnionRemoteSerializer : IRemoteSerializer
{
    readonly IMagicOnionSerializer serializer;

    public MagicOnionRemoteSerializer(IMagicOnionSerializer serializer)
    {
        this.serializer = serializer;
    }

    public void Serialize<T>(IBufferWriter<byte> bufferWriter, T value, SerializationContext ctx)
    {
        var writer = new MessagePackWriter(bufferWriter);
        writer.WriteArrayHeader(2);
        writer.WriteInt32(FNV1A32.GetHashCode(ctx.MethodName));
        writer.Flush();
        serializer.Serialize(bufferWriter, value);
    }
}

internal class MagicOnionMulticastGroupProviderFactory(IInMemoryProxyFactory inMemoryProxyFactory, IRemoteProxyFactory remoteProxyFactory, IOptions<MagicOnionOptions> options)
{
    readonly ConcurrentDictionary<(string, Type), object> groupProviders = new();

    public IMulticastGroupProvider<T> CreateProvider<T>(string methodHandlerName)
    {
        return (IMulticastGroupProvider<T>)groupProviders.GetOrAdd(
            (methodHandlerName, typeof(T)),
            _ => new RemoteCompositeGroupProvider<T>(
                inMemoryProxyFactory,
                remoteProxyFactory,
                new MagicOnionRemoteSerializer(options.Value.MessageSerializer.Create(MethodType.DuplexStreaming, null))
            )
        );
    }
}

public interface IGroup<T> : IMulticastGroup<T>
{
    ValueTask RemoveAsync(ServiceContext context);
}

internal class Group<T> : IGroup<T>
{
    readonly IMulticastGroup<T> _group;

    public Group(IMulticastGroup<T> group)
    {
        _group = group;
    }

    public T All => _group.All;

    public T Except(IReadOnlyList<Guid> excludes)
        => _group.Except(excludes);

    public T Only(IReadOnlyList<Guid> targets)
        => _group.Only(targets);

    public ValueTask RemoveAsync(ServiceContext context)
        => _group.RemoveAsync(context.ContextId);

    public ValueTask AddAsync(Guid key, T receiver)
        => _group.AddAsync(key, receiver);

    public ValueTask RemoveAsync(Guid key)
        => _group.RemoveAsync(key);

    public ValueTask<int> CountAsync() => _group.CountAsync();
}

public class HubGroupRepository<T> : IMulticastGroupProvider<T>
{
    readonly StreamingServiceContext<byte[], byte[]> streamingContext;
    readonly IMulticastGroupProvider<T> groupProvider;
    readonly ConcurrentBag<IMulticastGroup<T>> addedGroups = new();
    readonly T client;

    internal HubGroupRepository(T remoteClient, StreamingServiceContext<byte[], byte[]> streamingContext)
    {
        Debug.Assert(remoteClient is IRemoteSingleReceiverWriterAccessor singleReceiverWriterAccessor && singleReceiverWriterAccessor.TryGetSingleReceiver(out _));

        this.client = remoteClient;
        this.streamingContext = streamingContext;
        this.groupProvider = streamingContext.ServiceProvider.GetRequiredService<MagicOnionMulticastGroupProviderFactory>().CreateProvider<T>(streamingContext.MethodHandler.ToString());
    }

    /// <summary>
    /// Add to group.
    /// </summary>
    public async ValueTask<IGroup<T>> AddAsync(string groupName)
    {
        var group = groupProvider.GetOrAdd(groupName);
        await group.AddAsync(streamingContext.ContextId, client).ConfigureAwait(false);
        addedGroups.Add(group);
        return new Group<T>(group);
    }

    internal async ValueTask DisposeAsync()
    {
        foreach (var item in addedGroups)
        {
            await item.RemoveAsync(streamingContext.ContextId);
        }
    }

    IMulticastGroup<T> IMulticastGroupProvider<T>.GetOrAdd(string name)
        => groupProvider.GetOrAdd(name);

    public IMulticastGroupProvider<T> AsMulticastGroupProvider()
        => groupProvider;
}

public interface IGroup
{
    string GroupName { get; }
    IInMemoryStorage<T> GetInMemoryStorage<T>() where T : class;
    ValueTask<int> GetMemberCountAsync();
    ValueTask AddAsync(ServiceContext context);
    /// <summary>Note: return bool is `removed from parent`.</summary>
    ValueTask<bool> RemoveAsync(ServiceContext context);
    Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget);
    Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget);
    Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget);
    Task WriteExceptRawAsync(ArraySegment<byte> message, Guid[] exceptConnectionIds, bool fireAndForget);
    Task WriteToAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget);
    Task WriteToAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget);
    Task WriteToRawAsync(ArraySegment<byte> message, Guid[] connectionIds, bool fireAndForget);
}

public interface IInMemoryStorage
{
    void Remove(Guid connectionId);
}

public interface IInMemoryStorage<T> : IInMemoryStorage
    where T : class
{
    ICollection<T> AllValues { get; }
    void Set(Guid connectionId, T value);
    [return: MaybeNull]
    T Get(Guid connectionId);
}

public class DefaultInMemoryStorage<T> : IInMemoryStorage<T>
    where T : class
{
    readonly ConcurrentDictionary<Guid, T> storage = new ConcurrentDictionary<Guid, T>();

    public ICollection<T> AllValues => storage.Values;

    public void Set(Guid id, T value)
    {
        storage[id] = value;
    }

    [return: MaybeNull]
    public T? Get(Guid id)
    {
        return storage.TryGetValue(id, out var value)
            ? value
            : null;
    }

    public void Remove(Guid id)
    {
        storage.TryRemove(id, out _);
    }
}

public static class GroupBroadcastExtensions
{
    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to all clients.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcaster<TReceiver>(this IGroup group)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType;
        return (TReceiver) Activator.CreateInstance(type, group)!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to all clients excepts one.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="except"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterExcept<TReceiver>(this IGroup group, Guid except)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptOne;
        return (TReceiver) Activator.CreateInstance(type, new object[] {group, except})!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to all clients excepts some clients.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="excepts"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterExcept<TReceiver>(this IGroup group, Guid[] excepts)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptMany;
        return (TReceiver) Activator.CreateInstance(type, new object[] {group, excepts})!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to one client.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="toConnectionId"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterTo<TReceiver>(this IGroup group, Guid toConnectionId)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToOne;
        return (TReceiver) Activator.CreateInstance(type, new object[] { group, toConnectionId })!;
    }

    /// <summary>
    /// Create a receiver proxy from the group. Can be use to broadcast messages to some clients.
    /// </summary>
    /// <typeparam name="TReceiver"></typeparam>
    /// <param name="group"></param>
    /// <param name="toConnectionIds"></param>
    /// <returns></returns>
    public static TReceiver CreateBroadcasterTo<TReceiver>(this IGroup group, Guid[] toConnectionIds)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToMany;
        return (TReceiver) Activator.CreateInstance(type, new object[] { group, toConnectionIds })!;
    }
}
