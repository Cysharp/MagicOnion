#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MagicOnion.Tests
{
    public interface IStreamingContextGroupTestService : IService<IStreamingContextGroupTestService>
    {
        Task<UnaryResult<bool>> Register();
        Task<UnaryResult<bool>> Unregister();
        Task<UnaryResult<bool>> SendMessage(string message);
        Task<ServerStreamingResult<string>> ReceiveMessages();
    }

    public class StreamingContextGroupTestServiceService : ServiceBase<IStreamingContextGroupTestService>, IStreamingContextGroupTestService
    {
        static StreamingContextGroup<string, IStreamingContextGroupTestService> group = new StreamingContextGroup<string, IStreamingContextGroupTestService>();

        public async Task<UnaryResult<bool>> Register()
        {
            var id = this.GetConnectionContext().ConnectionId;
            group.Add(id, new StreamingContextRepository<IStreamingContextGroupTestService>());
            return UnaryResult(true);
        }

        public async Task<UnaryResult<bool>> Unregister()
        {
            var id = this.GetConnectionContext().ConnectionId;
            group.Remove(id);
            return UnaryResult(true);
        }

        public async Task<UnaryResult<bool>> SendMessage(string message)
        {
            await group.BroadcastAllAsync(x => x.ReceiveMessages, message, ignoreError: false);
            return UnaryResult(true);
        }

        public async Task<ServerStreamingResult<string>> ReceiveMessages()
        {
            var id = this.GetConnectionContext().ConnectionId;
            return await group.Get(id).RegisterStreamingMethod(this, ReceiveMessages);
        }
    }



    public class StreamingContextGroupTest : IClassFixture<ServerFixture>, IDisposable
    {
        ServerPort serverPort;
        List<Channel> channels = new List<Channel>();

        public StreamingContextGroupTest(ServerFixture server)
        {
            this.serverPort = server.ServerPort;
        }

        public void Dispose()
        {
            foreach (var item in channels)
            {
                item.ShutdownAsync().Wait();
            }
        }

        [Fact]
        public async Task MyTestMethod()
        {
            var channel1 = new Channel(serverPort.Host, serverPort.Port, ChannelCredentials.Insecure);
            var channel2 = new Channel(serverPort.Host, serverPort.Port, ChannelCredentials.Insecure);
            var channel3 = new Channel(serverPort.Host, serverPort.Port, ChannelCredentials.Insecure);

            using (var c1 = new ChannelContext(channel1, () => "id1"))
            using (var c2 = new ChannelContext(channel1, () => "id2"))
            using (var c3 = new ChannelContext(channel1, () => "id3"))
            {
                var hoge = c1.CreateClient<IStreamingContextGroupTestService>();
                var huga = c2.CreateClient<IStreamingContextGroupTestService>();
                var tako = c3.CreateClient<IStreamingContextGroupTestService>();

                await await hoge.Register();
                await await huga.Register();
                await await tako.Register();

                var l1 = new List<string>();
                var l2 = new List<string>();
                var l3 = new List<string>();

                var t1 = (await hoge.ReceiveMessages()).ResponseStream.ForEachAsync(x => l1.Add(x));
                var t2 = (await huga.ReceiveMessages()).ResponseStream.ForEachAsync(x => l2.Add(x));
                var t3 = (await tako.ReceiveMessages()).ResponseStream.ForEachAsync(x => l3.Add(x));

                await Task.Delay(TimeSpan.FromMilliseconds(300));

                await await hoge.SendMessage("from message 1");
                await await huga.SendMessage("from message 2");
                await await tako.SendMessage("from message 3");

                l1.Is("from message 1", "from message 2", "from message 3");
                l2.Is("from message 1", "from message 2", "from message 3");
                l3.Is("from message 1", "from message 2", "from message 3");

                await hoge.Unregister();
                await huga.Unregister();
                await tako.Unregister();
            }
        }
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously