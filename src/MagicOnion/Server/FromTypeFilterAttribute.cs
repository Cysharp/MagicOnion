using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        static readonly MethodInfo serviceLocatorGetServiceT = typeof(IServiceLocator).GetMethod("GetService");

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

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            return CreateInstance<MagicOnionFilterAttribute>(serviceLocator);
        }

        StreamingHubFilterAttribute IMagicOnionFilterFactory<StreamingHubFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            return CreateInstance<StreamingHubFilterAttribute>(serviceLocator);
        }

        protected T CreateInstance<T>(IServiceLocator serviceLocator)
        {
            var filterType = Type;
            var ctors = filterType.GetConstructors();
            var ctor = ctors.Select(x => (Ctor: x, Parameters: x.GetParameters()))
                .OrderByDescending(x => x.Parameters.Length)
                .First();

            var @params = ctor.Parameters
                .Select((x, i) => (Arguments.Length > i) ? Arguments[i] : serviceLocatorGetServiceT.MakeGenericMethod(x.ParameterType).Invoke(serviceLocator, null))
                .ToArray();

            return (T)Activator.CreateInstance(filterType, @params);
        }
    }
}
