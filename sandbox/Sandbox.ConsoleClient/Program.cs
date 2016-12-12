using Grpc.Core;
using MagicOnion.Client;
using Sandbox.ConsoleServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client:::");

            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            channel.ConnectAsync().Wait();
            var c = MagicOnionClient.Create<IMyFirstService>(channel);

            UnaryRun(c).GetAwaiter().GetResult();
            ClientStreamRun(c).GetAwaiter().GetResult();
            ServerStreamRun(c).GetAwaiter().GetResult();
            DuplexStreamRun(c).GetAwaiter().GetResult();

            // many run
            UnaryDoDoDoRun(c).GetAwaiter().GetResult();
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

        static async Task UnaryDoDoDoRun(IMyFirstService client)
        {
            List<Task> t = new List<Task>();
            Parallel.For(0, 100, x =>
            {
                lock (t)
                {
                    t.Add(client.SumAsync(x, x).ContinueWith(y => y.Result.ResponseAsync).Unwrap());
                }
            });
            await Task.WhenAll(t);
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
