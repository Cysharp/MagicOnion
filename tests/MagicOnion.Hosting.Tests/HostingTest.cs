using System;
using System.Threading.Tasks;
using Xunit;
using Grpc.Core;
using Microsoft.Extensions.Hosting;

namespace MagicOnion.Hosting.Tests
{

    public class HostingTest
    {
        [Fact]
        public async Task TestHosting()
        {
            var randomPort = new Random().Next(10000, 20000);
            var ports = new[] { new ServerPort("localhost", randomPort, ServerCredentials.Insecure) };
            using (var host = new HostBuilder()
                .UseMagicOnion(ports, new MagicOnion.Server.MagicOnionOptions(), types: new []{ typeof(TestServiceImpl) }).Build())
            {
                host.Start();
                var channel = new Channel("localhost", randomPort, ChannelCredentials.Insecure);
                var client = MagicOnion.Client.MagicOnionClient.Create<ITestService>(channel);
                for (int i = 0; i < 10; i++)
                {
                    var ret = await client.Sum(i, i);
                    Assert.Equal(i * 2, ret);
                }
                await host.StopAsync();
            }
        }
    }
}
