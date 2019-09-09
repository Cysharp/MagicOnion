using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicOnion.Server
{
    /// <summary>
    /// A MagicOnion filter that provided another filter via <see cref="IServiceLocator"/>.
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

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            if (!typeof(MagicOnionFilterAttribute).IsAssignableFrom(Type)) throw new InvalidOperationException($"Type '{Type.FullName}' doesn't inherit from {nameof(MagicOnionFilterAttribute)}.");
            return CreateInstance<MagicOnionFilterAttribute>(serviceLocator);
        }

        StreamingHubFilterAttribute IMagicOnionFilterFactory<StreamingHubFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            if (!typeof(StreamingHubFilterAttribute).IsAssignableFrom(Type)) throw new InvalidOperationException($"Type '{Type.FullName}' doesn't inherit from {nameof(StreamingHubFilterAttribute)}.");
            return CreateInstance<StreamingHubFilterAttribute>(serviceLocator);
        }

        protected T CreateInstance<T>(IServiceLocator serviceLocator)
        {
            return (T)serviceLocator.GetService(Type);
        }
    }
}
