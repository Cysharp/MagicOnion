using System;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using Xunit;



namespace MagicOnion.Tests
{
    public interface IPartialDefinition : IService<IPartialDefinition>, IPartialDefinition2
    {
        UnaryResult<int> Unary1(int x, int y);
    }


    public interface IPartialDefinition2
    {
        UnaryResult<int> Unary2();
    }


    public class CombinedDefinition : ServiceBase<IPartialDefinition>, IPartialDefinition2
    {
        public UnaryResult<int> Unary1(int x, int y)
            => this.UnaryResult(x + y);

        public UnaryResult<int> Unary2()
            => this.UnaryResult(100);
    }


    public class PartialDefinitionTest : IClassFixture<ServerFixture>, IDisposable
    {
        Channel channel;

        public PartialDefinitionTest(ServerFixture server)
        {
            this.channel = new Channel(server.ServerPort.Host, server.ServerPort.Port, ChannelCredentials.Insecure);
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        [Fact]
        public async Task Unary1()
        {
            var client = MagicOnionClient.Create<IPartialDefinition>(channel);
            var r = await client.Unary1(10, 20);
            r.Is(30);
        }

        [Fact]
        public async Task Unary2()
        {
            var client = MagicOnionClient.Create<IPartialDefinition>(channel);
            var r = await client.Unary2();
            r.Is(100);
        }
    }
}