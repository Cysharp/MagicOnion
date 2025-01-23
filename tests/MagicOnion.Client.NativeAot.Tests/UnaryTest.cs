using MagicOnion.Serialization.MessagePack;
using MessagePack.Resolvers;

namespace MagicOnion.Client.NativeAot.Tests;

[TestClass]
public sealed class UnaryTest
{
    [TestMethod]
    public void Create()
    {
        // Arrange
        var resolvers = CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance);
        var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
        var callInvokerMock = new MockCallInvoker();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public async Task Invoke_With_Arguments()
    {
        // Arrange
        var resolvers = CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance);
        var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
        var callInvokerMock = new MockCallInvoker();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

        // Act
        var resultTask = client.TwoParametersReturnValueType(12345, "Hello");
        callInvokerMock.ResponseChannel.Writer.TryWrite(MessagePackSerializer.Serialize(67890));
        var result = await resultTask.ResponseAsync.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.AreEqual(67890, result);
        Assert.AreEqual(1, callInvokerMock.RequestPayloads.Count);
        Assert.AreEqual("0x92, 0xcd, 0x30, 0x39, 0xa5, 0x48, 0x65, 0x6c, 0x6c, 0x6f", string.Join(", ", callInvokerMock.RequestPayloads[0].Select(x => $"0x{x:x2}")));
    }

    [TestMethod]
    public async Task Invoke_Enum()
    {
        // Arrange
        var resolvers = CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance);
        var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
        var callInvokerMock = new MockCallInvoker();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

        // Act
        var resultTask = client.Enum(MyEnumValue.C);
        callInvokerMock.ResponseChannel.Writer.TryWrite(MessagePackSerializer.Serialize(Nil.Default));
        await resultTask.ResponseAsync.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.AreEqual(1, callInvokerMock.RequestPayloads.Count);
        Assert.AreEqual("0x02", string.Join(", ", callInvokerMock.RequestPayloads[0].Select(x => $"0x{x:x2}")));
    }

    [TestMethod]
    public async Task Invoke_Enum_Return()
    {
        // Arrange
        var resolvers = CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance);
        var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
        var callInvokerMock = new MockCallInvoker();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

        // Act
        var resultTask = client.EnumReturn();
        callInvokerMock.ResponseChannel.Writer.TryWrite([0x02]);
        var result = await resultTask.ResponseAsync.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.AreEqual(1, callInvokerMock.RequestPayloads.Count);
        Assert.AreEqual(MyEnumValue.C, result);
    }


    [TestMethod]
    public async Task Invoke_BuiltInGeneric()
    {
        // Arrange
        var resolvers = CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance);
        var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
        var callInvokerMock = new MockCallInvoker();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

        // Act
        var resultTask = client.BuiltInGeneric([new(1), new(2), new(3), new(4), new(5)]);
        callInvokerMock.ResponseChannel.Writer.TryWrite(MessagePackSerializer.Serialize(Nil.Default));
        await resultTask.ResponseAsync.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.AreEqual(1, callInvokerMock.RequestPayloads.Count);
        Assert.AreEqual("0x95, 0x91, 0x01, 0x91, 0x02, 0x91, 0x03, 0x91, 0x04, 0x91, 0x05", string.Join(", ", callInvokerMock.RequestPayloads[0].Select(x => $"0x{x:x2}")));
    }

    [TestMethod]
    public async Task Invoke_BuiltInGeneric_Return()
    {
        // Arrange
        var resolvers = CompositeResolver.Create(MagicOnionClientGeneratedInitializer.Resolver, StandardResolver.Instance);
        var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
        var callInvokerMock = new MockCallInvoker();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

        // Act
        var resultTask = client.BuiltInGenericReturn();
        callInvokerMock.ResponseChannel.Writer.TryWrite([0x82, 0x91, 0x0c, 0xa6, 0x46, 0x6f, 0x6f, 0x42, 0x61, 0x72, 0x91, 0x22, 0xa3, 0x42, 0x61, 0x7a]); // [{"12": "FooBar"}, {"34": "Baz"}]
        var result = await resultTask.ResponseAsync.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.AreEqual(1, callInvokerMock.RequestPayloads.Count);
        Assert.IsTrue(result.ContainsKey(new(12)));
        Assert.AreEqual("FooBar", result[new(12)]);
        Assert.IsTrue(result.ContainsKey(new(34)));
        Assert.AreEqual("Baz", result[new(34)]);
    }

    //[TestMethod]
    //public async Task Invoke_With_Arguments_ResolverUnregistered()
    //{
    //    // Arrange
    //    var resolvers = StandardResolver.Instance; // Unuse MagicOnionClientGeneratedInitializer.Resolver (serialization may fail)
    //    var serializerOptions = MessagePackMagicOnionSerializerProvider.Default.WithOptions(MessagePackSerializerOptions.Standard.WithResolver(resolvers));
    //    var callInvokerMock = new MockCallInvoker();
    //    var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock, serializerOptions);

    //    // Act
    //    Exception? ex = default;
    //    try
    //    {
    //        var resultTask = client.TwoParametersReturnValueType(12345, "Hello");
    //        await resultTask.ResponseAsync.WaitAsync(TimeSpan.FromSeconds(5));
    //    }
    //    catch (Exception e)
    //    {
    //        ex = e;
    //    }

    //    // Assert
    //    Assert.IsNotNull(ex);
    //    Assert.IsInstanceOfType<MessagePackSerializationException>(ex);
    //}
}

[MagicOnionClientGeneration(typeof(IUnaryTestService))]
partial class MagicOnionClientGeneratedInitializer;
