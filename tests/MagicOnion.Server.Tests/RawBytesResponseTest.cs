using System.Collections.Concurrent;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MessagePack;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Server.Tests;

public class RawBytesResponseTest : IClassFixture<MagicOnionApplicationFactory<RawBytesResponseTestService>>
{
    readonly WebApplicationFactory<MagicOnionTestServer.Program> factory;

    public RawBytesResponseTest(MagicOnionApplicationFactory<RawBytesResponseTestService> factory)
    {
        this.factory = factory.WithMagicOnionOptions(options => {});
    }

    [Fact]
    public async Task RefType()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IRawBytesResponseTestService>(channel, MessagePackMagicOnionSerializerProvider.Default);

        // Act
        var result  = await client.RefType();
        var result2  = await client.RefType();

        // Assert
        var expected = (RawBytesResponseTestService_RefTypeResponse)FixedResponseFilterAttribute.ResponseCache["/IRawBytesResponseTestService/RefType"];
        Assert.Equal(expected.Value1, result.Value1);
        Assert.Equal(expected.Value2, result.Value2);
        Assert.Equal(expected.Value3, result.Value3);
        Assert.Equal(expected.Value1, result2.Value1);
        Assert.Equal(expected.Value2, result2.Value2);
        Assert.Equal(expected.Value3, result2.Value3);
    }

    [Fact]
    public async Task ValueType()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = MagicOnionClient.Create<IRawBytesResponseTestService>(channel, MessagePackMagicOnionSerializerProvider.Default);

        // Act
        var result  = await client.ValueType();
        var result2  = await client.ValueType();

        // Assert
        var expected = (RawBytesResponseTestService_ValueTypeResponse)FixedResponseFilterAttribute.ResponseCache["/IRawBytesResponseTestService/ValueType"];
        Assert.Equal(expected.Value1, result.Value1);
        Assert.Equal(expected.Value2, result.Value2);
        Assert.Equal(expected.Value3, result.Value3);
        Assert.Equal(expected.Value1, result2.Value1);
        Assert.Equal(expected.Value2, result2.Value2);
        Assert.Equal(expected.Value3, result2.Value3);
    }
}

file class FixedResponseFilterAttribute : MagicOnionFilterAttribute
{
    public static readonly RawBytesResponseTestService_RefTypeResponse ResponseRefType;
    public static readonly RawBytesResponseTestService_ValueTypeResponse ResponseValueType;

    public static readonly ConcurrentDictionary<string, object?> ResponseCache = new();
    public static readonly ConcurrentDictionary<string, byte[]> ResponseBytesCache = new();

    static FixedResponseFilterAttribute()
    {
        ResponseRefType = new () { Value1 = Guid.NewGuid().ToString(), Value2 = Random.Shared.Next(), Value3 = Random.Shared.NextInt64() };
        ResponseValueType = new () { Value1 = Guid.NewGuid().ToString(), Value2 = Random.Shared.Next(), Value3 = Random.Shared.NextInt64() };
    }

    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        if (ResponseBytesCache.TryGetValue(context.CallContext.Method, out var cachedBytes))
        {
            context.SetRawBytesResponse(cachedBytes);
            return;
        }

        await next(context);

        ResponseCache[context.CallContext.Method] = context.Result;
        ResponseBytesCache[context.CallContext.Method] = MessagePackSerializer.Serialize(context.Result);
    }
}

public interface IRawBytesResponseTestService : IService<IRawBytesResponseTestService>
{
    UnaryResult<RawBytesResponseTestService_RefTypeResponse> RefType();
    UnaryResult<RawBytesResponseTestService_ValueTypeResponse> ValueType();

}

[FixedResponseFilter]
public class RawBytesResponseTestService : ServiceBase<IRawBytesResponseTestService>, IRawBytesResponseTestService
{
    public UnaryResult<RawBytesResponseTestService_RefTypeResponse> RefType()
        => UnaryResult.FromResult(new RawBytesResponseTestService_RefTypeResponse() { Value1 = Guid.NewGuid().ToString(), Value2 = Random.Shared.Next(), Value3 = Random.Shared.NextInt64() });

    public UnaryResult<RawBytesResponseTestService_ValueTypeResponse> ValueType()
        => UnaryResult.FromResult(new RawBytesResponseTestService_ValueTypeResponse(){ Value1 = Guid.NewGuid().ToString(), Value2 = Random.Shared.Next(), Value3 = Random.Shared.NextInt64() });
}

[MessagePackObject(true)]
public class RawBytesResponseTestService_RefTypeResponse
{
    public string Value1 { get; set; }
    public int Value2 { get; set; }
    public long Value3 { get; set; }
}

[MessagePackObject(true)]
public struct RawBytesResponseTestService_ValueTypeResponse
{
    public string Value1 { get; set; }
    public int Value2 { get; set; }
    public long Value3 { get; set; }
}
