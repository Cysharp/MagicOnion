using MagicOnion.Client;
using Grpc.Net.Client;
using MagicOnion.Server;
using Xunit.Abstractions;
using MagicOnion.Serialization;
using MagicOnion.Integration.Tests.Generated;

namespace MagicOnion.Integration.Tests;

public class DynamicArgumentTupleServiceTest : IClassFixture<MagicOnionApplicationFactory<DynamicArgumentTupleService>>
{
    readonly MagicOnionApplicationFactory<DynamicArgumentTupleService> factory;

    public DynamicArgumentTupleServiceTest(MagicOnionApplicationFactory<DynamicArgumentTupleService> factory)
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
    public async Task Unary1(TestMagicOnionClientFactory clientFactory)
    {
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IDynamicArgumentTupleService>(channel);
        var result  = await client.Unary1(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        result.Should().Be(120);
    }
}

public interface IDynamicArgumentTupleService : IService<IDynamicArgumentTupleService>
{
    // T0 - T14 (TypeParams = 15)
    UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);
}

public class DynamicArgumentTupleService : ServiceBase<IDynamicArgumentTupleService>, IDynamicArgumentTupleService
{
    public UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14)
    {
        return UnaryResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + arg14);
    }
}
