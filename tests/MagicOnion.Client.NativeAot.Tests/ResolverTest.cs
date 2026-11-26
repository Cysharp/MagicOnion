namespace MagicOnion.Client.NativeAot.Tests;

// NOTE: These tests use the generated formatter registrations for IUnaryTestService.
public sealed class ResolverTest
{
    [Test]
    public async Task MyObject()
    {
        // Act
        var bytes = MessagePackSerializer.Serialize(new MyObject(12345), MessagePackSerializerOptions.DefaultAot);

        // Assert
        await Assert.That(string.Join(", ", bytes.Select(x => $"0x{x:x2}"))).IsEqualTo("0x91, 0xcd, 0x30, 0x39");
    }

    [Test]
    public async Task DynamicArgumentTuple()
    {
        // Act
        var bytes = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, string>(12345, "Hello world!"), MessagePackSerializerOptions.DefaultAot);

        // Assert
        await Assert.That(string.Join(", ", bytes.Select(x => $"0x{x:x2}"))).IsEqualTo("0x92, 0xcd, 0x30, 0x39, 0xac, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x21");
    }

    [Test]
    public async Task Enum()
    {
        // Act
        var bytes = MessagePackSerializer.Serialize(MyEnumValue.C, MessagePackSerializerOptions.DefaultAot);

        // Assert
        await Assert.That(string.Join(", ", bytes.Select(x => $"0x{x:x2}"))).IsEqualTo("0x02");
    }

    [Test]
    public async Task BuiltInGenerics_List()
    {
        // Act
        var bytes = MessagePackSerializer.Serialize<List<MyObject>>([new MyObject(1), new MyObject(100), new MyObject(1000)], MessagePackSerializerOptions.DefaultAot);

        // Assert
        await Assert.That(string.Join(", ", bytes.Select(x => $"0x{x:x2}"))).IsEqualTo("0x93, 0x91, 0x01, 0x91, 0x64, 0x91, 0xcd, 0x03, 0xe8");
    }

    [Test]
    public async Task BuiltInGenerics_Dictionary()
    {
        // Act
        var bytes = MessagePackSerializer.Serialize(new Dictionary<MyObject, string>()
        {
            [new MyObject(12345)] = "Hello",
            [new MyObject(67890)] = "World",
        }, MessagePackSerializerOptions.DefaultAot);

        // Assert
        await Assert.That(string.Join(", ", bytes.Select(x => $"0x{x:x2}"))).IsEqualTo("0x82, 0x91, 0xcd, 0x30, 0x39, 0xa5, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x91, 0xce, 0x00, 0x01, 0x09, 0x32, 0xa5, 0x57, 0x6f, 0x72, 0x6c, 0x64");
    }
}
