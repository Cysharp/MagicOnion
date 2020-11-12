using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sandbox.AspNetCore.Models;

namespace Sandbox.AspNetCore.Hubs
{
    public interface IMessageReceiver2
    {
        void OnReceiveMessage(string senderUser, string message);
        void Foo2(Foo foo2);
    }

    public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver2>
    {
        [MethodId(100)]
        Task JoinAsync(string userName, string roomName);
        Task LeaveAsync();
        Task SendMessageAsync(string message);
    }

    [GroupConfiguration(typeof(ConcurrentDictionaryGroupRepositoryFactory))]
    public class ChatHub : StreamingHubBase<IChatHub, IMessageReceiver2>, IChatHub
    {
        // insantiate per user connected and live while connecting.
        string userName;
        IGroup room;

        public ChatHub(ILogger<ChatHub> hoggaer)
        {

        }

        public async Task JoinAsync(string userName, string roomName)
        {
            this.userName = userName;
            this.room = await Group.AddAsync("InMemoryRoom:" + roomName);
        }

        public Task SendMessageAsync(string message)
        {
            // broadcast to connected group(same roomname members).
            if (room != null)
            {
                Broadcast(room).OnReceiveMessage(this.userName, message);
            }

            return Task.CompletedTask;
        }

        public async Task LeaveAsync()
        {
            BroadcastExceptSelf(room).OnReceiveMessage(userName, "SYSTEM_MESSAGE_LEAVE_USER");
            await room.RemoveAsync(this.Context);
        }

        protected override ValueTask OnConnecting()
        {
            return CompletedTask; // you can hook connecting event.
        }

        protected override async ValueTask OnDisconnected()
        {
            if (room != null)
            {
                await room.RemoveAsync(this.Context); // remove from group.
            }
        }
    }
}
