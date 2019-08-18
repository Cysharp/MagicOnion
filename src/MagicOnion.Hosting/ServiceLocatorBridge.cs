using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MagicOnion.Hosting
{
    public class ServiceLocatorBridge : IServiceLocator
    {
        IServiceProvider provider;
        readonly IServiceCollection serviceCollection;

        public ServiceLocatorBridge(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public T GetService<T>()
        {
            if (provider == null) throw new InvalidOperationException("ServiceProvider has already been built.");

            return provider.GetService<T>();
        }

        public void Register<T>()
        {
            if (provider != null) throw new InvalidOperationException("ServiceProvider has already been built.");
            serviceCollection.AddTransient(typeof(T));
        }

        public void Register<T>(T singleton)
        {
            if (provider != null) throw new InvalidOperationException("ServiceProvider has already been built.");
            serviceCollection.AddSingleton(typeof(T), singleton);
        }

        public void Build()
        {
            if (provider != null) throw new InvalidOperationException("ServiceProvider has already been built.");
            provider = this.serviceCollection.BuildServiceProvider();
        }
    }
}
