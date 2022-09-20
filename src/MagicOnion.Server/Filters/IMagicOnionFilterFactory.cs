using System;

namespace MagicOnion.Server.Filters
{
    /// <summary>
    /// An interface for filter which can create an instance of executable filter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMagicOnionFilterFactory<out T> : IMagicOnionOrderedFilter, IMagicOnionFilterMetadata
        where T : IMagicOnionFilterMetadata
    {
        /// <summary>
        /// Creates an instance of executable filter.
        /// </summary>
        /// <param name="serviceLocator"></param>
        /// <returns></returns>
        T CreateInstance(IServiceProvider serviceLocator);
    }

    public interface IMagicOnionOrderedFilter : IMagicOnionFilterMetadata
    {
        int Order { get; }
    }
}