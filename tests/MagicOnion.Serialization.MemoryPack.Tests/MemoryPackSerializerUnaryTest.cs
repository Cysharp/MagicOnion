using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server;
using MemoryPack;
using MessagePack;
using Microsoft.AspNetCore.Mvc.Testing;

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously

namespace MagicOnion.Serialization.MemoryPack.Tests;

public class MemoryPackSerializerUnaryTest : IClassFixture<MagicOnionApplicationFactory<MemoryPackSerializerTestService>>
{
    readonly WebApplicationFactory<Program> factory;

    public MemoryPackSerializerUnaryTest(MagicOnionApplicationFactory<MemoryPackSerializerTestService> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = MemoryPackMagicOnionSerializerProvider.Instance;
        });
    }

    [Fact]
    public async Task Unary_Incompatible()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMemoryPackSerializerTestService>(channel, MessagePackMagicOnionSerializerProvider.Default); // Use MagicOnionMessagePackMessageSerializer by client. but the server still use XorMagicOnionMessagePackSerializer.

        // Act
        var result = await Record.ExceptionAsync(async () => await client.UnaryReturnNil());

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Unary_ReturnNil()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMemoryPackSerializerTestService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.UnaryReturnNil();

        // Assert
        Assert.Equal(Nil.Default, result);
    }

    [Fact]
    public async Task Unary_Return_CustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMemoryPackSerializerTestService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.UnaryReturnCustomObject();

        // Assert
        Assert.Equal("Alice", result.Name);
        Assert.Equal(18, result.Age);
    }

    [Fact]
    public async Task Unary_Parameterless()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMemoryPackSerializerTestService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.UnaryParameterless();

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public async Task Unary_Parameter_Many()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMemoryPackSerializerTestService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.Unary1(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, "15");

        // Assert
        Assert.Equal(120, result);
    }

    [Fact]
    public async Task Unary_Parameter_CustomObject()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMemoryPackSerializerTestService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.UnaryCustomObject(new MyObject() { Name = "Alice", Age = 18 });

        // Assert
        Assert.Equal("Alice", result.Name);
        Assert.Equal(18, result.Age);
    }
}

public interface IMemoryPackSerializerTestService : IService<IMemoryPackSerializerTestService>
{
    UnaryResult<Nil> UnaryReturnNil();
    UnaryResult<int> UnaryParameterless();
    UnaryResult<MyObject> UnaryReturnCustomObject();

    UnaryResult<(string Name, int Age)> UnaryCustomObject(MyObject obj);

    // T0 - T14 (TypeParams = 15)
    UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, string arg14);
}

public class MemoryPackSerializerTestService : ServiceBase<IMemoryPackSerializerTestService>, IMemoryPackSerializerTestService
{
    public UnaryResult<Nil> UnaryReturnNil()
        => UnaryResult.FromResult(Nil.Default);
    public UnaryResult<int> UnaryParameterless()
        => UnaryResult.FromResult(123);
    public UnaryResult<MyObject> UnaryReturnCustomObject()
        => UnaryResult.FromResult(new MyObject { Name = "Alice", Age = 18 });

    public UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, string arg14)
        => UnaryResult.FromResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + int.Parse(arg14));
    public UnaryResult<(string Name, int Age)> UnaryCustomObject(MyObject obj)
        => UnaryResult.FromResult((obj.Name, obj.Age));
}

[MemoryPackable]
public partial class MyObject
{
    public required string Name { get; init; }
    public int Age { get; init; }
}
