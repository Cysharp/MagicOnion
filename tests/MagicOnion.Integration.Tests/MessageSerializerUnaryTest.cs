using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnionTestServer;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Integration.Tests;

public class MessageSerializerUnaryTest : IClassFixture<MagicOnionApplicationFactory<MessageSerializerTestService>>
{
    readonly WebApplicationFactory<Program> factory;

    public MessageSerializerUnaryTest(MagicOnionApplicationFactory<MessageSerializerTestService> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = XorMagicOnionMessagePackSerializer.Instance;
        });
    }
    
    public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
    {
        yield return new [] { new TestMagicOnionClientFactory<IMessageSerializerTestService>("Dynamic", (x, messageSerializer) => MagicOnionClient.Create<IMessageSerializerTestService>(x, messageSerializer)) };
        yield return new [] { new TestMagicOnionClientFactory<IMessageSerializerTestService>("Generated", (x, messageSerializer) => new MessageSerializerTestServiceClient(x, messageSerializer)) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Incompatible(TestMagicOnionClientFactory<IMessageSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, MagicOnionMessagePackMessageSerializer.Instance); // Use MagicOnionMessagePackMessageSerializer by client. but the server still use XorMagicOnionMessagePackSerializer.

        // Act
        var result  = Record.ExceptionAsync(async () => await client.UnaryReturnNil());

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_ReturnNil(TestMagicOnionClientFactory<IMessageSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, XorMagicOnionMessagePackSerializer.Instance);

        // Act
        var result  = await client.UnaryReturnNil();

        // Assert
        result.Should().Be(Nil.Default);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Parameterless(TestMagicOnionClientFactory<IMessageSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, XorMagicOnionMessagePackSerializer.Instance);

        // Act
        var result  = await client.UnaryParameterless();

        // Assert
        result.Should().Be(123);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task Unary_Parameter_Many(TestMagicOnionClientFactory<IMessageSerializerTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel, XorMagicOnionMessagePackSerializer.Instance);

        // Act
        var result  = await client.Unary1(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        // Assert
        result.Should().Be(120);
    }
}

public interface IMessageSerializerTestService : IService<IMessageSerializerTestService>
{
    UnaryResult<Nil> UnaryReturnNil();
    UnaryResult<int> UnaryParameterless();

    // T0 - T14 (TypeParams = 15)
    UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);
}

public class MessageSerializerTestService : ServiceBase<IMessageSerializerTestService>, IMessageSerializerTestService
{
    public UnaryResult<Nil> UnaryReturnNil()
        => UnaryResult(Nil.Default);
    public UnaryResult<int> UnaryParameterless()
        => UnaryResult(123);

    public UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14)
        => UnaryResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + arg14);
}
