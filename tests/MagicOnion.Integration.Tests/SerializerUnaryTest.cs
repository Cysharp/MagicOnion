using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server;
using MagicOnionTestServer;
using Microsoft.AspNetCore.Mvc.Testing;

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously

namespace MagicOnion.Integration.Tests;

public class SerializerUnaryTest : IClassFixture<MagicOnionApplicationFactory<SerializerTestService>>
{
    readonly WebApplicationFactory<Program> factory;

    public SerializerUnaryTest(MagicOnionApplicationFactory<SerializerTestService> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = XorMessagePackMagicOnionSerializerProvider.Instance;
        });
    }

    public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
    {
        yield return new [] { new TestMagicOnionClientFactory("Dynamic", DynamicMagicOnionClientFactoryProvider.Instance) };
        yield return new [] { new TestMagicOnionClientFactory("Generated", MagicOnionGeneratedClientInitializer.ClientFactoryProvider) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Incompatible(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<ISerializerTestService>(channel, MessagePackMagicOnionSerializerProvider.Default); // Use MagicOnionMessagePackMessageSerializer by client. but the server still use XorMagicOnionMessagePackSerializer.

        // Act
        var result  = Record.ExceptionAsync(async () => await client.UnaryReturnNil());

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_ReturnNil(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<ISerializerTestService>(channel, XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.UnaryReturnNil();

        // Assert
        result.Should().Be(Nil.Default);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Parameterless(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<ISerializerTestService>(channel, XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.UnaryParameterless();

        // Assert
        result.Should().Be(123);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Parameter_Many(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<ISerializerTestService>(channel, XorMessagePackMagicOnionSerializerProvider.Instance);

        // Act
        var result  = await client.Unary1(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        // Assert
        result.Should().Be(120);
    }
}

public interface ISerializerTestService : IService<ISerializerTestService>
{
    UnaryResult<Nil> UnaryReturnNil();
    UnaryResult<int> UnaryParameterless();

    // T0 - T14 (TypeParams = 15)
    UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);
}

public class SerializerTestService : ServiceBase<ISerializerTestService>, ISerializerTestService
{
    public UnaryResult<Nil> UnaryReturnNil()
        => UnaryResult.FromResult(Nil.Default);
    public UnaryResult<int> UnaryParameterless()
        => UnaryResult.FromResult(123);

    public UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14)
        => UnaryResult.FromResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + arg14);
}
