#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MagicOnion.Tests
{
    public interface IStreamingRepositoryTestService : IService<IStreamingRepositoryTestService>
    {
        Task<UnaryResult<bool>> Register();
        Task<UnaryResult<bool>> Unregister();
        Task<UnaryResult<bool>> SendMessage(string message);
        Task<ServerStreamingResult<string>> ReceiveMessages();
    }

    public class StreamingRepositoryTestService : ServiceBase<IStreamingRepositoryTestService>, IStreamingRepositoryTestService
    {
        static StreamingContextRepository<IStreamingRepositoryTestService> cache;

        public async Task<UnaryResult<bool>> Register()
        {
            cache = new StreamingContextRepository<IStreamingRepositoryTestService>();
            return UnaryResult(true);
        }

        public async Task<UnaryResult<bool>> Unregister()
        {
            cache.Dispose();
            return UnaryResult(true);
        }

        public async Task<UnaryResult<bool>> SendMessage(string message)
        {
            await cache.WriteAsync(x => x.ReceiveMessages, message);
            return UnaryResult(true);
        }

        public async Task<ServerStreamingResult<string>> ReceiveMessages()
        {
            return await cache.RegisterStreamingMethod(this, ReceiveMessages);
        }
    }

    public class StreamingContextRepositoryTest : IClassFixture<ServerFixture>, IDisposable
    {
        Channel channel;

        public StreamingContextRepositoryTest(ServerFixture server)
        {
            this.channel = new Channel(server.ServerPort.Host, server.ServerPort.Port, ChannelCredentials.Insecure);
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        [Fact]
        public async Task ParallelWrite()
        {
            using (var channelContext = new ChannelContext(channel))
            {
                await channelContext.WaitConnectComplete();

                var client = channelContext.CreateClient<IStreamingRepositoryTestService>();


                var list = new List<string>();
                await await client.Register();
                var streaming = await client.ReceiveMessages();

                var t2 = streaming.ResponseStream.ForEachAsync(x =>
                {
                    list.Add(x);
                });

                await Task.Delay(TimeSpan.FromMilliseconds(100)); // wait subscribe done...

                var tasks = Enumerable.Range(0, 100)
                    .Select(x => "Write:" + x.ToString())
                    .Select(async x => (await client.SendMessage(x)).ResponseAsync)
                    .Select(x => x.Unwrap())
                    .ToArray();


                await Task.WhenAll(tasks);
                await client.Unregister();
                await t2;
            }
        }
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously