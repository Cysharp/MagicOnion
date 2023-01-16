using MemoryPack;
using MessagePack;

namespace PerformanceTest.Shared;

[MessagePackObject]
[MemoryPackable]
public partial class ComplexResponse
{
    public static ComplexResponse Cached { get; } = new ComplexResponse
    {
        Value1 = true,
        Value2 = 1234567,
        Value3 = new InnerObject1
        {
            Value1 = 987654321,
            Value2 = "FooBarBaz🚀こんにちは世界",
            Value3 = 1234567890123,
            Value4 = true,
            Value5 = 123456789,
            Value6 = 0,
            Value7 = 20,
            Value8 = 256,
            Value9 = DateTimeOffset.Now,
        },
        Value4 = new InnerObject2[]
        {
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
            new InnerObject2 { Value1 = 123456789, Value2 = 123, Value3 = 4567, Value4 = 89101112 },
        },
    };

    [Key(0)]
    public bool Value1 { get; set; }
    [Key(1)]
    public int Value2 { get; set; }
    [Key(2)]
    public InnerObject1 Value3 { get; set; } = default!;
    [Key(3)]
    public IReadOnlyList<InnerObject2> Value4 { get; set; } = default!;
}

[MessagePackObject]
[MemoryPackable]
public partial class InnerObject1
{
    [Key(0)]
    public int Value1 { get; set; }
    [Key(1)]
    public string Value2 { get; set; } = default!;
    [Key(2)]
    public long Value3 { get; set; }
    [Key(3)]
    public bool Value4 { get; set; }
    [Key(4)]
    public int Value5 { get; set; }
    [Key(5)]
    public int Value6 { get; set; }
    [Key(6)]
    public int Value7 { get; set; }
    [Key(7)]
    public int Value8 { get; set; }
    [Key(8)]
    public DateTimeOffset Value9 { get; set; }
}
    
[MessagePackObject]
[MemoryPackable]
public partial struct InnerObject2
{
    [Key(0)]
    public long Value1 { get; set; }
    [Key(1)]
    public int Value2 { get; set; }
    [Key(2)]
    public int Value3 { get; set; }
    [Key(3)]
    public int Value4 { get; set; }
}
