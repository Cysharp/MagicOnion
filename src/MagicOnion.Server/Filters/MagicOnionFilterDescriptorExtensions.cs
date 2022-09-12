using System;
using System.Collections.Generic;
using System.Linq;
using MagicOnion.Server.Filters;

// ReSharper disable once CheckNamespace
namespace MagicOnion.Server;

public static class MagicOnionFilterDescriptorExtensions
{
    /// <summary>
    /// Adds the MagicOnion filter as type.
    /// </summary>
    /// <param name="descriptors"></param>
    public static void Add<T>(this IList<MagicOnionServiceFilterDescriptor> descriptors)
    {
        if (typeof(IMagicOnionFilterFactory<IMagicOnionServiceFilter>).IsAssignableFrom(typeof(T)))
        {
            var ctor = typeof(T).GetConstructors().SingleOrDefault(x => x.GetParameters().Length == 0);
            if (ctor == null)
            {
                throw new InvalidOperationException($"Type '{typeof(T).FullName}' has no parameter-less constructor. You can also use `Add(instance)` overload method.");
            }
            descriptors.Add(new MagicOnionServiceFilterDescriptor((IMagicOnionFilterFactory<IMagicOnionServiceFilter>)Activator.CreateInstance<T>()!));
        }
        else if (typeof(IMagicOnionServiceFilter).IsAssignableFrom(typeof(T)))
        {
            descriptors.Add(new MagicOnionServiceFilterDescriptor(typeof(T)));
        }
        else
        {
            throw new InvalidOperationException($"Type '{typeof(T).FullName}' is not compatible with {nameof(IMagicOnionServiceFilter)} or {nameof(IMagicOnionFilterFactory<IMagicOnionServiceFilter>)}");
        }
    }

    /// <summary>
    /// Adds the MagicOnion filter instance as singleton.
    /// </summary>
    /// <param name="descriptors"></param>
    /// <param name="filterInstance"></param>
    public static void Add(this IList<MagicOnionServiceFilterDescriptor> descriptors, IMagicOnionServiceFilter filterInstance)
    {
        if (filterInstance == null) throw new ArgumentNullException(nameof(filterInstance));

        descriptors.Add(new MagicOnionServiceFilterDescriptor(filterInstance));
    }

    /// <summary>
    /// Adds the MagicOnion filter as type.
    /// </summary>
    /// <param name="descriptors"></param>
    /// <param name="factory"></param>
    public static void Add(this IList<MagicOnionServiceFilterDescriptor> descriptors, IMagicOnionFilterFactory<IMagicOnionServiceFilter> factory)
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
        if (typeof(IMagicOnionFilterFactory<IStreamingHubFilter>).IsAssignableFrom(typeof(T)))
        {
            var ctor = typeof(T).GetConstructors().SingleOrDefault(x => x.GetParameters().Length == 0);
            if (ctor == null)
            {
                throw new InvalidOperationException($"Type '{typeof(T).FullName}' has no parameter-less constructor. You can also use `Add(instance)` overload method.");
            }
            descriptors.Add(new StreamingHubFilterDescriptor((IMagicOnionFilterFactory<IStreamingHubFilter>)Activator.CreateInstance<T>()!));
        }
        else if (typeof(IStreamingHubFilter).IsAssignableFrom(typeof(T)))
        {
            descriptors.Add(new StreamingHubFilterDescriptor(typeof(T)));
        }
        else
        {
            throw new InvalidOperationException($"Type '{typeof(T).FullName}' is not compatible with {nameof(IStreamingHubFilter)} or {nameof(IMagicOnionFilterFactory<IStreamingHubFilter>)}");
        }
    }

    /// <summary>
    /// Adds the MagicOnion StreamingHub filter instance as singleton.
    /// </summary>
    /// <param name="descriptors"></param>
    /// <param name="filterInstance"></param>
    public static void Add(this IList<StreamingHubFilterDescriptor> descriptors, IStreamingHubFilter filterInstance)
    {
        if (filterInstance == null) throw new ArgumentNullException(nameof(filterInstance));

        descriptors.Add(new StreamingHubFilterDescriptor(filterInstance));
    }

    /// <summary>
    /// Adds the MagicOnion StreamingHub filter instance as singleton.
    /// </summary>
    /// <param name="descriptors"></param>
    /// <param name="factory"></param>
    public static void Add(this IList<StreamingHubFilterDescriptor> descriptors, IMagicOnionFilterFactory<IStreamingHubFilter> factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        descriptors.Add(new StreamingHubFilterDescriptor(factory));
    }
}