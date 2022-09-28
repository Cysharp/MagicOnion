using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Filters;

/// <summary>
/// A descriptor of MagicOnion filter.
/// </summary>
/// <typeparam name="TFilter"></typeparam>
public abstract class MagicOnionFilterDescriptor<TFilter>
    where TFilter : IMagicOnionFilterMetadata
{
    public IMagicOnionFilterMetadata Filter { get; }
    public int Order { get; }

    protected MagicOnionFilterDescriptor(Type type, int? order = default)
    {
        if (typeof(TFilter).IsAssignableFrom(type))
        {
            Filter = new MagicOnionFilterFromTypeFactory(type);
        }
        else if (typeof(IMagicOnionFilterFactory<TFilter>).IsAssignableFrom(type))
        {
            Filter = new MagicOnionFilterFromTypeFactoryFactory(type);
        }
        else
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is not compatible with {typeof(TFilter).Name} or {typeof(IMagicOnionFilterFactory<TFilter>).Name}");
        }

        Order = order ?? int.MaxValue;
    }

    protected MagicOnionFilterDescriptor(TFilter instance, int? order = default)
    {
        Filter = instance ?? throw new ArgumentNullException(nameof(instance));
        Order = GetOrder(Filter, order);
    }

    protected MagicOnionFilterDescriptor(IMagicOnionFilterFactory<TFilter> factory, int? order = default)
    {
        Filter = factory ?? throw new ArgumentNullException(nameof(factory));
        Order = GetOrder(Filter, order);
    }

    static int GetOrder(IMagicOnionFilterMetadata filter, int? order)
        => order ?? (filter is IMagicOnionOrderedFilter ordered ? ordered.Order : int.MaxValue);

    // Create a filter instance from specified type.
    internal class MagicOnionFilterFromTypeFactory : IMagicOnionFilterFactory<TFilter>
    {
        public Type Type { get; }

        public MagicOnionFilterFromTypeFactory(Type type)
        {
            Type = type;
        }

        public TFilter CreateInstance(IServiceProvider serviceProvider)
            => (TFilter)ActivatorUtilities.CreateInstance(serviceProvider, Type);
    }

    internal class MagicOnionFilterFromTypeFactoryFactory : IMagicOnionFilterFactory<TFilter>
    {
        public Type Type { get; }

        public MagicOnionFilterFromTypeFactoryFactory(Type type)
        {
            Type = type;
        }

        public TFilter CreateInstance(IServiceProvider serviceProvider)
            => ((IMagicOnionFilterFactory<TFilter>)ActivatorUtilities.CreateInstance(serviceProvider, Type)).CreateInstance(serviceProvider);
    }
}

public class MagicOnionServiceFilterDescriptor : MagicOnionFilterDescriptor<IMagicOnionServiceFilter>
{
    public MagicOnionServiceFilterDescriptor(Type type, int? order = default)
        : base(type, order)
    {
    }

    public MagicOnionServiceFilterDescriptor(IMagicOnionServiceFilter instance, int? order = default)
        : base(instance, order)
    {
    }

    public MagicOnionServiceFilterDescriptor(IMagicOnionFilterFactory<IMagicOnionServiceFilter> factory, int? order = default)
        : base(factory, order)
    {
    }
}

public class StreamingHubFilterDescriptor : MagicOnionFilterDescriptor<IStreamingHubFilter>
{
    public StreamingHubFilterDescriptor(Type type, int? order = default)
        : base(type, order)
    {
    }

    public StreamingHubFilterDescriptor(IStreamingHubFilter instance, int? order = default)
        : base(instance, order)
    {
    }

    public StreamingHubFilterDescriptor(IMagicOnionFilterFactory<IStreamingHubFilter> factory, int? order = default)
        : base(factory, order)
    {
    }
}
