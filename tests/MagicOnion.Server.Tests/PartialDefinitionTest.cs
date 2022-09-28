using System;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

namespace MagicOnion.Server.Tests
{
    // note, do not allow partial definition.

    //public interface IPartialDefinition : IService<IPartialDefinition>, IPartialDefinition2
    //{
    //    UnaryResult<int> Unary1(int x, int y);
    //}


    //public interface IPartialDefinition2
    //{
    //    UnaryResult<int> Unary2();
    //}


    //public class CombinedDefinition : ServiceBase<IPartialDefinition>, IPartialDefinition2
    //{
    //    public UnaryResult<int> Unary1(int x, int y)
    //        => this.UnaryResult(x + y);

    //    public UnaryResult<int> Unary2()
    //        => this.UnaryResult(100);
    //}


    //[Collection(nameof(AllAssemblyGrpcServerFixture))]
    //public class PartialDefinitionTest
    //{
    //    ITestOutputHelper logger;
    //    IPartialDefinition client;

    //    public PartialDefinitionTest(ITestOutputHelper logger, ServerFixture server)
    //    {
    //        this.logger = logger;
    //        this.client = server.CreateClient<IPartialDefinition>();
    //    }

    //    [Fact]
    //    public async Task Unary1()
    //    {
    //        var r = await client.Unary1(10, 20);
    //        r.Should().Be(30);
    //    }

    //    [Fact]
    //    public async Task Unary2()
    //    {
    //        var r = await client.Unary2();
    //        r.Should().Be(100);
    //    }
    //}
}