using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using MagicOnion.Server.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    /// <summary>
    /// Chat server processing.
    /// One class instance for one connection.
    /// </summary>
    public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
    {
        private IGroup room;
        private string myName;


        public async Task JoinAsync(JoinRequest request)
        {
            this.room = await this.Group.AddAsync(request.RoomName);

            this.myName = request.UserName;

            this.Broadcast(this.room).OnJoin(request.UserName);
        }


        public async Task LeaveAsync()
        {
            await this.room.RemoveAsync(this.Context);

            this.Broadcast(this.room).OnLeave(this.myName);
        }


        public async Task SendMessageAsync(string message)
        {
            var response = new MessageResponse {  UserName = this.myName, Message = message };
            this.Broadcast(this.room).OnSendMessage(response);

            await Task.CompletedTask;
        }


        // It is not called because it is a method as a sample of arguments.
        public Task SampleMethod(List<int> sampleList, Dictionary<int, string> sampleDictionary)
        {
            throw new System.NotImplementedException();
        }
    }
}
