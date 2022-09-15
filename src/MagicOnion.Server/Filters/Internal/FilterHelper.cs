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
    public static IReadOnlyList<MagicOnionServiceFilterDescriptor> GetFilterFactories(IEnumerable<MagicOnionServiceFilterDescriptor> globalFilters, Type classType, MethodInfo methodInfo)
    {
        return globalFilters
            .Concat(classType.GetCustomAttributes(inherit: true).OfType<IMagicOnionFilterFactory<IMagicOnionServiceFilter>>().Select(x => new MagicOnionServiceFilterDescriptor(x, x.Order)))
            .Concat(methodInfo.GetCustomAttributes(inherit: true).OfType<IMagicOnionFilterFactory<IMagicOnionServiceFilter>>().Select(x => new MagicOnionServiceFilterDescriptor(x, x.Order)))
            .OrderBy(x => x.Order)
            .ToArray();
    }

    public static IReadOnlyList<StreamingHubFilterDescriptor> GetFilterFactories(IEnumerable<StreamingHubFilterDescriptor> globalFilters, Type classType, MethodInfo methodInfo)
    {
        return globalFilters
            .Concat(classType.GetCustomAttributes(inherit: true).OfType<IMagicOnionFilterFactory<IStreamingHubFilter>>().Select(x => new StreamingHubFilterDescriptor(x, x.Order)))
            .Concat(methodInfo.GetCustomAttributes(inherit: true).OfType<IMagicOnionFilterFactory<IStreamingHubFilter>>().Select(x => new StreamingHubFilterDescriptor(x, x.Order)))
            .OrderBy(x => x.Order)
            .ToArray();
    }

    public static Func<ServiceContext, ValueTask> WrapMethodBodyWithFilter(IServiceProvider serviceProvider, IEnumerable<IMagicOnionFilterFactory<IMagicOnionServiceFilter>> filters, Func<ServiceContext, ValueTask> methodBody)
    {
        Func<ServiceContext, ValueTask> next = methodBody;

        foreach (var filterFactory in filters.Reverse())
        {
            var newFilter = filterFactory.CreateInstance(serviceProvider);
            next = new InvokeHelper<ServiceContext, Func<ServiceContext, ValueTask>>(newFilter.Invoke, next).GetDelegate();
        }

        return next;
    }

    public static Func<StreamingHubContext, ValueTask> WrapMethodBodyWithFilter(IServiceProvider serviceProvider, IEnumerable<IMagicOnionFilterFactory<IStreamingHubFilter>> filters, Func<StreamingHubContext, ValueTask> methodBody)
    {
        Func<StreamingHubContext, ValueTask> next = methodBody;

        foreach (var filterFactory in filters.Reverse())
        {
            var newFilter = filterFactory.CreateInstance(serviceProvider);
            next = new InvokeHelper<StreamingHubContext, Func<StreamingHubContext, ValueTask>>(newFilter.Invoke, next).GetDelegate();
        }

        return next;
    }
}