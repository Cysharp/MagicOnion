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
            if (provider == null)
            {
                provider = serviceCollection.BuildServiceProvider();
            }

            return provider.GetService<T>();
        }

        public void Register<T>()
        {
            serviceCollection.AddTransient(typeof(T));
            provider = null;
        }

        public void Register<T>(T singleton)
        {
            serviceCollection.AddSingleton(typeof(T), singleton);
            provider = null;
        }

        public void Build()
        {
            provider = this.serviceCollection.BuildServiceProvider();
            provider = null;
        }
    }
}
