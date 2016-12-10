using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MagicOnion.Tests
{
    public interface IUnaryTest : IService<IUnaryTest>
    {
        UnaryResult<int> TestSum(int x, int y);
        Task<UnaryResult<int>> TestSumTask(int x, int y);
    }

    public class UnaryTestImpl : ServiceBase<IUnaryTest>, IUnaryTest
    {
        public UnaryResult<int> TestSum(int x, int y)
        {
            return UnaryResult(x + y);
        }

        public async Task<UnaryResult<int>> TestSumTask(int x, int y)
        {
            await Task.Yield();
            return UnaryResult(x + y);
        }
    }

    public class UnaryTest : IClassFixture<ServerFixture>, IDisposable
    {
        Channel channel;

        public UnaryTest(ServerFixture server)
        {
            channel = new Channel(server.ServerPort.Host, server.ServerPort.Port, ChannelCredentials.Insecure);
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        [Fact]
        public async Task StandardTestSum()
        {
            var client = MagicOnionClient.Create<IUnaryTest>(channel);

            var r = await client.TestSum(10, 20);
            r.Is(30);

            var r2 = await await client.TestSumTask(1000, 2000);
            r2.Is(3000);
        }
    }
}
