using System.Buffers;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnionTestServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Integration.Tests;

public class MessageSerializerTest : IClassFixture<MagicOnionApplicationFactory<MessageSerializerTestService>>
{
    readonly WebApplicationFactory<Program> factory;

    public MessageSerializerTest(MagicOnionApplicationFactory<MessageSerializerTestService> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = XorMagicOnionMessagePackSerializer.Default;
        });
    }

    [Fact]
    public async Task Unary_Incompatible()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMessageSerializerTestService>(channel, MagicOnionMessagePackMessageSerializer.Default); // Use the default serialize by client. but the server still use XorMagicOnionMessagePackSerializer.

        // Act
        var result  = Record.ExceptionAsync(async () => await client.UnaryReturnNil());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Unary_ReturnNil()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMessageSerializerTestService>(channel, XorMagicOnionMessagePackSerializer.Default);

        // Act
        var result  = await client.UnaryReturnNil();

        // Assert
        result.Should().Be(Nil.Default);
    }

    [Fact]
    public async Task Unary_Parameterless()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMessageSerializerTestService>(channel, XorMagicOnionMessagePackSerializer.Default);

        // Act
        var result  = await client.UnaryParameterless();

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public async Task Unary_Parameter_Many()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IMessageSerializerTestService>(channel, XorMagicOnionMessagePackSerializer.Default);

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

public class XorMagicOnionMessagePackSerializer : MagicOnionMessagePackMessageSerializer
{
    const int MagicNumber = 0x11;

    public static MagicOnionMessagePackMessageSerializer Default { get; } = new XorMagicOnionMessagePackSerializer(MessagePackSerializer.DefaultOptions, enableFallback: false);

    private XorMagicOnionMessagePackSerializer(MessagePackSerializerOptions serializerOptions, bool enableFallback)
        : base(serializerOptions, enableFallback)
    { }

    protected override MagicOnionMessagePackMessageSerializer Create(MessagePackSerializerOptions serializerOptions, bool enableFallback)
        => new XorMagicOnionMessagePackSerializer(serializerOptions, enableFallback);

    protected override T Deserialize<T>(in ReadOnlySequence<byte> bytes, MessagePackSerializerOptions serializerOptions)
    {
        var array = ArrayPool<byte>.Shared.Rent((int)bytes.Length);
        try
        {
            bytes.CopyTo(array);
            for (var i = 0; i < bytes.Length; i++)
            {
                array[i] ^= MagicNumber;
            }
            return MessagePackSerializer.Deserialize<T>(array.AsMemory(0, (int)bytes.Length), serializerOptions);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    protected override void Serialize<T>(IBufferWriter<byte> writer, in T value, MessagePackSerializerOptions serializerOptions)
    {
        var serialized = MessagePackSerializer.Serialize(value, serializerOptions);
        for (var i = 0; i < serialized.Length; i++)
        {
            serialized[i] ^= MagicNumber;
        }
        writer.Write(serialized);
    }
}
