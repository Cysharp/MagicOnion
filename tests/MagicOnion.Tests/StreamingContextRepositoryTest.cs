﻿#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

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
using Xunit.Abstractions;

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
            cache = new StreamingContextRepository<IStreamingRepositoryTestService>(this.GetConnectionContext());
            return UnaryResult(true);
        }

        public async Task<UnaryResult<bool>> Unregister()
        {
            cache.Dispose();
            return UnaryResult(true);
        }

        public async Task<UnaryResult<bool>> SendMessage(string message)
        {
            await cache.WriteAsync(x => nameof(x.ReceiveMessages), message);
            return UnaryResult(true);
        }

        public async Task<ServerStreamingResult<string>> ReceiveMessages()
        {
            return await cache.RegisterStreamingMethod(this, ReceiveMessages);
        }
    }
    
    [Collection(nameof(AllAssemblyGrpcServerFixture))]
    public class StreamingContextRepositoryTest
    {
        ITestOutputHelper logger;
        Channel channel;

        public StreamingContextRepositoryTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
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