using AotSample.Shared;
using MagicOnion.Server.Hubs;

namespace AotSample.Server.Services;

public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = string.Empty;

    public async Task JoinAsync(string roomName, string userName)
    {
        this.userName = userName;
        room = await Group.AddAsync(roomName);
        // Broadcast to all members in the room
        room.All.OnUserJoined(userName);
    }

    public Task LeaveAsync()
    {
        if (room is not null)
        {
            room.All.OnUserLeft(userName);
            room.RemoveAsync(Context);
            room = null;
        }
        return Task.CompletedTask;
    }

    public Task SendMessageAsync(string message)
    {
        if (room is not null)
        {
            room.All.OnMessage(userName, message);
        }
        return Task.CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        if (room is not null)
        {
            room.All.OnUserLeft(userName);
        }
        return default;
    }
}
