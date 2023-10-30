using MagicOnion.Serialization.MessagePack;
using MessagePack.Resolvers;

namespace MagicOnion.Shared.Tests;

public class UnsafeDirectBlitResolverTest
{
    [Fact]
    public void SerializeTest()
    {
        UnsafeDirectBlitResolver.Instance.Register<MyStruct>();

        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(UnsafeDirectBlitResolver.Instance, StandardResolver.Instance));

        var s = new MyStruct { X = 10, Y = 99, Z = 999 };

        var bin = MessagePackSerializer.Serialize(s, options);

        var z = MessagePackSerializer.Deserialize<MyStruct>(bin, options);

        z.X.Should().Be(10);
        z.Y.Should().Be(99);
        z.Z.Should().Be(999);
    }
}

public struct MyStruct
{
    public int X;
    public int Y;
    public int Z;
}
