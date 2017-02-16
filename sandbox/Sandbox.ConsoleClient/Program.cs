using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Client;
using MagicOnion.Client.EmbeddedServices;
using Sandbox.ConsoleClient;
using Sandbox.ConsoleServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client:::");

            //GrpcEnvironment.SetThreadPoolSize(1000);
            //GrpcEnvironment.SetCompletionQueueCount(1000);

            //Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            channel.ConnectAsync().Wait();
            var c = MagicOnionClient.Create<IMyFirstService>(channel);

            // TestHeartbeat(channel).GetAwaiter().GetResult();
            //UnaryRun(c).GetAwaiter().GetResult();
            ClientStreamRun(c).GetAwaiter().GetResult();
            ServerStreamRun(c).GetAwaiter().GetResult();
            DuplexStreamRun(c).GetAwaiter().GetResult();

            // many run
            //UnaryLoadTest(c).GetAwaiter().GetResult();


            //HearbeatClient.Test(channel).GetAwaiter().GetResult();
            //Console.ReadLine();

            //ChatClient.Run(channel).GetAwaiter().GetResult();
            //TestHeartbeat(channel).GetAwaiter().GetResult();
        }

        static async Task TestHeartbeat(Channel channel)
        {
            await channel.ConnectAsync();
            Console.WriteLine("Client -> Server, " + await new PingClient(channel).Ping() + "ms");
            Console.WriteLine("Client -> Server, " + await new PingClient(channel).Ping() + "ms");

            var cc = new ChannelContext(channel);
            cc.RegisterDisconnectedAction(() =>
            {
                Console.WriteLine("disconnected detected!");
            });
            await cc.WaitConnectComplete();




            Console.ReadLine();
            //cc.Dispose();
            await new PingClient(channel).Ping();

            Console.ReadLine();

        }

        static async Task UnaryRun(IMyFirstService client)
        {
            try
            {
                var vvvvv = await await client.SumAsync(10, 20);
                Console.WriteLine("SumAsync:" + vvvvv);

                var v2 = await client.SumAsync2(999, 1000);
                Console.WriteLine("v2:" + v2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task UnaryLoadTest(IMyFirstService client)
        {
            const int requestCount = 100000;

            ThreadPool.SetMinThreads(1000, 1000);
            var sw = Stopwatch.StartNew();
            Task[] t = new Task[requestCount];
            for (int i = 0; i < requestCount; i++)
            {
                t[i] = (client.SumAsync(i, i).ContinueWith(y => y.Result.ResponseAsync).Unwrap());
            }
            await Task.WhenAll(t);

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms"); // 10000request, x-ms
            var one = sw.Elapsed.TotalMilliseconds / requestCount; // 1request, ms
            Console.WriteLine(one);
            Console.WriteLine((1000.0 / one) + "req per/sec");
        }

        static async Task ClientStreamRun(IMyFirstService client)
        {
            try
            {
                var stream = await client.StreamingOne();

                for (int i = 0; i < 3; i++)
                {
                    await stream.RequestStream.WriteAsync(i);
                }
                await stream.RequestStream.CompleteAsync();

                var response = await stream.ResponseAsync;

                Console.WriteLine("Response:" + response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task ServerStreamRun(IMyFirstService client)
        {
            try
            {
                var stream = await client.StreamingTwo(10, 20, 3);

                await stream.ResponseStream.ForEachAsync(x =>
                {
                    Console.WriteLine("ServerStream Response:" + x);
                });

                var stream2 = client.StreamingTwo2(10, 20, 3);
                await stream2.ResponseStream.ForEachAsync(x => { });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task DuplexStreamRun(IMyFirstService client)
        {
            try
            {
                var stream = await client.StreamingThree();

                var count = 0;
                await stream.ResponseStream.ForEachAsync(async x =>
                {
                    Console.WriteLine("DuplexStream Response:" + x);

                    await stream.RequestStream.WriteAsync(count++);
                    if (x == "finish")
                    {
                        await stream.RequestStream.CompleteAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }

    public class ClientSimu : MagicOnionClientBase<IMyFirstService>, IMyFirstService
    {
        protected ClientSimu(CallInvoker callInvoker) : base(callInvoker)
        {
        }

        public Task<ClientStreamingResult<int, string>> StreamingOne()
        {
            var callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(null, host, option);
            var result = new ClientStreamingResult<int, string>(callResult, null, null);
            return Task.FromResult(result);
        }

        public Task<DuplexStreamingResult<int, string>> StreamingThree()
        {
            throw new NotImplementedException();
        }

        public Task<ServerStreamingResult<string>> StreamingTwo(int x, int y, int z)
        {
            // throw new NotImplementedException();
            byte[] request = null; // marshalling

            var callResult = callInvoker.AsyncServerStreamingCall<byte[], byte[]>(null, host, option, request);
            var result = new ServerStreamingResult<string>(callResult, null); // response marshaller
            return Task.FromResult(result);
        }

        public ServerStreamingResult<string> StreamingTwo2(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public Task<UnaryResult<string>> SumAsync(int x, int y)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<string> SumAsync2(int x, int y)
        {
            throw new NotImplementedException();
        }

        protected override MagicOnionClientBase<IMyFirstService> Clone()
        {
            throw new NotImplementedException();
        }
    }
}
