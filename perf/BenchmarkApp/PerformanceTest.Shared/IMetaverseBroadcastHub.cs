using MagicOnion;

namespace PerformanceTest.Shared;

public interface IMetaverseBroadcastHub : IStreamingHub<IMetaverseBroadcastHub, IMetaverseBroadcastHubReceiver>
{
    ValueTask<string> JoinAsync(int targetFps);
    ValueTask<string> LeaveAsync();
    ValueTask UpdatePositionAsync(BroadcastPositionMessage position);
}

public interface IMetaverseBroadcastHubReceiver
{
    void OnBroadcastAllPositions(AllClientsPositionMessage message);
}
