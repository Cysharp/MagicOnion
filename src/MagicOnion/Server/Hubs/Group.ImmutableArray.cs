using MagicOnion.Utils;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class ImmutableArrayGroupRepositoryFactory : IGroupRepositoryFactory
    {
        public IGroupRepository CreateRepository(IServiceLocator serviceLocator)
        {
            return new ImmutableArrayGroupRepository(serviceLocator.GetService<IFormatterResolver>());
        }
    }

    public class ImmutableArrayGroupRepository : IGroupRepository
    {
        IFormatterResolver resolver;

        readonly Func<string, IGroup> factory;
        ConcurrentDictionary<string, IGroup> dictionary = new ConcurrentDictionary<string, IGroup>();

        public ImmutableArrayGroupRepository(IFormatterResolver resolver)
        {
            this.resolver = resolver;
            this.factory = CreateGroup;
        }

        public IGroup GetOrAdd(string groupName)
        {
            return dictionary.GetOrAdd(groupName, factory);
        }

        IGroup CreateGroup(string groupName)
        {
            return new ImmutableArrayGroup(groupName, this, resolver);
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

    public class ImmutableArrayGroup : IGroup
    {
        readonly object gate = new object();
        readonly IGroupRepository parent;
        readonly IFormatterResolver resolver;

        ImmutableArray<ServiceContext> members;

        public string GroupName { get; }

        public ImmutableArrayGroup(string groupName, IGroupRepository parent, IFormatterResolver resolver)
        {
            this.GroupName = groupName;
            this.parent = parent;
            this.resolver = resolver;
            this.members = ImmutableArray<ServiceContext>.Empty;
        }

        public ValueTask<int> GetMemberCountAsync()
        {
            return new ValueTask<int>(members.Length);
        }

        public ValueTask AddAsync(ServiceContext context)
        {
            lock (gate)
            {
                members = members.Add(context);
            }
            return default(ValueTask);
        }

        public ValueTask<bool> RemoveAsync(ServiceContext context)
        {
            lock (gate)
            {
                members = members.Remove(context);
                if (members.Length == 0)
                {
                    if (parent.TryRemove(GroupName))
                    {
                        return new ValueTask<bool>(true);
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
                    WriteInAsyncLockVoid(source[i], message);
                }
                return Task.CompletedTask;
            }
            else
            {
                var promise = new ReservedWhenAllPromise(source.Length);
                for (int i = 0; i < source.Length; i++)
                {
                    promise.Add(WriteInAsyncLock(source[i], message));
                }

                return promise.AsValueTask().AsTask();
            }
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid connectionId, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);

            var source = members;
            if (fireAndForget)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].ContextId != connectionId)
                    {
                        WriteInAsyncLockVoid(source[i], message);
                    }
                }
                return Task.CompletedTask;
            }
            else
            {
                var promise = new ReservedWhenAllPromise(source.Length);
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].ContextId == connectionId)
                    {
                        promise.Add(default(ValueTask));
                    }
                    else
                    {
                        promise.Add(WriteInAsyncLock(source[i], message));
                    }
                }

                return promise.AsValueTask().AsTask();
            }
        }

        public Task WriteExceptAsync<T>(int methodId, T value, Guid[] connectionIds, bool fireAndForget)
        {
            var message = BuildMessage(methodId, value);

            var source = members;
            if (fireAndForget)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    foreach (var item in connectionIds)
                    {
                        if (source[i].ContextId == item)
                        {
                            goto NEXT;
                        }
                    }

                    WriteInAsyncLockVoid(source[i], message);
                NEXT:
                    continue;
                }
                return Task.CompletedTask;
            }
            else
            {
                var promise = new ReservedWhenAllPromise(source.Length);
                for (int i = 0; i < source.Length; i++)
                {
                    foreach (var item in connectionIds)
                    {
                        if (source[i].ContextId == item)
                        {
                            promise.Add(default(ValueTask));
                            goto NEXT;
                        }
                    }

                    promise.Add(WriteInAsyncLock(source[i], message));
                NEXT:
                    continue;
                }

                return promise.AsValueTask().AsTask();
            }
        }

        public Task WriteRawAsync(ArraySegment<byte> msg, Guid[] exceptConnectionIds, bool fireAndForget)
        {
            // oh, copy is bad but current gRPC interface only accepts byte[]...
            var message = new byte[msg.Count];
            Array.Copy(msg.Array, msg.Offset, message, 0, message.Length);

            var source = members;
            if (fireAndForget)
            {
                if (exceptConnectionIds == null)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        WriteInAsyncLockVoid(source[i], message);
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
                        WriteInAsyncLockVoid(source[i], message);

                    NEXT:
                        continue;
                    }
                }
                return Task.CompletedTask;
            }
            else
            {
                var promise = new ReservedWhenAllPromise(source.Length);

                if (exceptConnectionIds == null)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        promise.Add(WriteInAsyncLock(source[i], message));
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
                                promise.Add(default(ValueTask));
                                goto NEXT;
                            }
                        }

                        promise.Add(WriteInAsyncLock(source[i], message));
                    NEXT:
                        continue;
                    }
                }

                return promise.AsValueTask().AsTask();
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

        // async void is better than return Task when fire-and-forget to avoid create unnecessary promise.
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
    }
}