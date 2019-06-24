using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MagicOnion.Redis
{
    public class RedisGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository(IServiceLocator serviceLocator)
        {
            var resolver = serviceLocator.GetService<IFormatterResolver>();
            var logger = serviceLocator.GetService<IMagicOnionLogger>();
            var connection = serviceLocator.GetService<ConnectionMultiplexer>();
            if (connection == null)
            {
                throw new InvalidOperationException("RedisGroup requires add ConnectionMultiplexer to MagicOnionOptions.ServiceLocator before create it. Please try new MagicOnionOptions{DefultServiceLocator.Register(new ConnectionMultiplexer)}");
            }

            return new RedisGroupRepository(resolver, connection, logger);
        }
    }

    public class RedisGroupRepository : IGroupRepository
    {
        IFormatterResolver resolver;
        IMagicOnionLogger logger;
        ConnectionMultiplexer connection;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public RedisGroupRepository(IFormatterResolver resolver, ConnectionMultiplexer connection, IMagicOnionLogger logger)
        {
            this.resolver = resolver;
            this.logger = logger;
            this.factory = CreateGroup;
            this.connection = connection;
        }

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, factory);
        }

        IGroup CreateGroup(string groupName)
        {
            return new RedisGroup(groupName, resolver, new ConcurrentDictionaryGroup(groupName, this, resolver, logger), connection.GetSubscriber(), connection.GetDatabase());
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
        IFormatterResolver resolver;
        ChannelMessageQueue mq;
        RedisKey counterKey;

        public RedisGroup(string groupName, IFormatterResolver resolver, IGroup inmemoryGroup, ISubscriber redisSubscriber, IDatabaseAsync database)
        {
            this.GroupName = groupName;
            this.resolver = resolver;
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

        public bool IsEmpty
        {
            get
            {
                throw new NotSupportedException("IsEmpty does not support in RedisGroup.");
            }
        }

        public (bool, T) AtomicInvoke<T>(string key, Func<T> action)
        {
            throw new NotSupportedException("AtomicInvoke does not support in RedisGroup.");
        }

        public bool AtomicInvoke(string key, Action action)
        {
            throw new NotSupportedException("AtomicInvoke does not support in RedisGroup.");
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
            var offset = 0;
            int readSize;
            var len1 = MessagePackBinary.ReadArrayHeader(buffer, offset, out readSize);
            offset += readSize;
            if (len1 == 2)
            {
                var excludes = NativeGuidArrayFormatter.Deserialize(buffer, offset, out readSize);
                offset += readSize;

                return group.WriteRawAsync(new ArraySegment<byte>(buffer, offset, buffer.Length - offset), excludes, fireAndForget: false);
            }

            return Task.CompletedTask;
        }

        public ValueTask<int> GetMemberCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task WriteAllAsync<T>(int methodId, T value, bool fireAndForget)
        {
            var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
            return subscriber.PublishAsync(channel, BuildMessage(methodId, value, null), flags);
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget)
        {
            var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
            return subscriber.PublishAsync(channel, BuildMessage(methodId, value, new[] { connectionId }), flags);
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
        {
            var flags = (fireAndForget) ? CommandFlags.FireAndForget : CommandFlags.None;
            return subscriber.PublishAsync(channel, BuildMessage(methodId, value, connectionIds), flags);
        }

        byte[] BuildMessage<T>(int methodId, T value, Guid[] exceptIds)
        {
            var rent = System.Buffers.ArrayPool<byte>.Shared.Rent(ushort.MaxValue);
            var buffer = rent;
            try
            {
                var offset = 0;

                // redis-format: [[exceptIds], [raw-bloadcast-format]]
                offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 2);
                offset += NativeGuidArrayFormatter.Serialize(ref buffer, offset, exceptIds);

                offset += MessagePackBinary.WriteArrayHeader(ref buffer, offset, 2);
                offset += MessagePackBinary.WriteInt32(ref buffer, offset, methodId);
                offset += LZ4MessagePackSerializer.SerializeToBlock(ref buffer, offset, value, resolver);

                var result = MessagePackBinary.FastCloneWithResize(buffer, offset);
                return result;
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(rent);
            }
        }

        public Task WriteRawAsync(ArraySegment<byte> message, Guid[] exceptConnectionIds, bool fireAndForget)
        {
            // only for the inmemory routing.
            throw new NotSupportedException();
        }
    }
}