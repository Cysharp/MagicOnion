using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Tests
{
    public interface ISimpleTest : IService<ISimpleTest>
    {
        UnaryResult<int> Unary1(int x, int y);
        Task<UnaryResult<int>> Unary1Task(int x, int y);
        UnaryResult<int> Unary2(int x, int y);

        ClientStreamingResult<int, string> ClientStreaming1();
        Task<ClientStreamingResult<int, string>> ClientStreaming1Task();

        ServerStreamingResult<string> Serverstreaming1(int x, int y, int z);
        Task<ServerStreamingResult<string>> ServerStreaming1Task(int x, int y, int z);

        DuplexStreamingResult<int, string> DuplexStreaming1();
        Task<DuplexStreamingResult<int, string>> DuplexStreaming1Task();
    }

    public class UnaryTestImpl : ServiceBase<ISimpleTest>, ISimpleTest
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

        public DuplexStreamingResult<int, string> DuplexStreaming1()
        {
            var stream = GetDuplexStreamingContext<int, string>();
            return stream.Result();
        }

        public async Task<DuplexStreamingResult<int, string>> DuplexStreaming1Task()
        {
            var stream = GetDuplexStreamingContext<int, string>();

            var l = new List<int>();

            while (await stream.MoveNext())
            {
                l.Add(stream.Current);
                await stream.WriteAsync(string.Join(", ", l));
            }

            return stream.Result();
        }

        public ServerStreamingResult<string> Serverstreaming1(int x, int y, int z)
        {
            var stream = GetServerStreamingContext<string>();

            // no write?
            return stream.Result();
        }

        public async Task<ServerStreamingResult<string>> ServerStreaming1Task(int x, int y, int z)
        {
            var stream = GetServerStreamingContext<string>();

            var acc = 0;
            for (int i = 0; i < z; i++)
            {
                acc = acc + x + y;
                await stream.WriteAsync(acc.ToString());
            }

            return stream.Result();
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

#pragma warning disable CS1998

        public async UnaryResult<int> Unary2(int x, int y)
        {
            return x + y;
        }

#pragma warning restore CS1998

    }

    [Collection(nameof(AllAssemblyGrpcServerFixture))]
    public class SimpleTest
    {
        ITestOutputHelper logger;
        Channel channel;

        public SimpleTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task Unary()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);

            var r = await client.Unary1(10, 20);
            r.Is(30);

            var r2 = await await client.Unary1Task(1000, 2000);
            r2.Is(3000);
        }

        [Fact]
        public async Task ClientStreaming()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);
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
                var result = await r.ResponseAsync;
                result.Is("finished:");
            }
        }

        [Fact]
        public async Task ServerStreaming()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);
            {
                var r = await client.ServerStreaming1Task(10, 20, 3);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Is("30");
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Is("60");
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Is("90");
            }
            {
                var r = client.Serverstreaming1(100, 200, 3);
                (await r.ResponseStream.MoveNext()).IsFalse();
            }
        }

        [Fact]
        public async Task DuplexStreaming()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);
            {
                var r = await client.DuplexStreaming1Task();

                await r.RequestStream.WriteAsync(1000);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Is("1000");

                await r.RequestStream.WriteAsync(2000);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Is("1000, 2000");

                await r.RequestStream.WriteAsync(3000);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Is("1000, 2000, 3000");

                await r.RequestStream.CompleteAsync();
                (await r.ResponseStream.MoveNext()).IsFalse();
            }
            {
                var r = client.DuplexStreaming1();
                (await r.ResponseStream.MoveNext()).IsFalse();
            }
        }
    }
}
