using MemoryPack;
using MessagePack;

namespace PerformanceTest.Shared;

[MemoryPackable]
[MessagePackObject]
public readonly partial struct Vector3
{
    [Key(0)]
    public float X { get; }

    [Key(1)]
    public float Y { get; }

    [Key(2)]
    public float Z { get; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
