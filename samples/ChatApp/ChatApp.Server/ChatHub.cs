using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Server;

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
        if (this.room is not null)
        {
            await this.room.RemoveAsync(this.Context);
            this.Broadcast(this.room).OnLeave(this.myName);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (this.room is not null)
        {
            var response = new MessageResponse { UserName = this.myName, Message = message };
            this.Broadcast(this.room).OnSendMessage(response);
        }

        await Task.CompletedTask;
    }

    public Task GenerateException(string message)
    {
        throw new Exception(message);
    }

    // It is not called because it is a method as a sample of arguments.
    public Task SampleMethod(List<int> sampleList, Dictionary<int, string> sampleDictionary)
    {
        throw new System.NotImplementedException();
    }

    protected override ValueTask OnConnecting()
    {
        // handle connection if needed.
        Console.WriteLine($"client connected {this.Context.ContextId}");
        return CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        // handle disconnection if needed.
        // on disconnecting, if automatically removed this connection from group.
        return CompletedTask;
    }
}
