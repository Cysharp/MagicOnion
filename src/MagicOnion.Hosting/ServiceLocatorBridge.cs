using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MagicOnion.Hosting
{
    public class ServiceLocatorBridge : IServiceLocator
    {
        internal IServiceProvider provider; // set after register service completed.
        readonly IServiceCollection serviceCollection;

        public ServiceLocatorBridge(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public T GetService<T>()
        {
            if (provider == null)
            {
                // get raw before provider create.
                foreach (var item in serviceCollection)
                {
                    if (item.ServiceType == typeof(T))
                    {
                        if (item.Lifetime == ServiceLifetime.Singleton)
                        {
                            return (T)item.ImplementationInstance;
                        }
                        else if (item.Lifetime == ServiceLifetime.Transient)
                        {
                            return (T)Activator.CreateInstance(item.ServiceType);
                        }
                    }
                }
                throw new Exception("Service does not found in ServiceCollection.");
            }

            return provider.GetService<T>();
        }

        public void Register<T>()
        {
            serviceCollection.AddTransient(typeof(T));
        }

        public void Register<T>(T singleton)
        {
            serviceCollection.AddSingleton(typeof(T), singleton);
        }
    }
}
