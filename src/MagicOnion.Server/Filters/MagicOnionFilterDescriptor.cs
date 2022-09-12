using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Filters
{
    public abstract class MagicOnionFilterDescriptor<TFilter> : IMagicOnionFilterFactory<TFilter>
        where TFilter : IMagicOnionFilterMetadata
    {
        public IMagicOnionFilterFactory<TFilter>? Factory { get; }
        public TFilter? Instance { get; }
        public int Order { get; }

        protected MagicOnionFilterDescriptor(Type type, int order = 0)
        {
            Factory = new MagicOnionFilterTypeFactory(type, order);
            Instance = default;
            Order = order;
        }

        protected MagicOnionFilterDescriptor(TFilter instance, int order = 0)
        {
            Factory = null;
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Order = order;
        }

        protected MagicOnionFilterDescriptor(IMagicOnionFilterFactory<TFilter> factory, int order = 0)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Instance = default;
            Order = order;
        }

        public TFilter CreateInstance(IServiceProvider serviceProvider)
        {
            if (Instance != null) return Instance;
            if (Factory != null) return Factory.CreateInstance(serviceProvider);

            throw new InvalidOperationException("MagicOnionFilterDescriptor requires instance or factory");
        }

        // Create a filter instance from specified type.
        class MagicOnionFilterTypeFactory : IMagicOnionFilterFactory<TFilter>
        {
            public Type Type { get; }
            public int Order { get; }

            public MagicOnionFilterTypeFactory(Type type, int order)
            {
                Type = type;
                Order = order;
            }

            public TFilter CreateInstance(IServiceProvider serviceProvider)
            {
                return (TFilter)ActivatorUtilities.CreateInstance(serviceProvider, Type);
            }
        }
    }

    public class MagicOnionServiceFilterDescriptor : MagicOnionFilterDescriptor<IMagicOnionFilter>
    {
        public MagicOnionServiceFilterDescriptor(Type type, int order = 0)
            : base(type, order)
        {
        }

        public MagicOnionServiceFilterDescriptor(IMagicOnionFilter instance, int order = 0)
            : base(instance, order)
        {
        }

        public MagicOnionServiceFilterDescriptor(IMagicOnionFilterFactory<IMagicOnionFilter> factory, int order = 0)
            : base(factory, order)
        {
        }
    }

    public class StreamingHubFilterDescriptor : MagicOnionFilterDescriptor<IStreamingHubFilter>
    {
        public StreamingHubFilterDescriptor(Type type, int order = 0)
            : base(type, order)
        {
        }

        public StreamingHubFilterDescriptor(IStreamingHubFilter instance, int order = 0)
            : base(instance, order)
        {
        }

        public StreamingHubFilterDescriptor(IMagicOnionFilterFactory<IStreamingHubFilter> factory, int order = 0)
            : base(factory, order)
        {
        }
    }
}