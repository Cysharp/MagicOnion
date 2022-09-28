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
        where T : IMagicOnionFilterMetadata
    {
        descriptors.Add(new MagicOnionServiceFilterDescriptor(typeof(T)));
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
        where T : IMagicOnionFilterMetadata
    {
        descriptors.Add(new StreamingHubFilterDescriptor(typeof(T)));
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