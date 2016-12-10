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
            Run().GetAwaiter().GetResult();
        }

        static async Task Run()
        {
            try
            {
                var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
                await channel.ConnectAsync();

                var c = MagicOnionClient.Create<IMyFirstService>(channel);

                //var huga = c.GetFeatureAsync(10, 20);

                var vvvvv = await await c.SumAsync(10, 20);
                Console.WriteLine("SumAsync:" + vvvvv);

                var v2 = await c.SumAsync2(999, 1000);
                Console.WriteLine("v2:" + v2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
