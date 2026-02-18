using MemoryPack;
using MessagePack;

namespace PerformanceTest.Shared;

[MemoryPackable]
[MessagePackObject]
public partial struct BroadcastPositionMessage
{
    // Cached instance for performance testing
    public static readonly BroadcastPositionMessage Cached = new (999, new Vector3(100.0f, 50.0f, 200.0f));

    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public Vector3 Position { get; set; }

    public BroadcastPositionMessage(int id, Vector3 position)
    {
        Id = id;
        Position = position;
    }
}
