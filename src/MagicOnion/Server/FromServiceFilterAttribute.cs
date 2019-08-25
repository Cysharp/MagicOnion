using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MagicOnion.Server
{
    /// <summary>
    /// A MagicOnion filter that finds another filter in an <see cref="IServiceLocator"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FromServiceFilterAttribute : Attribute
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
    }
}
