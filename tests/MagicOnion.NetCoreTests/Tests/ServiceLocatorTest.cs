using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MagicOnion.Tests;
using Xunit;

namespace MagicOnion.NetCoreTests.Tests
{
    [Collection(nameof(ServiceLocatorTestCollectionFixture))]
    public class ServiceLocatorTest : IDisposable
    {
        readonly Channel channel;
        readonly MagicOnionOptions options;

        public ServiceLocatorTest(ServiceLocatorTestServerFixture serverFixture)
        {
            channel = serverFixture.DefaultChannel;
            options = serverFixture.Options;
        }

        public void Dispose()
        {
            var serviceLocator = ((ServiceLocatorTestServerFixture.DummyServiceLocator)options.ServiceLocator);
            serviceLocator.StackedScopes.Clear();
            serviceLocator.PoppedScopes.Clear();
        }

        [Fact]
        public async Task ScopedCreateInstancePerUnaryCall()
        {
            var client = MagicOnionClient.Create<IServiceLocatorTestScopedService>(channel);
            await client.HelloAsync();
            await client.HelloAsync();
            await client.HelloAsync();
            await client.HelloAsync();
            await client.HelloAsync();

            var serviceLocator = ((ServiceLocatorTestServerFixture.DummyServiceLocator) options.ServiceLocator);
            Assert.Empty(serviceLocator.StackedScopes);
            Assert.Equal(5, serviceLocator.PoppedScopes.Count);
        }

        [Fact]
        public async Task ScopedCreateInstancePerStreamingHub()
        {
            var serviceLocator = ((ServiceLocatorTestServerFixture.DummyServiceLocator)options.ServiceLocator);
            Assert.Empty(serviceLocator.StackedScopes);

            var client = StreamingHubClient.Connect<IServiceLocatorTestScopedHub, IServiceLocatorTestScopedHubReceiver>(channel, null);
            await client.HelloAsync();
            Assert.Single(serviceLocator.StackedScopes);
            Assert.Empty(serviceLocator.PoppedScopes);

            await client.HelloAsync();
            await client.HelloAsync();
            await client.HelloAsync();
            await client.HelloAsync();
            Assert.Single(serviceLocator.StackedScopes);
            Assert.Empty(serviceLocator.PoppedScopes);

            await client.DisposeAsync();

            Assert.Empty(serviceLocator.StackedScopes);
            Assert.Single(serviceLocator.PoppedScopes);
        }
    }

    public interface IServiceLocatorTestScopedService : IService<IServiceLocatorTestScopedService>
    {
        UnaryResult<string> HelloAsync();
    }
    public class ServiceLocatorTestScopedService : ServiceBase<IServiceLocatorTestScopedService>, IServiceLocatorTestScopedService
    {
        public UnaryResult<string> HelloAsync() => UnaryResult("Konnichiwa");
    }

    public interface IServiceLocatorTestScopedHubReceiver { }
    public interface IServiceLocatorTestScopedHub : IStreamingHub<IServiceLocatorTestScopedHub, IServiceLocatorTestScopedHubReceiver>
    {
        Task<string> HelloAsync();
    }
    public class ServiceLocatorTestScopedHub : StreamingHubBase<IServiceLocatorTestScopedHub, IServiceLocatorTestScopedHubReceiver>, IServiceLocatorTestScopedHub
    {
        public Task<string> HelloAsync() => Task.FromResult("Konnichiwa");
    }

    [CollectionDefinition(nameof(ServiceLocatorTestCollectionFixture))]
    public class ServiceLocatorTestCollectionFixture : ICollectionFixture<ServiceLocatorTestServerFixture>
    {

    }
    public class ServiceLocatorTestServerFixture : ServerFixture
    {
        protected override MagicOnionOptions CreateMagicOnionOptions()
        {
            var options = base.CreateMagicOnionOptions();
            options.ServiceLocator = new DummyServiceLocator();
            return options;
        }

        protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            => MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(ServiceLocatorTestScopedService), typeof(ServiceLocatorTestScopedHub) }, options);

        public class DummyServiceLocator : IServiceLocator
        {
            public Stack<IServiceLocatorScope> StackedScopes { get; } = new Stack<IServiceLocatorScope>();
            public Stack<IServiceLocatorScope> PoppedScopes { get; } = new Stack<IServiceLocatorScope>();

            public T GetService<T>()
            {
                return default;
            }

            public IServiceLocatorScope CreateScope()
            {
                var scope = new DummyServiceLocatorScope(this);
                StackedScopes.Push(scope);
                return scope;
            }
        }

        public class DummyServiceLocatorScope : IServiceLocatorScope
        {
            private readonly DummyServiceLocator _serviceLocator;

            public IServiceLocator ServiceLocator => _serviceLocator;

            public DummyServiceLocatorScope(DummyServiceLocator serviceLocator)
            {
                _serviceLocator = serviceLocator;
            }

            public void Dispose()
            {
                var scope = _serviceLocator.StackedScopes.Pop();
                _serviceLocator.PoppedScopes.Push(scope);
            }
        }
    }
}
