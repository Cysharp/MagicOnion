using Grpc.Net.Client;
using MagicOnion.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Integration.Tests.Generated;

namespace MagicOnion.Integration.Tests;

public class UnaryServiceTest : IClassFixture<MagicOnionApplicationFactory<UnaryService>>
{
    readonly MagicOnionApplicationFactory<UnaryService> factory;

    public UnaryServiceTest(MagicOnionApplicationFactory<UnaryService> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
    {
        yield return new [] { new TestMagicOnionClientFactory("Dynamic", DynamicMagicOnionClientFactoryProvider.Instance) };
        yield return new [] { new TestMagicOnionClientFactory("Generated", MagicOnionGeneratedClientFactoryProvider.Instance) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task NonGeneric(TestMagicOnionClientFactory clientFactory)
    {
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IUnaryService>(channel);
        var result = client.NonGeneric(123);
        await result;

        result.GetTrailers().Should().Contain(x => x.Key == "x-request-arg0" && x.Value == "123");
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ManyParametersReturnsValueType(TestMagicOnionClientFactory clientFactory)
    {
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IUnaryService>(channel);
        var result  = await client.ManyParametersReturnsValueType(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        result.Should().Be(120);
    }
}

public interface IUnaryService : IService<IUnaryService>
{
    UnaryResult NonGeneric(int arg0);

    // T0 - T14 (TypeParams = 15)
    UnaryResult<int> ManyParametersReturnsValueType(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);
}

public class UnaryService : ServiceBase<IUnaryService>, IUnaryService
{
    public UnaryResult NonGeneric(int arg0)
    {
        Context.CallContext.ResponseTrailers.Add("x-request-arg0", "" + arg0);
        return default;
    }

    public UnaryResult<int> ManyParametersReturnsValueType(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14)
    {
        return UnaryResult.FromResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + arg14);
    }
}
