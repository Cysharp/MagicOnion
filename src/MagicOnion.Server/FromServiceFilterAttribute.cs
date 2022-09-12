using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MagicOnion.Server.Filters;

namespace MagicOnion.Server
{
    /// <summary>
    /// A MagicOnion filter that provided another filter via <see cref="IServiceProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FromServiceFilterAttribute : Attribute,
        IMagicOnionFilterFactory<IMagicOnionFilter>,
        IMagicOnionFilterFactory<IStreamingHubFilter>
    {
        public Type Type { get; }

        public int Order { get; set; }

        public FromServiceFilterAttribute(Type type)
        {
            if (!typeof(IMagicOnionFilter).IsAssignableFrom(type) &&
                !typeof(IStreamingHubFilter).IsAssignableFrom(type))
            {
                throw new ArgumentException($"{type.FullName} doesn't implement {nameof(IMagicOnionFilter)} or {nameof(IStreamingHubFilter)}.", nameof(type));
            }

            Type = type;
        }

        IMagicOnionFilter IMagicOnionFilterFactory<IMagicOnionFilter>.CreateInstance(IServiceProvider serviceProvider)
        {
            if (!typeof(IMagicOnionFilter).IsAssignableFrom(Type)) return ThroughFilter.Instance;
            return (IMagicOnionFilter)ActivatorUtilities.CreateInstance(serviceProvider, Type);
        }

        IStreamingHubFilter IMagicOnionFilterFactory<IStreamingHubFilter>.CreateInstance(IServiceProvider serviceProvider)
        {
            if (!typeof(IStreamingHubFilter).IsAssignableFrom(Type)) return ThroughFilter.Instance;
            return (IStreamingHubFilter)ActivatorUtilities.CreateInstance(serviceProvider, Type);
        }

        class ThroughFilter : IMagicOnionFilter, IStreamingHubFilter
        {
            public static ThroughFilter Instance { get; } = new ThroughFilter();

            private ThroughFilter() {}

            public ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
                => next(context);

            public ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
                => next(context);
        }
    }
}
