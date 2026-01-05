using MagicOnion;

namespace AotSample.Shared;

/// <summary>
/// Chat hub receiver interface - defines methods that clients can receive.
/// </summary>
public interface IChatHubReceiver
{
    void OnMessage(string user, string message);
    void OnUserJoined(string user);
    void OnUserLeft(string user);
}

/// <summary>
/// Chat hub interface - defines methods that clients can call.
/// </summary>
public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
{
    Task JoinAsync(string roomName, string userName);
    Task LeaveAsync();
    Task SendMessageAsync(string message);
}
