using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

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
                    using (var rng = RandomNumberGenerator.Create())
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
        public Channel DefaultChannel { get; private set; }

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
            DefaultChannel = new Channel(serverPort.Host, serverPort.Port, ChannelCredentials.Insecure);
        }

        public T CreateClient<T>()
            where T : IService<T>
        {
            return MagicOnionClient.Create<T>(DefaultChannel);
        }

        public void Dispose()
        {
            DefaultChannel.ShutdownAsync().Wait();
            server.ShutdownAsync().Wait();
        }
    }

    [CollectionDefinition(nameof(AllAssemblyGrpcServerFixture))]
    public class AllAssemblyGrpcServerFixture : ICollectionFixture<ServerFixture>
    {

    }
}