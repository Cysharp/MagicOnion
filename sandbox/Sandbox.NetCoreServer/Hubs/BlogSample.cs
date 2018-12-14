#pragma warning disable CS1998

using MagicOnion;
using MagicOnion.Server.Hubs;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sandbox.NetCoreServer.Hubs
{
    public interface IGamingHubReceiver
    {
        void OnJoin(Player player);
        void OnLeave(Player player);
        void OnMove(Player player);
    }

    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        Task<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation);
        Task LeaveAsync();
        Task MoveAsync(Vector3 position, Quaternion rotation);
    }

    [MessagePackObject]
    public class Player
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public Vector3 Position { get; set; }
        [Key(2)]
        public Quaternion Rotation { get; set; }
    }


    public class GamingHub : StreamingHubBase<IGamingHub, IGamingHubReceiver>, IGamingHub
    {
        IGroup room;
        Player self;
        IInMemoryStorage<Player> storage;

        public async Task<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation)
        {
            self = new Player() { Name = userName, Position = position, Rotation = rotation };

            (room, storage) = await Group.AddAsync(roomName, self);

            Broadcast(room).OnJoin(self);

            return storage.AllValues.ToArray();
        }

        public async Task LeaveAsync()
        {
            await room.RemoveAsync(this.Context);
            Broadcast(room).OnLeave(self);
        }

        public async Task MoveAsync(Vector3 position, Quaternion rotation)
        {
            self.Position = position;
            self.Rotation = rotation;
            Broadcast(room).OnMove(self);
        }

        protected override async ValueTask OnDisconnected()
        {
            if (room != null)
            {
                await room.RemoveAsync(this.Context);
            }
        }
    }


}

#pragma warning restore CS1998