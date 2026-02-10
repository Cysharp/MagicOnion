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
    [Transport(TransportReliability.Unreliable)]
    void OnMessage(BroadcastPositionMessage message);
}
