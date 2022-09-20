using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Filters.Internal;

internal class FilterHelper
{
    public static IReadOnlyList<MagicOnionServiceFilterDescriptor> GetFilters(IEnumerable<MagicOnionServiceFilterDescriptor> globalFilters, Type classType, MethodInfo methodInfo)
    {
        var attributedFilters = classType.GetCustomAttributes(inherit: true).Concat(methodInfo.GetCustomAttributes(inherit: true))
            .OfType<IMagicOnionFilterMetadata>()
            .Where(x => x is IMagicOnionServiceFilter or IMagicOnionFilterFactory<IMagicOnionServiceFilter>)
            .Select(x =>
            {
                var order = (x is IMagicOnionOrderedFilter ordered) ? ordered.Order : 0;
                return x switch
                {
                    IMagicOnionServiceFilter filter
                        => new MagicOnionServiceFilterDescriptor(filter, order),
                    IMagicOnionFilterFactory<IMagicOnionServiceFilter> filterFactory
                        => new MagicOnionServiceFilterDescriptor(filterFactory, order),
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                    // _ => throw new InvalidOperationException($"The '{x.GetType().FullName}' attribute must implement IMagicOnionFilterFactory<IMagicOnionServiceFilter> or IMagicOnionServiceFilter"),
                };
            });

        return globalFilters
            .Concat(attributedFilters)
            .OrderBy(x => x.Order)
            .ToArray();
    }

    public static IReadOnlyList<StreamingHubFilterDescriptor> GetFilters(IEnumerable<StreamingHubFilterDescriptor> globalFilters, Type classType, MethodInfo methodInfo)
    {
        var attributedFilters = classType.GetCustomAttributes(inherit: true).Concat(methodInfo.GetCustomAttributes(inherit: true))
            .OfType<IMagicOnionFilterMetadata>()
            .Where(x => x is IStreamingHubFilter or IMagicOnionFilterFactory<IStreamingHubFilter>)
            .Select(x =>
            {
                var order = (x is IMagicOnionOrderedFilter ordered) ? ordered.Order : 0;
                return x switch
                {
                    IStreamingHubFilter filter
                        => new StreamingHubFilterDescriptor(filter, order),
                    IMagicOnionFilterFactory<IStreamingHubFilter> filterFactory
                        => new StreamingHubFilterDescriptor(filterFactory, order),
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                    // _ => throw new InvalidOperationException($"The '{x.GetType().FullName}' attribute must implement IMagicOnionFilterFactory<IMagicOnionServiceFilter> or IMagicOnionServiceFilter"),
                };
            });

        return globalFilters
            .Concat(attributedFilters)
            .OrderBy(x => x.Order)
            .ToArray();
    }

    public static Func<ServiceContext, ValueTask> WrapMethodBodyWithFilter(IServiceProvider serviceProvider, IEnumerable<MagicOnionServiceFilterDescriptor> filters, Func<ServiceContext, ValueTask> methodBody)
    {
        Func<ServiceContext, ValueTask> next = methodBody;

        foreach (var filterDescriptor in filters.Reverse())
        {
            var newFilter = CreateOrGetInstance(serviceProvider, filterDescriptor);
            next = new InvokeHelper<ServiceContext, Func<ServiceContext, ValueTask>>(newFilter.Invoke, next).GetDelegate();
        }

        return next;
    }

    public static Func<StreamingHubContext, ValueTask> WrapMethodBodyWithFilter(IServiceProvider serviceProvider, IEnumerable<StreamingHubFilterDescriptor> filters, Func<StreamingHubContext, ValueTask> methodBody)
    {
        Func<StreamingHubContext, ValueTask> next = methodBody;

        foreach (var filterDescriptor in filters.Reverse())
        {
            var newFilter = CreateOrGetInstance(serviceProvider, filterDescriptor);
            next = new InvokeHelper<StreamingHubContext, Func<StreamingHubContext, ValueTask>>(newFilter.Invoke, next).GetDelegate();
        }

        return next;
    }

    public static TFilter CreateOrGetInstance<TFilter>(IServiceProvider serviceProvider, MagicOnionFilterDescriptor<TFilter> descriptor)
        where TFilter : IMagicOnionFilterMetadata
    {
        switch (descriptor.Filter)
        {
            case IMagicOnionFilterFactory<TFilter> factory:
                return factory.CreateInstance(serviceProvider);
            case TFilter filterInstance:
                return filterInstance;
            default:
                throw new InvalidOperationException($"MagicOnionFilterDescriptor requires instance or factory. but the descriptor has '{descriptor.Filter.GetType()}'");
        }
    }
}