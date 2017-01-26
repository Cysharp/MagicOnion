using Grpc.Core;
using MagicOnion.Server;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MagicOnion.Tests
{
    public static class RandomProvider
    {
        [ThreadStatic]
        static Random random;

        public static Random ThreadRandom
        {
            get
            {
                if (random == null)
                {
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        var buffer = new byte[sizeof(int)];
                        rng.GetBytes(buffer);
                        var seed = BitConverter.ToInt32(buffer, 0);
                        random = new Random(seed);
                    }
                }

                return random;
            }
        }
    }

    public class ServerFixture : IDisposable
    {
        Grpc.Core.Server server;
        public ServerPort ServerPort { get; private set; }

        public ServerFixture()
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

            var port = RandomProvider.ThreadRandom.Next(10000, 30000);
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