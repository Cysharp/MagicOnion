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
            var connection = serviceLocator.GetService<ConnectionMultiplexer>();
            if (connection == null)
            {
                throw new InvalidOperationException("RedisGroup requires add ConnectionMultiplexer to MagicOnionOptions.ServiceLocator before create it. Please try new MagicOnionOptions{DefultServiceLocator.Register(new ConnectionMultiplexer)}");
            }

            return new RedisGroupRepository(resolver, connection);
        }
    }

    public class RedisGroupRepository : IGroupRepository
    {
        IFormatterResolver resolver;
        ConnectionMultiplexer connection;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public RedisGroupRepository(IFormatterResolver resolver, ConnectionMultiplexer connection)
        {
            this.resolver = resolver;
            this.factory = CreateGroup;
            this.connection = connection;
        }

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, factory);
        }

        IGroup CreateGroup(string groupName)
        {
            return new RedisGroup(groupName, resolver, new ConcurrentDictionaryGroup(groupName, this, resolver), connection.GetSubscriber());
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
        RedisChannel channel;
        IFormatterResolver resolver;
        ChannelMessageQueue mq;

        public RedisGroup(string groupName, IFormatterResolver resolver, IGroup inmemoryGroup, ISubscriber redisSubscriber)
        {
            this.GroupName = groupName;
            this.resolver = resolver;
            this.channel = new RedisChannel("MagicOnion.Redis.RedisGroup?groupName=" + groupName, RedisChannel.PatternMode.Literal);
            this.inmemoryGroup = inmemoryGroup;
            this.subscriber = redisSubscriber;

            this.mq = redisSubscriber.Subscribe(channel);
            mq.OnMessage(message => PublishFromRedisToMemoryGroup(message.Message, this.inmemoryGroup));
        }

        public string GroupName { get; }

        public ValueTask AddAsync(ServiceContext context)
        {
            return inmemoryGroup.AddAsync(context);
        }

        public async ValueTask<bool> RemoveAsync(ServiceContext context)
        {
            if (await inmemoryGroup.RemoveAsync(context)) // if inmemoryGroup.Remove succeed, removed from.RedisGroupRepository.
            {
                await mq.UnsubscribeAsync();
                return true;
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

                return group.WriteRawAsync(new ArraySegment<byte>(buffer, offset, buffer.Length - offset), excludes);
            }

            return Task.CompletedTask;
        }

        public Task WriteAllAsync<T>(int methodId, T value)
        {
            return subscriber.PublishAsync(channel, BuildMessage(methodId, value, null));
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId)
        {
            return subscriber.PublishAsync(channel, BuildMessage(methodId, value, new[] { connectionId }));
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds)
        {
            return subscriber.PublishAsync(channel, BuildMessage(methodId, value, connectionIds));
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

        public Task WriteRawAsync(ArraySegment<byte> message, Guid[] exceptConnectionIds)
        {
            // only for the inmemory routing.
            throw new NotSupportedException();
        }
    }
}