using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace Sandbox.ConsoleServer.Services
{
    // Room is "not" serializable
    public class ChatRoom
    {
        StreamingContextGroup<string, RoomMember, IChatRoomStreaming> group;

        public string Id { get; }
        public string Name { get; }

        public int MemberCount => group.Count;

        public ChatRoom(string id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.group = new StreamingContextGroup<string, RoomMember, IChatRoomStreaming>();
        }

        public void AddMember(RoomMember member, StreamingContextRepository<IChatRoomStreaming> streaming)
        {
            this.group.Add(member.Id, member, streaming);
        }

        public void RemoveMember(string memberId)
        {
            this.group.Remove(memberId);
        }

        public RoomMember? GetMember(string memberId)
        {
            var v = this.group.Get(memberId);
            return (v == null) ? (RoomMember?)null : v.Item1;
        }

        public Task BroadcastJoinAsync(RoomMember joinMember)
        {
            return this.group.BroadcastAllAsync(x => x.OnJoin, joinMember);
        }

        public Task BroadcastLeaveAsync(RoomMember leaveMember)
        {
            return this.group.BroadcastAllAsync(x => x.OnLeave, leaveMember);
        }

        public Task BroadcastMessageAsync(RoomMember sendMember, string message)
        {
            return this.group.BroadcastAllAsync(x => x.OnMessageReceived, new ChatMessage { Sender = sendMember, Message = message });
        }

        public ChatRoomResponse ToChatRoomResponse()
        {
            return new ChatRoomResponse { Id = Id, Name = Name };
        }
    }

    

    public class StreamingContextGroup<TKey, TStreamingService>
        where TStreamingService : IStreamingService
    {
    }

    public class StreamingContextGroup<TKey, TValue, TStreamingService>
        where TStreamingService : IStreamingService
    {
        ImmutableDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>> repositories;

        public int Count
        {
            get
            {
                return repositories.Count;
            }
        }

        public IEnumerable<TKey> Keys()
        {
            return repositories.Keys;
        }

        public IEnumerable<KeyValuePair<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>> KeyValues()
        {
            return repositories.AsEnumerable();
        }

        public StreamingContextGroup()
        {
            repositories = ImmutableDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>.Empty;
        }

        public StreamingContextGroup(IEqualityComparer<TKey> comparer)
        {
            repositories = ImmutableDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>.Empty.WithComparers(comparer);
        }

        public void Add(TKey key, TValue value, StreamingContextRepository<TStreamingService> repository)
        {
            if (repository.IsDisposed) return;
            ImmutableInterlocked.Update(ref repositories, (x, y) => x.Add(y.Item1, Tuple.Create(y.Item2, y.Item3)), Tuple.Create(key, value, repository));
        }

        public void Remove(TKey key)
        {
            ImmutableInterlocked.Update(ref repositories, (x, y) => x.Remove(y), key);
        }

        public Tuple<TValue, StreamingContextRepository<TStreamingService>> Get(TKey key)
        {
            Tuple<TValue, StreamingContextRepository<TStreamingService>> v;
            return repositories.TryGetValue(key, out v) ? v : null;
        }

        public TValue GetValue(TKey key)
        {
            var value = Get(key);
            return (value != null) ? value.Item1 : default(TValue);
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> All()
        {
            return repositories.Values;
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> AllExcept(TKey exceptKey)
        {
            var comparer = repositories.KeyComparer;
            return repositories.Where(x => !comparer.Equals(x.Key, exceptKey)).Select(x => x.Value);
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> AllExcept(params TKey[] exceptKeys)
        {
            var comparer = repositories.KeyComparer;
            var set = new HashSet<TKey>(exceptKeys, comparer);
            return repositories.Where(x => !set.Equals(x.Key)).Select(x => x.Value);
        }

        public async Task BroadcastAllAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(repositories.Values.Select(x =>
                {
                    return AwaitErrorHandling(x.Item2.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in repositories.Values)
                {
                    await AwaitErrorHandling(item.Item2.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey exceptKey, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(AllExcept(exceptKey).Select(x =>
                {
                    return AwaitErrorHandling(x.Item2.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in AllExcept(exceptKey))
                {
                    await AwaitErrorHandling(item.Item2.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey[] exceptKeys, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(AllExcept(exceptKeys).Select(x =>
                {
                    return AwaitErrorHandling(x.Item2.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in AllExcept(exceptKeys))
                {
                    await AwaitErrorHandling(item.Item2.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        async Task AwaitErrorHandling(Task task, bool ignoreError)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (RpcException ex)
            {
                if (ignoreError)
                {
                    GrpcEnvironment.Logger.Error(ex, "logged but ignore error from StreamingContextGroup.BroadcastAll");
                }
                else
                {
                    throw;
                }
            }
        }
    }

    internal interface IStreamingContextInfo
    {
        object ServerStreamingContext { get; }
        void Complete();
    }

    public class StreamingContextInfo<T> : IStreamingContextInfo
    {
        readonly TaskCompletionSource<object> tcs;
        readonly object serverStreamingContext;

        object IStreamingContextInfo.ServerStreamingContext
        {
            get
            {
                return serverStreamingContext;
            }
        }

        internal StreamingContextInfo(TaskCompletionSource<object> tcs, object serverStreamingContext)
        {
            this.tcs = tcs;
            this.serverStreamingContext = serverStreamingContext;
        }

        public TaskAwaiter<ServerStreamingResult<T>> GetAwaiter()
        {
            return tcs.Task.ContinueWith(_ => default(ServerStreamingResult<T>)).GetAwaiter();
        }

        public void Complete()
        {
            tcs.TrySetResult(null);
        }
    }

    public class StreamingContextRepository<TService> : IDisposable
        where TService : IStreamingService
    {
        bool isDisposed;
        TService dummyInstance;

        readonly ConcurrentDictionary<MethodInfo, IStreamingContextInfo> streamingContext = new ConcurrentDictionary<MethodInfo, IStreamingContextInfo>();

        public bool IsDisposed => isDisposed;

        public StreamingContextInfo<TResponse> RegisterStreamingMethod<TResponse>(TService self, Func<Task<ServerStreamingResult<TResponse>>> methodSelector)
        {
            if (dummyInstance == null)
            {
                dummyInstance = (TService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(self.GetType());
            }

            var context = self.GetServerStreamingContext<TResponse>();

            var tcs = new TaskCompletionSource<object>();

            // 接続切断時にちゅーぶらりんになるのはアレなので、これで動かしてあげる。
            context.ServiceContext.GetConnectionContext().ConnectionStatus.Register(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
            }, tcs);

            var info = new StreamingContextInfo<TResponse>(tcs, context);
            streamingContext[methodSelector.Method] = info;
            return info;
        }

        public async Task WriteAsync<TResponse>(Func<TService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value)
        {
            IStreamingContextInfo streamingContextObject;
            if (streamingContext.TryGetValue(methodSelector.Method, out streamingContextObject))
            {
                var context = streamingContextObject.ServerStreamingContext as ServerStreamingContext<TResponse>;
                await context.WriteAsync(value).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("Does not exists streaming context. :" + methodSelector.Method.Name);
            }
        }

        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("StreamingContextRepository");
            isDisposed = true;

            // complete all.
            foreach (var item in streamingContext)
            {
                item.Value.Complete();
            }
        }
    }

    // In-Memory Room Repository.
    public class RoomRepository
    {
        public static RoomRepository Default = new RoomRepository();

        ConcurrentDictionary<string, ChatRoom> rooms = new ConcurrentDictionary<string, ChatRoom>();

        // use ddefault only...
        RoomRepository()
        {
        }

        public void AddRoom(ChatRoom room)
        {
            rooms[room.Id] = room;
        }

        public ChatRoom GetRoom(string roomId)
        {
            return rooms[roomId];
        }

        public ChatRoom RemoveRoom(string roomId)
        {
            ChatRoom room;
            return rooms.TryRemove(roomId, out room)
                ? room
                : null;
        }

        public ICollection<ChatRoom> GetRooms()
        {
            return rooms.Values;
        }
    }

    // This class requires Heartbeat.Connect
    public class ChatRoomService : ServiceBase<IChatRoomService>, IChatRoomService
    {
        // Helper Common Methods
        static string GetMyId(ConnectionContext context, ChatRoom room)
        {
            return context.Items[$"RoomService{room.Id}.MyId"] as string;
        }

        static void SetMyId(ConnectionContext context, ChatRoom room, string id)
        {
            context.Items[$"RoomService{room.Id}.MyId"] = id;
        }

        // RoomCommand

        public Task<UnaryResult<ChatRoomResponse>> CreateNewRoom(string roomName, string nickName)
        {
            var room = new ChatRoom(Guid.NewGuid().ToString(), roomName);
            var member = new RoomMember(Guid.NewGuid().ToString(), nickName);
            room.AddMember(member, GetStreamingContextRepository());
            RoomRepository.Default.AddRoom(room);

            var connectionContext = this.GetConnectionContext();
            SetMyId(connectionContext, room, member.Id);

            var roomId = room.Id;
            this.GetConnectionContext().ConnectionStatus.Register(state =>
            {
                var t = (Tuple<string, string>)state;
                LeaveCore(t.Item1, t.Item2).Wait();
            }, Tuple.Create(room.Id, member.Id));

            return Task.FromResult(UnaryResult(room.ToChatRoomResponse()));
        }

        public Task<UnaryResult<ChatRoomResponse[]>> GetRooms()
        {
            return Task.FromResult(UnaryResult(RoomRepository.Default.GetRooms().Select(x => x.ToChatRoomResponse()).ToArray()));
        }

        public async Task<UnaryResult<ChatRoomResponse>> Join(string roomId, string nickName)
        {
            var room = RoomRepository.Default.GetRoom(roomId);
            var newMember = new RoomMember(Guid.NewGuid().ToString(), nickName);
            room.AddMember(newMember, GetStreamingContextRepository());

            await room.BroadcastJoinAsync(newMember);

            return UnaryResult(room.ToChatRoomResponse());
        }

        public async Task<UnaryResult<bool>> Leave(string roomId)
        {
            var connectionContext = this.GetConnectionContext();
            var room = RoomRepository.Default.GetRoom(roomId);
            if (room == null) return UnaryResult(false);
            await LeaveCore(roomId, GetMyId(connectionContext, room));
            return UnaryResult(true);
        }

        // called from ConnectionStatus.Register so should be static.
        static async Task LeaveCore(string roomId, string myId)
        {
            var room = RoomRepository.Default.GetRoom(roomId);
            if (room == null) return;

            var self = room.GetMember(myId);
            if (self == null) return;

            room.RemoveMember(myId);
            if (room.MemberCount == 0)
            {
                RoomRepository.Default.RemoveRoom(roomId);
            }
            else
            {
                await room.BroadcastLeaveAsync(self.Value);
            }
        }

        public Task<UnaryResult<bool>> SendMessage(string roomId, string message)
        {
            var room = RoomRepository.Default.GetRoom(roomId);
            var myId = GetMyId(this.GetConnectionContext(), room);
            var self = room.GetMember(myId);
            if (self == null) return Task.FromResult(UnaryResult(false));

            RoomRepository.Default.GetRoom(roomId).BroadcastMessageAsync(self.Value, message);
            return Task.FromResult(UnaryResult(true));
        }

        // RoomStreaming

        StreamingContextRepository<IChatRoomStreaming> GetStreamingContextRepository()
        {
            var connection = this.GetConnectionContext();
            var item = connection.Items.GetOrAdd("RoomStreamingStreamingContextRepository", _ => new Lazy<StreamingContextRepository<IChatRoomStreaming>>(() =>
            {
                return new StreamingContextRepository<IChatRoomStreaming>();
            }));
            return (item as Lazy<StreamingContextRepository<IChatRoomStreaming>>).Value;
        }

        public async Task<ServerStreamingResult<RoomMember>> OnJoin()
        {
            return await GetStreamingContextRepository().RegisterStreamingMethod(this, OnJoin);
        }

        public async Task<ServerStreamingResult<RoomMember>> OnLeave()
        {
            return await GetStreamingContextRepository().RegisterStreamingMethod(this, OnLeave);
        }

        public async Task<ServerStreamingResult<ChatMessage>> OnMessageReceived()
        {
            return await GetStreamingContextRepository().RegisterStreamingMethod(this, OnMessageReceived);
        }
    }
}
