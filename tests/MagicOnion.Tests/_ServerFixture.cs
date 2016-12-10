using Grpc.Core;
using MagicOnion.Server;
using System;

namespace MagicOnion.Tests
{
    public class ServerFixture : IDisposable
    {
        Grpc.Core.Server server;
        public ServerPort ServerPort { get; private set; }

        public ServerFixture()
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition();

            var port = new Random().Next(10000, 60000);
            var serverPort = new ServerPort("localhost", port, ServerCredentials.Insecure);

            server = new global::Grpc.Core.Server
            {
                Services = { service },
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