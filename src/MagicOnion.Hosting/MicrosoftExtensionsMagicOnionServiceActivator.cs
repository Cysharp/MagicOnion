using System;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Hosting
{
    public class MicrosoftExtensionsMagicOnionServiceActivator : IMagicOnionServiceActivator
    {
        public T Create<T>(IServiceLocator serviceLocator)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance<T>((IServiceProvider)serviceLocator);
        }
    }

}