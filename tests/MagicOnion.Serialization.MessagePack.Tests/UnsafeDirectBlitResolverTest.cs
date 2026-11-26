using MessagePack;
#pragma warning disable CS0618 // Type or member is obsolete

namespace MagicOnion.Serialization.MessagePack.Tests;

public class UnsafeDirectBlitResolverTest
{
    [Fact]
    public void SerializeTest()
    {
        UnsafeDirectBlitResolver.Instance.Register<MyStruct>();

        var options = new MessagePackSerializerOptions([UnsafeDirectBlitResolver.Instance, MessagePackFormatterFactory.Default]);

        var s = new MyStruct { X = 10, Y = 99, Z = 999 };

        var bin = MessagePackSerializer.Serialize(s, options);

        var z = MessagePackSerializer.Deserialize<MyStruct>(bin, options);

        Assert.Equal(10, z.X);
        Assert.Equal(99, z.Y);
        Assert.Equal(999, z.Z);
    }
}

public struct MyStruct
{
    public int X;
    public int Y;
    public int Z;
}
