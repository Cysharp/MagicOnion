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
        UnaryResult<int> Unary1(int x, int y);
        Task<UnaryResult<int>> Unary1Task(int x, int y);

        ClientStreamingResult<int, string> ClientStreaming1();
        Task<ClientStreamingResult<int, string>> ClientStreaming1Task();
    }

    public class UnaryTestImpl : ServiceBase<IUnaryTest>, IUnaryTest
    {
        public ClientStreamingResult<int, string> ClientStreaming1()
        {
            var streaming = GetClientStreamingContext<int, string>();

            var list = new List<int>();
            // no listen from client...
            return streaming.Result("finished:" + string.Join(", ", list));
        }

        public async Task<ClientStreamingResult<int, string>> ClientStreaming1Task()
        {
            var streaming = GetClientStreamingContext<int, string>();

            var list = new List<int>();
            await streaming.ForEachAsync(x =>
            {
                list.Add(x);
            });

            return streaming.Result("finished:" + string.Join(", ", list));
        }

        public UnaryResult<int> Unary1(int x, int y)
        {
            return UnaryResult(x + y);
        }

        public async Task<UnaryResult<int>> Unary1Task(int x, int y)
        {
            await Task.Yield();
            return UnaryResult(x + y);
        }
    }

    public class SimpleTest : IClassFixture<ServerFixture>, IDisposable
    {
        Channel channel;

        public SimpleTest(ServerFixture server)
        {
            channel = new Channel(server.ServerPort.Host, server.ServerPort.Port, ChannelCredentials.Insecure);
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        [Fact]
        public async Task Unary()
        {
            var client = MagicOnionClient.Create<IUnaryTest>(channel);

            var r = await client.Unary1(10, 20);
            r.Is(30);

            var r2 = await await client.Unary1Task(1000, 2000);
            r2.Is(3000);
        }

        [Fact]
        public async Task ClientStreaming()
        {
            var client = MagicOnionClient.Create<IUnaryTest>(channel);
            {
                var r = await client.ClientStreaming1Task();
                await r.RequestStream.WriteAsync(10);
                await r.RequestStream.WriteAsync(20);
                await r.RequestStream.WriteAsync(30);
                await r.RequestStream.CompleteAsync();

                var result = await r.ResponseAsync;
                result.Is("finished:10, 20, 30");
            }
            {
                var r = client.ClientStreaming1();
                await r.RequestStream.WriteAsync(10);
                await r.RequestStream.WriteAsync(20);
                await r.RequestStream.WriteAsync(30);
                await r.RequestStream.CompleteAsync();

                var result = await r.ResponseAsync;
                result.Is("finished:");
            }
        }
    }
}
