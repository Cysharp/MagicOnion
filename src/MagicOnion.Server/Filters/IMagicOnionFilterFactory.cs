using System;

namespace MagicOnion.Server.Filters;

/// <summary>
/// An interface for filter which can create an instance of executable filter.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMagicOnionFilterFactory<out T> : IMagicOnionFilterMetadata
    where T : IMagicOnionFilterMetadata
{
    /// <summary>
    /// Creates an instance of executable filter.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    T CreateInstance(IServiceProvider serviceProvider);
}
