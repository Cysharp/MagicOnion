using Grpc.Core;
using System.Reflection;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;

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

    public abstract class ServerFixture : IDisposable
    {
        Grpc.Core.Server server;
        public ServerPort ServerPort { get; private set; }
        public Channel DefaultChannel { get; private set; }
        public MagicOnionOptions Options { get; private set; }
        public NonCachedDefaultServiceLocator ServiceLocator { get; } = new NonCachedDefaultServiceLocator();

        public ServerFixture()
        {
            PrepareServer();
        }

        protected virtual MagicOnionOptions CreateMagicOnionOptions()
            => new MagicOnionOptions
            {
                IsReturnExceptionStackTraceInErrorDetail = true,
                ServiceLocator = ServiceLocator
            };

        protected abstract MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options);

        protected virtual void PrepareServer()
        {
            var options = CreateMagicOnionOptions();
            var service = BuildServerServiceDefinition(options);

            var port = RandomProvider.ThreadRandom.Next(10000, 30000);
            var serverPort = new ServerPort("localhost", port, ServerCredentials.Insecure);

            server = new global::Grpc.Core.Server
            {
                Services = { service.ServerServiceDefinition },
                Ports = { serverPort }
            };

            server.Start();

            Options = options;
            ServerPort = serverPort;
            DefaultChannel = new Channel(serverPort.Host, serverPort.Port, ChannelCredentials.Insecure);
        }

        public T CreateClient<T>()
            where T : IService<T>
        {
            return MagicOnionClient.Create<T>(DefaultChannel);
        }

        public TStreamingHub CreateStreamingHubClient<TStreamingHub, TReceiver>(TReceiver receiver)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return StreamingHubClient.Connect<TStreamingHub, TReceiver>(DefaultChannel, receiver);
        }

        public void Dispose()
        {
            try { DefaultChannel.ShutdownAsync().Wait(1000); } catch { }

            try { server.ShutdownAsync().Wait(1000); } catch { }
        }
    }

    public class ServerFixture<T> : ServerFixture
    {
        protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            => MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(T) }, options);
    }
    public class ServerFixture<T1, T2> : ServerFixture
    {
        protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            => MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(T1), typeof(T2) }, options);
    }
    public class ServerFixture<T1, T2, T3> : ServerFixture
    {
        protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            => MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(T1), typeof(T2), typeof(T3) }, options);
    }
    public class ServerFixture<T1, T2, T3, T4> : ServerFixture
    {
        protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            => MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, options);
    }

    [CollectionDefinition(nameof(AllAssemblyGrpcServerFixture))]
    public class AllAssemblyGrpcServerFixture : ICollectionFixture<ServerFixture>
    { }

    public class NonCachedDefaultServiceLocator : IServiceLocator, IServiceLocatorScope
    {
        Dictionary<Type, object> factories = new Dictionary<Type, object>();

        public T GetService<T>()
        {
            return ((Func<T>)factories[typeof(T)])();
        }

        public void Register<T>(T singleton)
        {
            factories[typeof(T)] = (Func<T>)(() => singleton);
        }

        public void Register<T>()
        {
            if (!typeof(T).GetConstructors().Any(x => x.GetParameters().Length == 0))
            {
                throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", typeof(T).FullName));
            }

            factories[typeof(T)] = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }

        public IServiceLocatorScope CreateScope() => this;

        IServiceLocator IServiceLocatorScope.ServiceLocator => this;

        public void Dispose()
        {
        }
    }
}