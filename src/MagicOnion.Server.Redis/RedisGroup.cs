using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs;
using MagicOnion.Utils;
using MessagePack;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace MagicOnion.Server.Redis;

public class RedisGroupRepository : IGroupRepository
{
    MessagePackSerializerOptions serializerOptions;
    IMagicOnionLogger logger;
    ConnectionMultiplexer connection;
    int db;

    readonly Func<string, IGroup> factory;
    ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

    public RedisGroupRepository(MessagePackSerializerOptions serializerOptions, RedisGroupOptions redisGroupOptions, IMagicOnionLogger logger)
    {
        this.serializerOptions = serializerOptions;
        this.logger = logger;
        this.factory = CreateGroup;
        this.connection = redisGroupOptions.ConnectionMultiplexer ?? throw new InvalidOperationException("RedisGroup requires add ConnectionMultiplexer to MagicOnionOptions.ServiceLocator before create it. Please try new MagicOnionOptions{DefaultServiceLocator.Register(new ConnectionMultiplexer)}");
        this.db = redisGroupOptions.Db;
    }

    public IGroup GetOrAdd(string groupName)
    {
        return dictionary.GetOrAdd(groupName, factory);
    }

    IGroup CreateGroup(string groupName)
    {
        return new RedisGroup(groupName, serializerOptions, new ConcurrentDictionaryGroup(groupName, this, serializerOptions, logger), connection.GetSubscriber(), connection.GetDatabase(db));
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

public class RedisGroup : IGroup
{
    ISubscriber subscriber;
    IGroup inmemoryGroup;
    IDatabaseAsync database;
    RedisChannel channel;
    MessagePackSerializerOptions serializerOptions;
    ChannelMessageQueue mq;
    RedisKey counterKey;

    public RedisGroup(string groupName, MessagePackSerializerOptions serializerOptions, IGroup inmemoryGroup, ISubscriber redisSubscriber, IDatabaseAsync database)
    {
        this.GroupName = groupName;
        this.serializerOptions = serializerOptions;
        this.channel = new RedisChannel("MagicOnion.Redis.RedisGroup?groupName=" + groupName, RedisChannel.PatternMode.Literal);
        this.counterKey = "MagicOnion.Redis.RedisGroup.MemberCount?groupName=" + groupName;
        this.inmemoryGroup = inmemoryGroup;
        this.subscriber = redisSubscriber;
        this.database = database;

        this.mq = redisSubscriber.Subscribe(channel);
        mq.OnMessage(message => PublishFromRedisToMemoryGroup(message.Message, this.inmemoryGroup));
    }

    public string GroupName { get; }


    public IInMemoryStorage<T> GetInMemoryStorage<T>()
        where T : class
    {
        throw new NotSupportedException("InMemoryStorage does not support in RedisGroup.");
    }

    public async ValueTask AddAsync(ServiceContext context)
    {
        await database.StringIncrementAsync(counterKey, 1).ConfigureAwait(false);
        await inmemoryGroup.AddAsync(context).ConfigureAwait(false);
    }

    public async ValueTask<bool> RemoveAsync(ServiceContext context)
    {
        if (await inmemoryGroup.RemoveAsync(context)) // if inmemoryGroup.Remove succeed, removed from.RedisGroupRepository.
        {
            await database.KeyDeleteAsync(counterKey).ConfigureAwait(false);
            await mq.UnsubscribeAsync();
            return true;
        }
        else
        {
            await database.StringIncrementAsync(counterKey, -1).ConfigureAwait(false);
        }

        return false;
    }

    static Task PublishFromRedisToMemoryGroup(RedisValue value, IGroup group)
    {
        byte[] buffer = value;
        var reader = new MessagePackReader(buffer);

        var len1 = reader.ReadArrayHeader();
        if (len1 == 3)
        {
            var isExcept = reader.ReadBoolean();
            if (isExcept)
            {
                var excludes = NativeGuidArrayFormatter.Deserialize(ref reader);
                var offset = (int)reader.Consumed;
                return group.WriteExceptRawAsync(new ArraySegment<byte>(buffer, offset, buffer.Length - offset), excludes, fireAndForget: true);
            }
            else
            {
                var includes = NativeGuidArrayFormatter.Deserialize(ref reader);
                var offset = (int)reader.Consumed;
                return group.WriteToRawAsync(new ArraySegment<byte>(buffer, offset, buffer.Length - offset), includes, fireAndForget: true);
            }
        }

        return Task.CompletedTask;
    }

    public async ValueTask<int> GetMemberCountAsync()
    {
        return (int)await database.StringGetAsync(counterKey);
    }

    public Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget)
    {
        var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
        return subscriber.PublishAsync(channel, BuildMessage(methodId, value, null, true), flags);
    }

    public Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget)
    {
        var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
        return subscriber.PublishAsync(channel, BuildMessage(methodId, value, new[] { connectionId }, true), flags);
    }

    public Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
    {
        var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
        return subscriber.PublishAsync(channel, BuildMessage(methodId, value, connectionIds, true), flags);
    }

    public Task WriteToAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget)
    {
        var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
        return subscriber.PublishAsync(channel, BuildMessage(methodId, value, new[] { connectionId }, false), flags);
    }

    public Task WriteToAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
    {
        var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
        return subscriber.PublishAsync(channel, BuildMessage(methodId, value, connectionIds, false), flags);
    }

    byte[] BuildMessage<T>(int methodId, T value, Guid[] connectionIds, bool isExcept)
    {
        using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
        {
            // redis-format: [isExcept, [connectionIds], [raw-bloadcast-format]]
            var writer = new MessagePackWriter(buffer);

            writer.WriteArrayHeader(3);
            writer.Write(isExcept);
            NativeGuidArrayFormatter.Serialize(ref writer, connectionIds);

            writer.WriteArrayHeader(2);
            writer.WriteInt32(methodId);
            MessagePackSerializer.Serialize(ref writer, value, serializerOptions);

            writer.Flush();
            var result = buffer.WrittenSpan.ToArray();
            return result;
        }
    }


    public Task WriteExceptRawAsync(ArraySegment<byte> message, Guid[] exceptConnectionIds, bool fireAndForget)
    {
        // only for the inmemory routing.
        throw new NotSupportedException();
    }

    public Task WriteToRawAsync(ArraySegment<byte> message, Guid[] connectionIds, bool fireAndForget)
    {
        // only for the inmemory routing.
        throw new NotSupportedException();
    }
}
