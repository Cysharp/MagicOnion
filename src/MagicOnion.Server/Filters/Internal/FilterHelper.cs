using System.Diagnostics;
using System.Reflection;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Filters.Internal;

internal class FilterHelper
{
    public static IReadOnlyList<MagicOnionServiceFilterDescriptor> GetFilters(IEnumerable<MagicOnionServiceFilterDescriptor> globalFilters, Type classType, MethodInfo methodInfo)
        => GetFilters(globalFilters, classType.GetCustomAttributes(inherit: true).Concat(methodInfo.GetCustomAttributes(inherit: true)).OfType<Attribute>());

    public static IReadOnlyList<MagicOnionServiceFilterDescriptor> GetFilters(IEnumerable<MagicOnionServiceFilterDescriptor> globalFilters, IEnumerable<Attribute> attributes)
    {
        // Filters are sorted in the following order:
        // [Manually ordered filters] -> [Global Filters] -> [Class Filters] -> [Method Filters]
        // The filters has `int.MaxValue` as order by default. If the user specifies an order, it will take precedence.
        var attributedFilters = attributes
            .OfType<IMagicOnionFilterMetadata>()
            .Where(x => x is IMagicOnionServiceFilter or IMagicOnionFilterFactory<IMagicOnionServiceFilter>)
            .Select(x =>
            {
                var order = (x is IMagicOnionOrderedFilter ordered) ? ordered.Order : int.MaxValue;
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
        => GetFilters(globalFilters, classType.GetCustomAttributes(inherit: true).Concat(methodInfo.GetCustomAttributes(inherit: true)).OfType<Attribute>());

    public static IReadOnlyList<StreamingHubFilterDescriptor> GetFilters(IEnumerable<StreamingHubFilterDescriptor> globalFilters, IEnumerable<Attribute> attributes)
    {
        // Filters are sorted in the following order:
        // [Manually ordered filters] -> [Global Filters] -> [Class Filters] -> [Method Filters]
        // The filters has `int.MaxValue` as order by default. If the user specifies an order, it will take precedence.
        var attributedFilters = attributes
            .OfType<IMagicOnionFilterMetadata>()
            .Where(x => x is IStreamingHubFilter or IMagicOnionFilterFactory<IStreamingHubFilter>)
            .Select(x =>
            {
                var order = (x is IMagicOnionOrderedFilter ordered) ? ordered.Order : int.MaxValue;
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
        Func<ServiceContext, ValueTask> outer = methodBody;

        foreach (var filterDescriptor in filters.Reverse())
        {
            var newFilter = CreateOrGetInstance(serviceProvider, filterDescriptor);
            var inner = outer;
            outer = [StackTraceHidden] (ctx) => newFilter.Invoke(ctx, inner);
        }

        return outer;
    }

    public static Func<StreamingHubContext, ValueTask> WrapMethodBodyWithFilter(IServiceProvider serviceProvider, IEnumerable<StreamingHubFilterDescriptor> filters, Func<StreamingHubContext, ValueTask> methodBody)
    {
        Func<StreamingHubContext, ValueTask> outer = methodBody;

        foreach (var filterDescriptor in filters.Reverse())
        {
            var newFilter = CreateOrGetInstance(serviceProvider, filterDescriptor);
            var inner = outer;
            outer = [StackTraceHidden] (ctx) => newFilter.Invoke(ctx, inner);
        }

        return outer;
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
