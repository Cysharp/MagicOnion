using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server
{
    /// <summary>
    /// A MagicOnion filter that provided another filter via <see cref="IServiceProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FromServiceFilterAttribute : Attribute,
        IMagicOnionFilterFactory<MagicOnionFilterAttribute>,
        IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public Type Type { get; }

        public int Order { get; set; }

        public FromServiceFilterAttribute(Type type)
        {
            if (!typeof(MagicOnionFilterAttribute).IsAssignableFrom(type) &&
                !typeof(StreamingHubFilterAttribute).IsAssignableFrom(type))
            {
                throw new ArgumentException($"{type.FullName} doesn't inherit from MagicOnionFilterAttribute or StreamingHubFilterAttribute.", nameof(type));
            }

            Type = type;
        }

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            if (!typeof(MagicOnionFilterAttribute).IsAssignableFrom(Type)) throw new InvalidOperationException($"Type '{Type.FullName}' doesn't inherit from {nameof(MagicOnionFilterAttribute)}.");
            return CreateInstance<MagicOnionFilterAttribute>(serviceProvider);
        }

        StreamingHubFilterAttribute IMagicOnionFilterFactory<StreamingHubFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            if (!typeof(StreamingHubFilterAttribute).IsAssignableFrom(Type)) throw new InvalidOperationException($"Type '{Type.FullName}' doesn't inherit from {nameof(StreamingHubFilterAttribute)}.");
            return CreateInstance<StreamingHubFilterAttribute>(serviceProvider);
        }

        protected T CreateInstance<T>(IServiceProvider serviceProvider)
        {
            return (T)ActivatorUtilities.CreateInstance(serviceProvider, Type);
        }
    }
}
