using System.Buffers;
using MessagePack.Resolvers;

namespace MagicOnion.Client.NativeAot.Tests;

// NOTE: This test uses a Resolver that contains code corresponding to the arguments and return values of IUnaryTestService.
public sealed class ResolverTest
{
    static ResolverTest()
    {
        // WORKAROUND: GeneratedAssemblyMessagePackResolverAttribute in MessagePack v3.1.1 does not consider trimming, causing type metadata to be removed.
        _ = typeof(MessagePack.GeneratedMessagePackResolver).GetFields();
    }

    [Test]
    public async Task MyObject()
    {
        // Arrange
        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));
        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);

        // Act
        var formatter = SourceGeneratedFormatterResolver.Instance.GetFormatter<MyObject>();
        formatter?.Serialize(ref writer, new MyObject(12345), options);
        writer.Flush();

        // Assert
        await Assert.That(formatter).IsNotNull();
        await Assert.That(string.Join(", ", arrayBufferWriter.WrittenMemory.ToArray().Select(x => $"0x{x:x2}"))).IsEqualTo("0x91, 0xcd, 0x30, 0x39");
    }

    [Test]
    public async Task DynamicArgumentTuple()
    {
        // Arrange
        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));
        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);

        // Act
        var formatter = options.Resolver.GetFormatter<DynamicArgumentTuple<int, string>>();
        formatter?.Serialize(ref writer, new DynamicArgumentTuple<int, string>(12345, "Hello world!"), options);
        writer.Flush();

        // Assert
        await Assert.That(formatter).IsNotNull();
        await Assert.That(string.Join(", ", arrayBufferWriter.WrittenMemory.ToArray().Select(x => $"0x{x:x2}"))).IsEqualTo("0x92, 0xcd, 0x30, 0x39, 0xac, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x21");
    }

    //[Test]
    //public async Task DynamicArgumentTuple_Unknown()
    //{
    //    // Arrange
    //    var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));

    //    // Act
    //    var formatter = options.Resolver.GetFormatter<DynamicArgumentTuple<int, string, object, bool, Uri, Guid, MyEnumValue>>();

    //    // Assert
    //    await Assert.That(formatter).IsNull();
    //}

    [Test]
    public async Task Enum()
    {
        // Arrange
        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));
        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);

        // Act
        var formatter = options.Resolver.GetFormatter<MyEnumValue>();
        formatter?.Serialize(ref writer, MyEnumValue.C, options);
        writer.Flush();

        // Assert
        await Assert.That(formatter).IsNotNull();
        await Assert.That(string.Join(", ", arrayBufferWriter.WrittenMemory.ToArray().Select(x => $"0x{x:x2}"))).IsEqualTo("0x02");
    }

    //[Test]
    //public async Task Enum_Unknown()
    //{
    //    // Arrange
    //    var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));

    //    // Act
    //    var formatter = options.Resolver.GetFormatter<UnknownEnumValue>();

    //    // Assert
    //    await Assert.That(formatter).IsNull();
    //}

    [Test]
    public async Task BuiltInGenerics_List()
    {
        // Arrange
        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));
        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);

        // Act
        var formatter = options.Resolver.GetFormatter<List<MyObject>>();
        formatter?.Serialize(ref writer, [new MyObject(1), new MyObject(100), new MyObject(1000)], options);
        writer.Flush();

        // Assert
        await Assert.That(formatter).IsNotNull();
        await Assert.That(string.Join(", ", arrayBufferWriter.WrittenMemory.ToArray().Select(x => $"0x{x:x2}"))).IsEqualTo("0x93, 0x91, 0x01, 0x91, 0x64, 0x91, 0xcd, 0x03, 0xe8");
    }

    [Test]
    public async Task BuiltInGenerics_Dictionary()
    {
        // Arrange
        var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));
        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(arrayBufferWriter);

        // Act
        var formatter = options.Resolver.GetFormatter<Dictionary<MyObject, string>>();
        formatter?.Serialize(ref writer, new Dictionary<MyObject, string>()
        {
            [new MyObject(12345)] = "Hello",
            [new MyObject(67890)] = "World",
        }, options);
        writer.Flush();

        // Assert
        await Assert.That(formatter).IsNotNull();
        await Assert.That(string.Join(", ", arrayBufferWriter.WrittenMemory.ToArray().Select(x => $"0x{x:x2}"))).IsEqualTo("0x82, 0x91, 0xcd, 0x30, 0x39, 0xa5, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x91, 0xce, 0x00, 0x01, 0x09, 0x32, 0xa5, 0x57, 0x6f, 0x72, 0x6c, 0x64");
    }

    //[Test]
    //public async Task BuiltInGenerics_Unknown()
    //{
    //    // Arrange
    //    var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance));

    //    // Act
    //    var formatter1 = options.Resolver.GetFormatter<List<UnknownObject>>();
    //    var formatter2 = options.Resolver.GetFormatter<Dictionary<UnknownObject, string>>();

    //    // Assert
    //    await Assert.That(formatter1).IsNull();
    //    await Assert.That(formatter2).IsNull();
    //}

    class UnknownObject;

    enum UnknownEnumValue
    {
        A,
        B,
        C,
    }
}
