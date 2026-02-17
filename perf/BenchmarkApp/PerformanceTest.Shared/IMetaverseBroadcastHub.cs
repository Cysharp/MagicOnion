using MagicOnion;

namespace PerformanceTest.Shared;

public interface IMetaverseBroadcastHub : IStreamingHub<IMetaverseBroadcastHub, IMetaverseBroadcastHubReceiver>
{
    ValueTask<string> JoinAsync(int targetFps);
    ValueTask<string> LeaveAsync();
    ValueTask UpdatePositionAsync(BroadcastPositionMessage position);

    ValueTask StartBroadcast(int targetFps);
    ValueTask StopBroadcast();
}

public interface IMetaverseBroadcastHubReceiver
{
    void OnBroadcastAllPositions(AllClientsPositionMessage message);
}
