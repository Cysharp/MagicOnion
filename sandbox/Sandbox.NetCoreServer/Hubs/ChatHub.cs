using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer.Hubs
{
    public interface IMessageReceiver2
    {
        void OnReceiveMessage(string senderUser, string message);
    }

    public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver2>
    {
        Task JoinAsync(string userName, string roomName);
        Task LeaveAsync();
        Task SendMessageAsync(string message);
    }

    public class ChatHub : StreamingHubBase<IChatHub, IMessageReceiver2>, IChatHub
    {
        // insantiate per user connected and live while connecting.
        string userName;
        IGroup room;

        public async Task JoinAsync(string userName, string roomName)
        {
            this.userName = userName;
            this.room = await Group.AddAsync("InMemoryRoom:" + roomName, this.Context);
        }

        public Task SendMessageAsync(string message)
        {
            // broadcast to connected group(same roomname members).
            Broadcast(room).OnReceiveMessage(this.userName, message);

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
