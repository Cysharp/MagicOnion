using MemoryPack;
using MessagePack;

namespace PerformanceTest.Shared;

[MessagePackObject]
[MemoryPackable]
public partial class SimpleRequest
{
    public static SimpleRequest Cached { get; } = new SimpleRequest
    {
        Payload = [],
        ResponseSize = 0,
        UseCache = true,
    };

    [Key(0)]
    public byte[] Payload { get; set; } = default!;
    [Key(1)]
    public int ResponseSize { get; set; }
    [Key(2)]
    public bool UseCache { get; set; }
}

[MessagePackObject]
[MemoryPackable]
public partial class SimpleResponse
{
    public static SimpleResponse Cached { get; } = new SimpleResponse
    {
        Payload = [],
    };

    [Key(0)]
    public byte[] Payload { get; set; } = default!;
}
