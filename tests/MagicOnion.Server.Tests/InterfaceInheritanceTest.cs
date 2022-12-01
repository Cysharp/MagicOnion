using Grpc.Net.Client;
using MagicOnion.Client;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests;

public interface IInterfaceInheritanceServiceBaseBase
{
    UnaryResult<int> UnaryBaseBase(int x, int y);
}

public interface IInterfaceInheritanceServiceBase: IInterfaceInheritanceServiceBaseBase
{
    UnaryResult<int> UnaryBase(int x, int y);
}

public interface IInterfaceInheritanceService : IService<IInterfaceInheritanceService>, IInterfaceInheritanceServiceBase
{
    UnaryResult<int> Unary1(int x, int y);
}

public class InterfaceInheritanceService : ServiceBase<IInterfaceInheritanceService>, IInterfaceInheritanceService
{
    public UnaryResult<int> UnaryBaseBase(int x, int y) => UnaryResult(x - y);

    public UnaryResult<int> UnaryBase(int x, int y) => UnaryResult(x * y);

    public UnaryResult<int> Unary1(int x, int y) => UnaryResult(x + y);
}

public class InterfaceInheritanceTest : IClassFixture<ServerFixture<InterfaceInheritanceService>>
{
    readonly ITestOutputHelper logger;
    readonly GrpcChannel channel;

    public InterfaceInheritanceTest(ITestOutputHelper logger, ServerFixture<InterfaceInheritanceService> server)
    {
        this.logger = logger;
        this.channel = server.DefaultChannel;
    }

    [Fact]
    public async Task CallBaseMethods()
    {
        var client = MagicOnionClient.Create<IInterfaceInheritanceService>(channel);

        var r = await client.Unary1(3, 4);
        r.Should().Be(3 + 4);

        var m = await client.UnaryBase(3, 4);
        m.Should().Be(3 * 4);

        var s = await client.UnaryBaseBase(3, 4);
        s.Should().Be(3 - 4);
    }
}
