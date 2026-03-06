using MemoryPack;
using MessagePack;

namespace PerformanceTest.Shared;

[MemoryPackable]
[MessagePackObject]
public readonly partial struct Vector3
{
    public static readonly Vector3 Zero = new(0.0f, 0.0f, 0.0f);
    public static readonly Vector3 One = new(1.0f, 1.0f, 1.0f);

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
