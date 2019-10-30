using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace MagicOnion.Hosting
{
    public class ServiceLocatorBridge : IServiceLocator
    {
        readonly IServiceProvider serviceProvider;

        public ServiceLocatorBridge(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public T GetService<T>()
        {
            return serviceProvider.GetService<T>();
        }
    }
}
