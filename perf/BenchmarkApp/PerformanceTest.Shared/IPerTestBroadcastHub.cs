using MagicOnion;

namespace PerformanceTest.Shared;

public interface IPerTestBroadcastHub : IStreamingHub<IPerTestBroadcastHub, IPerTestBroadcastHubReceiver>
{
    // Broadcast
    ValueTask<string> JoinGroupAsync();
    ValueTask<string> LeaveGroupAsync();
}

public interface IPerTestBroadcastHubReceiver
{
    void OnMessage(SimpleResponse response);
}
