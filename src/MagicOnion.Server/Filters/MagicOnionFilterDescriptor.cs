using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Filters
{
    /// <summary>
    /// A descriptor of MagicOnion filter.
    /// </summary>
    /// <typeparam name="TFilter"></typeparam>
    public abstract class MagicOnionFilterDescriptor<TFilter>
        where TFilter : IMagicOnionFilterMetadata
    {
        public IMagicOnionFilterMetadata Filter { get; }
        public int Order { get; }

        protected MagicOnionFilterDescriptor(Type type, int order = 0)
        {
            Filter = new MagicOnionFilterFromTypeFactory(type, order);
            Order = order;
        }

        protected MagicOnionFilterDescriptor(TFilter instance, int order = 0)
        {
            Filter = instance ?? throw new ArgumentNullException(nameof(instance));
            Order = order;
        }

        protected MagicOnionFilterDescriptor(IMagicOnionFilterFactory<TFilter> factory, int order = 0)
        {
            Filter = factory ?? throw new ArgumentNullException(nameof(factory));
            Order = order;
        }

        // Create a filter instance from specified type.
        internal class MagicOnionFilterFromTypeFactory : IMagicOnionFilterFactory<TFilter>
        {
            public Type Type { get; }
            public int Order { get; }

            public MagicOnionFilterFromTypeFactory(Type type, int order)
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

    public class MagicOnionServiceFilterDescriptor : MagicOnionFilterDescriptor<IMagicOnionServiceFilter>
    {
        public MagicOnionServiceFilterDescriptor(Type type, int order = 0)
            : base(type, order)
        {
        }

        public MagicOnionServiceFilterDescriptor(IMagicOnionServiceFilter instance, int order = 0)
            : base(instance, order)
        {
        }

        public MagicOnionServiceFilterDescriptor(IMagicOnionFilterFactory<IMagicOnionServiceFilter> factory, int order = 0)
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