using MemoryPack;
using MessagePack;

namespace PerformanceTest.Shared;

[MemoryPackable]
[MessagePackObject]
public partial class AllClientsPositionMessage
{
    [Key(0)]
    public long FrameNumber { get; set; }
    
    [Key(1)]
    public BroadcastPositionMessage[] Positions { get; set; }

    [MemoryPackConstructor]
    public AllClientsPositionMessage(long frameNumber, BroadcastPositionMessage[] positions)
    {
        FrameNumber = frameNumber;
        Positions = positions;
    }
}
