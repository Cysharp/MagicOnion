using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;
using MessagePack.Formatters;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MagicOnion.Redis
{
    internal static class NativeGuidArrayFormatter
    {
        static readonly IMessagePackFormatter<Guid> formatter = BinaryGuidFormatter.Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Serialize(ref byte[] bytes, int offset, Guid[] value)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var start = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                offset += formatter.Serialize(ref bytes, offset, value[i], null);
            }
            return offset - start;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid[] Deserialize(byte[] bytes, int offset, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var start = offset;
            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var result = new Guid[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = formatter.Deserialize(bytes, offset, null, out readSize);
                offset += readSize;
            }
            readSize = offset - start;
            return result;
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
            if (await inmemoryGroup.RemoveAsync(context))
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