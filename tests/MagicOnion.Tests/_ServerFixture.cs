using Grpc.Core;
using MagicOnion.Server;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Tests
{
    public class ServerFixture : IDisposable
    {
        Grpc.Core.Server server;
        public ServerPort ServerPort { get; private set; }

        public ServerFixture()
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

            var port = new Random().Next(10000, 30000);
            var serverPort = new ServerPort("localhost", port, ServerCredentials.Insecure);

            server = new global::Grpc.Core.Server
            {
                Services = { service.ServerServiceDefinition },
                Ports = { serverPort }
            };

            server.Start();

            ServerPort = serverPort;
        }

        public void Dispose()
        {
            server.ShutdownAsync().Wait();
        }
    }
}