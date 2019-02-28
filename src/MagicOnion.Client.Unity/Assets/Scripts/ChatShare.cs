using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
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
    [MessagePackObject]
    public struct RoomMember
    {
        [Key(0)]
        public readonly string Id;
        [Key(1)]
        public readonly string Name;

        public RoomMember(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Id + ":" + Name;
        }
    }

    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)]
        public virtual RoomMember Sender { get; set; }
        [Key(1)]
        public virtual string Message { get; set; }

        public override string ToString()
        {
            return Sender.Name + ": " + Message;
        }
    }

    [MessagePackObject]
    public class ChatRoomResponse
    {
        [Key(0)]
        public virtual string Id { get; set; }
        [Key(1)]
        public virtual string Name { get; set; }

        public override string ToString()
        {
            return Id + ":" + Name;
        }
    }

    public interface IGamingHubReceiver
    {
        void OnJoin(Player player);
        void OnLeave(Player player);
        void OnMove(Player player);
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


    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        Task<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation);
        Task LeaveAsync();
        Task MoveAsync(Vector3 position, Quaternion rotation);
    }

    public class GamingHubClient : IGamingHubReceiver
    {
        Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

        IGamingHub client;

        public async Task<GameObject> ConnectAsync(Channel grpcChannel, string roomName, string playerName)
        {
            client = StreamingHubClient.Connect<IGamingHub, IGamingHubReceiver>(grpcChannel, this);

            var roomPlayers = await client.JoinAsync(roomName, playerName, Vector3.zero, Quaternion.identity);
            foreach (var player in roomPlayers)
            {
                (this as IGamingHubReceiver).OnJoin(player);
            }

            return players[playerName]; 
        }


        public Task LeaveAsync()
        {
            return client.LeaveAsync();
        }

        public Task MoveAsync(Vector3 position, Quaternion rotation)
        {
            return client.MoveAsync(position, rotation);
        }

        public Task DisposeAsync()
        {
            return client.DisposeAsync();
        }

        public Task WaitForDisconnect()
        {
            return client.WaitForDisconnect();
        }

        void IGamingHubReceiver.OnJoin(Player player)
        {
            Debug.Log("Join Player:" + player.Name);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = player.Name;
            cube.transform.SetPositionAndRotation(player.Position, player.Rotation);
            players[player.Name] = cube;
        }

        void IGamingHubReceiver.OnLeave(Player player)
        {
            Debug.Log("Leave Player:" + player.Name);

            if (players.TryGetValue(player.Name, out var cube))
            {
                GameObject.Destroy(cube);
            }
        }

        void IGamingHubReceiver.OnMove(Player player)
        {
            Debug.Log("Move Player:" + player.Name);

            if (players.TryGetValue(player.Name, out var cube))
            {
                cube.transform.SetPositionAndRotation(player.Position, player.Rotation);
            }
        }
    }
}
