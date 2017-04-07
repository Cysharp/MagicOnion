using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Client;
using MagicOnion.Client.EmbeddedServices;
using MessagePack;
using Sandbox.ConsoleServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Client:::");

                //UnaryCSharpSevenTest().GetAwaiter().GetResult();
                //return;

                //GrpcEnvironment.SetThreadPoolSize(1000);
                //GrpcEnvironment.SetCompletionQueueCount(1000);

                //Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
                GrpcEnvironment.SetLogger(new ConsoleLogger());

                var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
                //channel.ConnectAsync().Wait();

                var c = MagicOnionClient.Create<IStandard>(channel);

                RunTest(c).GetAwaiter().GetResult();
                return;


                //return;
                ////c.SumAsync(10, 20).GetAwaiter().GetResult().ResponseAsync.GetAwaiter().GetResult();

                //var c2 = MagicOnionClient.Create<IArgumentPattern>(channel);
                //var c3 = MagicOnionClient.Create<Sandbox.ConsoleServer.IChatRoomService>(channel);
                //var c4 = MagicOnionClient.Create<Sandbox.ConsoleServer.IStandard>(channel);

                //// TestHeartbeat(channel).GetAwaiter().GetResult();
                //UnaryRun(c).GetAwaiter().GetResult();
                //ClientStreamRun(c).GetAwaiter().GetResult();
                //DuplexStreamRun(c).GetAwaiter().GetResult();
                //ServerStreamRun(c).GetAwaiter().GetResult();

                //// many run
                ////UnaryLoadTest(c).GetAwaiter().GetResult();


                ////                HearbeatClient.Test(channel).GetAwaiter().GetResult();
                //Console.ReadLine();

                //              ChatClient.Run(channel).GetAwaiter().GetResult();
                //TestHeartbeat(channel).GetAwaiter().GetResult();
            }
            finally
            {
                var asm = AssemblyHolder.Save();
                Verify(asm);
                Console.WriteLine("end");
            }
        }


        static async Task RunTest(IStandard c)
        {
            var huga1 = await c.NullableCheck(false);
            var huga2 = await c.NullableCheck(true);

        }


        // client, perfect!

        static async Task UnaryCSharpSevenTest()
        {
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            var client = MagicOnionClient.Create<IStandard>(channel);

            var r1 = await client.Unary1(1, 10);
            var r2 = await client.Unary2(100, 20);

            // Console.WriteLine((r1, r2));
        }























        static void Verify(params AssemblyBuilder[] builders)
        {
            var path = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\PEVerify.exe";

            foreach (var targetDll in builders)
            {
                var psi = new ProcessStartInfo(path, targetDll.GetName().Name + ".dll")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                var p = Process.Start(psi);
                var data = p.StandardOutput.ReadToEnd();
                Console.WriteLine(data);
            }
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


}
