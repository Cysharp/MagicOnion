using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests
{
    public interface ISimpleTest : IService<ISimpleTest>
    {
        UnaryResult<int> Unary1(int x, int y);
        //Task<UnaryResult<int>> Unary1Task(int x, int y);
        UnaryResult<int> Unary2(int x, int y);

        //ClientStreamingResult<int, string> ClientStreaming1();
        Task<ClientStreamingResult<int, string>> ClientStreaming1Task();

        //ServerStreamingResult<string> Serverstreaming1(int x, int y, int z);
        Task<ServerStreamingResult<string>> ServerStreaming1Task(int x, int y, int z);

        //DuplexStreamingResult<int, string> DuplexStreaming1();
        Task<DuplexStreamingResult<int, string>> DuplexStreaming1Task();
    }

    public class UnaryTestImpl : ServiceBase<ISimpleTest>, ISimpleTest
    {
        //public ClientStreamingResult<int, string> ClientStreaming1()
        //{
        //    var streaming = GetClientStreamingContext<int, string>();

        //    var list = new List<int>();
        //    // no listen from client...
        //    return streaming.Result("finished:" + string.Join(", ", list));
        //}

        public async Task<ClientStreamingResult<int, string>> ClientStreaming1Task()
        {
            var streaming = GetClientStreamingContext<int, string>();

            var list = new List<int>();
            await foreach (var x in streaming.ReadAllAsync())
            {
                list.Add(x);
            }

            return streaming.Result("finished:" + string.Join(", ", list));
        }

        //public DuplexStreamingResult<int, string> DuplexStreaming1()
        //{
        //    var stream = GetDuplexStreamingContext<int, string>();
        //    return stream.Result();
        //}

        public async Task<DuplexStreamingResult<int, string>> DuplexStreaming1Task()
        {
            var stream = GetDuplexStreamingContext<int, string>();

            var l = new List<int>();

            await foreach (var x in stream.ReadAllAsync())
            {
                l.Add(x);
                await stream.WriteAsync(string.Join(", ", l));
            }

            return stream.Result();
        }

        //public ServerStreamingResult<string> Serverstreaming1(int x, int y, int z)
        //{
        //    var stream = GetServerStreamingContext<string>();

        //    // no write?
        //    return stream.Result();
        //}

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

    public class SimpleTest : IClassFixture<ServerFixture<UnaryTestImpl>>
    {
        ITestOutputHelper logger;
        GrpcChannel channel;

        public SimpleTest(ITestOutputHelper logger, ServerFixture<UnaryTestImpl> server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task Unary()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);

            var r = await client.Unary1(10, 20);
            r.Should().Be(30);

            //var r0 = await client.Unary1Task(1000, 2000);
            //var r2 = await r0;
            //r2.Should().Be(3000);
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
                result.Should().Be("finished:10, 20, 30");
            }
            //{
            //    var r = client.ClientStreaming1();
            //    var result = await r.ResponseAsync;
            //    result.Should().Be("finished:");
            //}
        }

        [Fact]
        public async Task ServerStreaming()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);
            {
                var r = await client.ServerStreaming1Task(10, 20, 3);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Should().Be("30");
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Should().Be("60");
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Should().Be("90");
            }
            //{
            //    var r = client.Serverstreaming1(100, 200, 3);
            //    (await r.ResponseStream.MoveNext()).Should().BeFalse();
            //}
        }

        [Fact]
        public async Task DuplexStreaming()
        {
            var client = MagicOnionClient.Create<ISimpleTest>(channel);
            {
                var r = await client.DuplexStreaming1Task();

                await r.RequestStream.WriteAsync(1000);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Should().Be("1000");

                await r.RequestStream.WriteAsync(2000);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Should().Be("1000, 2000");

                await r.RequestStream.WriteAsync(3000);
                await r.ResponseStream.MoveNext();
                r.ResponseStream.Current.Should().Be("1000, 2000, 3000");

                await r.RequestStream.CompleteAsync();
                (await r.ResponseStream.MoveNext()).Should().BeFalse();
            }
            //{
            //    var r = client.DuplexStreaming1();
            //    (await r.ResponseStream.MoveNext()).Should().BeFalse();
            //}
        }
    }
}
