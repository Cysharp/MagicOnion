using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MagicOnion.Hosting
{
    public class MicrosoftExtensionsServiceLocator : IServiceLocator, IServiceProvider
    {
        readonly IServiceProvider serviceProvider;

        public MicrosoftExtensionsServiceLocator(IServiceProvider serviceProvider, MagicOnionOptions options)
        {
            this.serviceProvider = serviceProvider;
        }

        public IServiceLocatorScope CreateScope()
        {
            return new ServiceLocatorScope(serviceProvider.CreateScope());
        }

        public T GetService<T>()
        {
            return serviceProvider.GetService<T>();
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }

        class ServiceLocatorScope : IServiceLocatorScope, IServiceLocator, IServiceProvider
        {
            readonly IServiceScope scope;

            public IServiceLocator ServiceLocator => this;

            public ServiceLocatorScope(IServiceScope scope)
            {
                this.scope = scope;
            }

            public void Dispose()
            {
                scope.Dispose();
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                return scope.ServiceProvider.GetService(serviceType);
            }

            public T GetService<T>()
            {
                return scope.ServiceProvider.GetService<T>();
            }

            public IServiceLocatorScope CreateScope()
            {
                return new ServiceLocatorScope(scope.ServiceProvider.CreateScope());
            }
        }
    }
}
