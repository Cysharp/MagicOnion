using System;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Hosting
{
    public class GenericHostServiceActivator : IServiceActivator
    {
        readonly ServiceProviderOptionsAdapter serviceProviderOptionsAdapter;

        public GenericHostServiceActivator(IServiceProvider serviceProvider, MagicOnionOptions options)
        {
            this.serviceProviderOptionsAdapter = new ServiceProviderOptionsAdapter(serviceProvider, options);
        }

        public T Create<T>(IServiceLocator serviceLocator)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance<T>(serviceProviderOptionsAdapter);
        }
    }

    internal class ServiceProviderOptionsAdapter : IServiceProvider
    {
        readonly IServiceProvider serviceProvider;
        readonly MagicOnionOptions options;

        public ServiceProviderOptionsAdapter(IServiceProvider serviceProvider, MagicOnionOptions options)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
        }

        public object GetService(Type serviceType)
        {
            var value = default(object);

            if (serviceType == typeof(IFormatterResolver)) value = options.FormatterResolver;
            else if (serviceType == typeof(IMagicOnionLogger)) value = options.MagicOnionLogger;
            else if (serviceType == typeof(IGroupRepositoryFactory)) value = options.DefaultGroupRepositoryFactory;

            return value ?? serviceProvider.GetService(serviceType);
        }
    }
}