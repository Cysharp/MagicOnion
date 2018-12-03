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
    public interface IMessageReceiver
    {
        Task OnReceiveMessage(string senderUser, string message);
    }

    public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver>
    {
        Task<Nil> JoinAsync(string userName, string roomName);
        Task<Nil> LeaveAsync();
        Task SendMessageAsync(string message);
    }

    public class ChatHub : StreamingHubBase<IChatHub, IMessageReceiver>, IChatHub
    {
        // insantiate per user connected and live while connecting.
        string userName;
        IGroup room;

        // return Task<T> wait server completed.
        public async Task<Nil> JoinAsync(string userName, string roomName)
        {
            this.userName = userName;
            this.room = await Group.AddAsync("InMemoryRoom:" + roomName, this.Context);

            return Nil.Default;
        }

        // return Task is fire and forget(does not wait server completed).
        public async Task SendMessageAsync(string message)
        {
            // broadcast to connected group(same roomname members).
            await Broadcast(room).OnReceiveMessage(this.userName, message);
        }

        public async Task<Nil> LeaveAsync()
        {
            await BroadcastExceptSelf(room).OnReceiveMessage(userName, "SYSTEM_MESSAGE_LEAVE_USER");
            await room.RemoveAsync(this.Context);

            return Nil.Default;
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
