namespace PerformanceTest.Shared;

public class BroadcastAllClientsPositionMessage 
{
    public long FrameNumber { get; set; }
    public BroadcastPositionMessage[] Positions { get; set; } = [];
}
