using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server
{
    /// <summary>
    /// A MagicOnion filter that creates another filter of type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FromTypeFilterAttribute : Attribute,
        IMagicOnionFilterFactory<MagicOnionFilterAttribute>,
        IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public Type Type { get; }

        public int Order { get; set; }

        public object[] Arguments { get; set; } = Array.Empty<object>();

        public FromTypeFilterAttribute(Type type)
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
            return (T)ActivatorUtilities.CreateInstance(serviceProvider, Type, Arguments);
        }
    }
}
