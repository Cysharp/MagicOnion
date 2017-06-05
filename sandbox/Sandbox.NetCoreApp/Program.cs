#pragma warning disable CS1998

using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace Sandbox.NetCoreApp
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<int> SumAsync(int x, int y);
    }

    // implement RPC service.
    // inehrit ServiceBase<interface>, interface
    public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
    {
        public async UnaryResult<int> SumAsync(int x, int y)
        {
            Logger.Debug($"Received:{x}, {y}");
            return x + y;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

            var server = new global::Grpc.Core.Server
            {
                Services = { service },
                Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
            };

            // launch gRPC Server.
            server.Start();

            // sample, launch server/client in same app.
            Task.Run(() =>
            {
                ClientImpl();
            });

            Console.ReadLine();
        }


        // Blank, used by next section 
        static async void ClientImpl()
        {
            // standard gRPC channel
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

            // create MagicOnion dynamic client proxy
            var client = MagicOnionClient.Create<IMyFirstService>(channel);

            // call method.
            var result = await client.SumAsync(100, 200);
            Console.WriteLine("Client Received:" + result);
        }
    }
}

#pragma warning restore CS1998