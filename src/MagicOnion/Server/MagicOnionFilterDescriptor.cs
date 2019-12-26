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
        public IMagicOnionFilterFactory<TAttribute> Factory { get; }
        public TAttribute Instance { get; }
        public int Order { get; }

        protected MagicOnionFilterDescriptor(Type type, int order = 0)
        {
            Factory = new MagicOnionFilterTypeFactory(type, order);
            Instance = null;
            Order = order;
        }

        protected MagicOnionFilterDescriptor(TAttribute instance, int order = 0)
        {
            Factory = null;
            Instance = instance;
            Order = order;
        }

        protected MagicOnionFilterDescriptor(IMagicOnionFilterFactory<TAttribute> factory, int order = 0)
        {
            Factory = factory;
            Instance = null;
            Order = order;
        }

        public TAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            if (Instance != null) return Instance;

            return Factory.CreateInstance(serviceLocator);
        }

        // Create a filter instance from specified type.
        class MagicOnionFilterTypeFactory : IMagicOnionFilterFactory<TAttribute>
        {
            public Type Type { get; }
            public int Order { get; }

            public MagicOnionFilterTypeFactory(Type type, int order)
            {
                Type = type;
                Order = order;
            }

            public TAttribute CreateInstance(IServiceLocator serviceLocator)
            {
                var filterType = Type;
                var ctors = filterType.GetConstructors();
                var ctor = ctors.Select(x => (Ctor: x, Parameters: x.GetParameters()))
                    .OrderByDescending(x => x.Parameters.Length)
                    .First();

                var @params = ctor.Parameters
                    .Select(x => serviceLocator.GetService(x.ParameterType))
                    .ToArray();

                return (TAttribute)Activator.CreateInstance(filterType, @params);
            }
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

        public MagicOnionServiceFilterDescriptor(IMagicOnionFilterFactory<MagicOnionFilterAttribute> factory, int order = 0)
            : base(factory, order)
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

        public StreamingHubFilterDescriptor(IMagicOnionFilterFactory<StreamingHubFilterAttribute> factory, int order = 0)
            : base(factory, order)
        {
        }
    }

    public static class MagicOnionFilterDescriptorExtensions
    {
        /// <summary>
        /// Adds the MagicOnion filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<T>(this IList<MagicOnionServiceFilterDescriptor> descriptors)
        {
            if (typeof(IMagicOnionFilterFactory<MagicOnionFilterAttribute>).IsAssignableFrom(typeof(T)))
            {
                var ctor = typeof(T).GetConstructors().SingleOrDefault(x => x.GetParameters().Length == 0);
                if (ctor == null)
                {
                    throw new InvalidOperationException($"Type '{typeof(T).FullName}' has no parameter-less constructor. You can also use `Add(instance)` overload method.");
                }
                descriptors.Add(new MagicOnionServiceFilterDescriptor((IMagicOnionFilterFactory<MagicOnionFilterAttribute>)Activator.CreateInstance<T>()));
            }
            else if (typeof(MagicOnionFilterAttribute).IsAssignableFrom(typeof(T)))
            {
                descriptors.Add(new MagicOnionServiceFilterDescriptor(typeof(T)));
            }
            else
            {
                throw new InvalidOperationException($"Type '{typeof(T).FullName}' is not compatible with MagicOnionFilterAttribute or IMagicOnionFilterFactory<MagicOnionFilterAttribute>");
            }
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
        /// Adds the MagicOnion filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="factory"></param>
        public static void Add(this IList<MagicOnionServiceFilterDescriptor> descriptors, IMagicOnionFilterFactory<MagicOnionFilterAttribute> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            descriptors.Add(new MagicOnionServiceFilterDescriptor(factory));
        }

        /// <summary>
        /// Adds the MagicOnion StreamingHub filter as type.
        /// </summary>
        /// <param name="descriptors"></param>
        public static void Add<T>(this IList<StreamingHubFilterDescriptor> descriptors)
        {
            if (typeof(IMagicOnionFilterFactory<StreamingHubFilterAttribute>).IsAssignableFrom(typeof(T)))
            {
                var ctor = typeof(T).GetConstructors().SingleOrDefault(x => x.GetParameters().Length == 0);
                if (ctor == null)
                {
                    throw new InvalidOperationException($"Type '{typeof(T).FullName}' has no parameter-less constructor. You can also use `Add(instance)` overload method.");
                }
                descriptors.Add(new StreamingHubFilterDescriptor((IMagicOnionFilterFactory<StreamingHubFilterAttribute>)Activator.CreateInstance<T>()));
            }
            else if (typeof(StreamingHubFilterAttribute).IsAssignableFrom(typeof(T)))
            {
                descriptors.Add(new StreamingHubFilterDescriptor(typeof(T)));
            }
            else
            {
                throw new InvalidOperationException($"Type '{typeof(T).FullName}' is not compatible with StreamingHubFilterAttribute or IMagicOnionFilterFactory<StreamingHubFilterAttribute>");
            }
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

        /// <summary>
        /// Adds the MagicOnion StreamingHub filter instance as singleton.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="factory"></param>
        public static void Add(this IList<StreamingHubFilterDescriptor> descriptors, IMagicOnionFilterFactory<StreamingHubFilterAttribute> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            descriptors.Add(new StreamingHubFilterDescriptor(factory));
        }
    }
}