using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using Xunit;



namespace MagicOnion.Tests
{
    public interface IParallelWriteTestDefinition : IService<IParallelWriteTestDefinition>
    {
        Task<ClientStreamingResult<int, int>> ClientStreaming();
        Task<ServerStreamingResult<int>> ServerStreaming(int x, int y, int z);
        Task<DuplexStreamingResult<int, int>> DuplexStreaming();
    }



    public class ParallelWriteTestDefinition : ServiceBase<IParallelWriteTestDefinition>, IParallelWriteTestDefinition
    {
        public async Task<ClientStreamingResult<int, int>> ClientStreaming()
        {
            var context = this.GetClientStreamingContext<int, int>();
            var list = new List<int>();
            await context.ForEachAsync(x => list.Add(x));
            return context.Result(list.Sum());
        }


        public async Task<ServerStreamingResult<int>> ServerStreaming(int x, int y, int z)
        {
            var context = this.GetServerStreamingContext<int>();
            var tasks   = Enumerable.Range(0, 10)
                        .Select(_ => x + y + z)
                        .Select(context.WriteAsync)  //--- 並列に書き込みを連打
                        .ToArray();
            await Task.WhenAll(tasks);
            return context.Result();
        }


        public async Task<DuplexStreamingResult<int, int>> DuplexStreaming()
        {
            var context = this.GetDuplexStreamingContext<int, int>();
            await context.ForEachAsync(context.WriteAsync);  //--- 来たやつをそのまま返す
            return context.Result();
        }
    }



    public class ParallelWriteTest : IClassFixture<ServerFixture>, IDisposable
    {
        Channel channel;

        public ParallelWriteTest(ServerFixture server)
        {
            this.channel = new Channel(server.ServerPort.Host, server.ServerPort.Port, ChannelCredentials.Insecure);
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        [Fact]
        public async Task ClientStreaming()
        {
            var streaming = await MagicOnionClient.Create<IParallelWriteTestDefinition>(channel).ClientStreaming();
            try
            {
                var tasks = Enumerable.Range(0, 10).Select(streaming.RequestStream.WriteAsync).ToArray();
                await Task.WhenAll(tasks);
            }
            finally
            {
                await streaming.RequestStream.CompleteAsync();
            }
            var result = await streaming.ResponseAsync;
            result.Is(45);
        }

        [Fact]
        public async Task ServerStreaming()
        {
            var streaming = await MagicOnionClient.Create<IParallelWriteTestDefinition>(channel).ServerStreaming(1, 2, 3);
            await streaming.ResponseStream.ForEachAsync(x => x.Is(6));
        }

        [Fact]
        public async Task DuplexStreaming()
        {
            var streaming = await MagicOnionClient.Create<IParallelWriteTestDefinition>(channel).DuplexStreaming();
            var receive = streaming.ResponseStream.ForEachAsync(x => x.Is(100));
            try
            {
                var tasks   = Enumerable.Range(0, 10)
                            .Select(_ => 100)
                            .Select(streaming.RequestStream.WriteAsync)
                            .ToArray();
                await Task.WhenAll(tasks);
            }
            finally
            {
                await streaming.RequestStream.CompleteAsync();
            }
            await receive;
        }
    }
}