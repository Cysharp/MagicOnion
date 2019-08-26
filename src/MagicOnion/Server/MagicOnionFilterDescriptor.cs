using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public abstract class MagicOnionFilterDescriptor<TAttribute> : IMagicOnionFilterFactory<TAttribute>
        where TAttribute: class
    {
        static readonly MethodInfo serviceLocatorGetServiceT = typeof(IServiceLocator).GetMethod("GetService");

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

        public TAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return GetOrCreateInstance(serviceLocator);
        }

        protected TAttribute GetOrCreateInstance(IServiceLocator serviceLocator)
        {
            if (Instance != null) return Instance;

            var filterType = Type;
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

    public class MagicOnionServiceFilterDescriptor : MagicOnionFilterDescriptor<MagicOnionFilterAttribute>
    {
        public MagicOnionServiceFilterDescriptor(Type type, int order = 0)
            : base(type, order)
        {
        }

        public MagicOnionServiceFilterDescriptor(MagicOnionFilterAttribute instance, int order = 0)
            : base(instance, order)
        {
        }
    }

    public class StreamingHubFilterDescriptor : MagicOnionFilterDescriptor<StreamingHubFilterAttribute>
    {
        public StreamingHubFilterDescriptor(Type type, int order = 0)
            : base(type, order)
        {
        }

        public StreamingHubFilterDescriptor(StreamingHubFilterAttribute instance, int order = 0)
            : base(instance, order)
        {
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
        public static void Add(this IList<MagicOnionServiceFilterDescriptor> descriptors, Type filterType)
        {
            descriptors.Add(new MagicOnionServiceFilterDescriptor(filterType));
        }

        /// <summary>
        /// Adds the MagicOnion filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<T>(this IList<MagicOnionServiceFilterDescriptor> descriptors)
            where T : MagicOnionFilterAttribute
        {
            descriptors.Add(new MagicOnionServiceFilterDescriptor(typeof(T)));
        }

        /// <summary>
        /// Adds the MagicOnion filter instance as singleton.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="filterInstance"></param>
        public static void Add(this IList<MagicOnionServiceFilterDescriptor> descriptors, MagicOnionFilterAttribute filterInstance)
        {
            if (filterInstance == null) throw new ArgumentNullException(nameof(filterInstance));

            descriptors.Add(new MagicOnionServiceFilterDescriptor(filterInstance));
        }

        /// <summary>
        /// Adds the MagicOnion StreamingHub filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<T>(this IList<StreamingHubFilterDescriptor> descriptors)
            where T : StreamingHubFilterAttribute
        {
            descriptors.Add(new StreamingHubFilterDescriptor(typeof(T)));
        }

        /// <summary>
        /// Adds the MagicOnion StreamingHub filter instance as singleton.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="filterInstance"></param>
        public static void Add(this IList<StreamingHubFilterDescriptor> descriptors, StreamingHubFilterAttribute filterInstance)
        {
            if (filterInstance == null) throw new ArgumentNullException(nameof(filterInstance));

            descriptors.Add(new StreamingHubFilterDescriptor(filterInstance));
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