using MagicOnion;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface IChatRoomStreaming// : IStreamingService
    {
        Task<ServerStreamingResult<RoomMember>> OnJoin();
        Task<ServerStreamingResult<RoomMember>> OnLeave();
        Task<ServerStreamingResult<ChatMessage>> OnMessageReceived();
    }

    public interface IChatRoomCommand
    {
        /// <summary>Create new room.</summary>
        UnaryResult<ChatRoomResponse> CreateNewRoom(string roomName, string nickName);

        UnaryResult<ChatRoomResponse> Join(string roomId, string nickName);

        UnaryResult<ChatRoomResponse[]> GetRooms();

        UnaryResult<bool> Leave(string roomId);

        UnaryResult<bool> SendMessage(string roomId, string message);
    }

    public interface IChatRoomService : IService<IChatRoomService>, IChatRoomCommand, IChatRoomStreaming
    {


    }
}
