using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Server;
using MagicOnionTestServer;
using Microsoft.AspNetCore.Mvc.Testing;

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously

namespace MagicOnion.Integration.Tests.MemoryPack;

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

    public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
    {
        yield return new[] { new TestMagicOnionClientFactory<IMemoryPackSerializerTestService>("Dynamic", (x, messageSerializer) => MagicOnionClient.Create<IMemoryPackSerializerTestService>(x, messageSerializer)) };
        yield return new[] { new TestMagicOnionClientFactory<IMemoryPackSerializerTestService>("Generated", (x, messageSerializer) => new MemoryPackSerializerTestServiceClient(x, messageSerializer ?? MessagePackMagicOnionSerializerProvider.Default)) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Incompatible(TestMagicOnionClientFactory<IMemoryPackSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, MessagePackMagicOnionSerializerProvider.Default); // Use MagicOnionMessagePackMessageSerializer by client. but the server still use XorMagicOnionMessagePackSerializer.

        // Act
        var result = Record.ExceptionAsync(async () => await client.UnaryReturnNil());

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_ReturnNil(TestMagicOnionClientFactory<IMemoryPackSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.UnaryReturnNil();

        // Assert
        result.Should().Be(Nil.Default);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Parameterless(TestMagicOnionClientFactory<IMemoryPackSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.UnaryParameterless();

        // Assert
        result.Should().Be(123);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Parameter_Many(TestMagicOnionClientFactory<IMemoryPackSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, MemoryPackMagicOnionSerializerProvider.Instance);

        // Act
        var result = await client.Unary1(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, "15");

        // Assert
        result.Should().Be(120);
    }
}

public interface IMemoryPackSerializerTestService : IService<IMemoryPackSerializerTestService>
{
    UnaryResult<Nil> UnaryReturnNil();
    UnaryResult<int> UnaryParameterless();

    // T0 - T14 (TypeParams = 15)
    UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, string arg14);
}

public class MemoryPackSerializerTestService : ServiceBase<IMemoryPackSerializerTestService>, IMemoryPackSerializerTestService
{
    public UnaryResult<Nil> UnaryReturnNil()
        => UnaryResult(Nil.Default);
    public UnaryResult<int> UnaryParameterless()
        => UnaryResult(123);

    public UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, string arg14)
        => UnaryResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + int.Parse(arg14));
}
