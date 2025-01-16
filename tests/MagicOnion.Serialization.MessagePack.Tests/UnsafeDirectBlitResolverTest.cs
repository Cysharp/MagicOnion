using MessagePack;
using MessagePack.Resolvers;

namespace MagicOnion.Serialization.MessagePack.Tests;

public class UnsafeDirectBlitResolverTest
{
    [Fact]
    public void SerializeTest()
    {
        UnsafeDirectBlitResolver.Instance.Register<MyStruct>();

        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(UnsafeDirectBlitResolver.Instance, StandardResolver.Instance));

        var s = new MyStruct { X = 10, Y = 99, Z = 999 };

        var bin = MessagePackSerializer.Serialize(s, options, cancellationToken: TestContext.Current.CancellationToken);

        var z = MessagePackSerializer.Deserialize<MyStruct>(bin, options, cancellationToken: TestContext.Current.CancellationToken);

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
