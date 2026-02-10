using MagicOnion;

namespace PerformanceTest.Shared;

public interface IPerTestBroadcastHub : IStreamingHub<IPerTestBroadcastHub, IPerTestBroadcastHubReceiver>
{
    // Broadcast
    ValueTask<string> JoinGroupAsync();
    ValueTask<string> LeaveGroupAsync();

    // 1 client send position to server, server broadcast to all clients
    ValueTask UpdatePositionAsync(BroadcastPositionMessage position);
}

public interface IPerTestBroadcastHubReceiver
{
    void OnMessage(BroadcastPositionMessage message);
}
