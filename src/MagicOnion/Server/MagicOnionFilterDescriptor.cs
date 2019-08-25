using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public class MagicOnionFilterDescriptor<TAttribute>
        where TAttribute: class
    {
        public Type Type { get; }
        public TAttribute Instance { get; }
        public int Order { get; }

        public MagicOnionFilterDescriptor(Type type, int order = 0)
        {
            Type = type;
            Instance = null;
            Order = order;
        }

        public MagicOnionFilterDescriptor(TAttribute instance, int order = 0)
        {
            Type = null;
            Instance = instance;
            Order = order;
        }
    }

    public static class MagicOnionFilterDescriptorExtensions
    {
        static readonly MethodInfo serviceLocatorGetServiceT = typeof(IServiceLocator).GetMethod("GetService");

        /// <summary>
        /// Adds the MagicOnion filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="filterType"></param>
        public static void Add<TAttribute>(this IList<MagicOnionFilterDescriptor<TAttribute>> descriptors, Type filterType)
            where TAttribute: class
        {
            if (!typeof(MagicOnionFilterAttribute).IsAssignableFrom(filterType))
            {
                throw new ArgumentException($"Type '{filterType.FullName}' doesn't inherit from MagicOnionFilterAttribute.", nameof(filterType));
            }

            descriptors.Add(new MagicOnionFilterDescriptor<TAttribute>(filterType));
        }

        /// <summary>
        /// Adds the MagicOnion filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<TAttribute, T>(this IList<MagicOnionFilterDescriptor<TAttribute>> descriptors)
            where TAttribute : class
        {
            descriptors.Add(new MagicOnionFilterDescriptor<TAttribute>(typeof(T)));
        }

        /// <summary>
        /// Adds the MagicOnion filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<T>(this IList<MagicOnionFilterDescriptor<MagicOnionFilterAttribute>> descriptors)
            where T : MagicOnionFilterAttribute
        {
            descriptors.Add(new MagicOnionFilterDescriptor<MagicOnionFilterAttribute>(typeof(T)));
        }

        /// <summary>
        /// Adds the MagicOnion StreamingHub filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<T>(this IList<MagicOnionFilterDescriptor<StreamingHubFilterAttribute>> descriptors)
            where T : StreamingHubFilterAttribute
        {
            descriptors.Add(new MagicOnionFilterDescriptor<StreamingHubFilterAttribute>(typeof(T)));
        }

        /// <summary>
        /// Adds the MagicOnion filter instance as singleton.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="filterInstance"></param>
        public static void Add<TAttribute>(this IList<MagicOnionFilterDescriptor<TAttribute>> descriptors, TAttribute filterInstance)
            where TAttribute: class
        {
            if (filterInstance == null) throw new ArgumentNullException(nameof(filterInstance));

            descriptors.Add(new MagicOnionFilterDescriptor<TAttribute>(filterInstance));
        }

        internal static TAttribute GetOrCreateInstance<TAttribute>(this MagicOnionFilterDescriptor<TAttribute> descriptor, IServiceLocator serviceLocator)
            where TAttribute: class
        {
            if (descriptor.Instance != null) return descriptor.Instance;

            var filterType = descriptor.Type;
            var ctors = filterType.GetConstructors();
            var ctor = ctors.Select(x => (Ctor: x, Parameters: x.GetParameters()))
                .OrderByDescending(x => x.Parameters.Length)
                .First();

            var @params = ctor.Parameters
                .Select(x => serviceLocatorGetServiceT.MakeGenericMethod(x.ParameterType).Invoke(serviceLocator, null))
                .ToArray();

            return (TAttribute)Activator.CreateInstance(filterType, @params);
        }
    }
}