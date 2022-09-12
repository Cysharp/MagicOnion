using System;

namespace MagicOnion.Server.Filters
{
    public interface IMagicOnionFilterFactory<T> : IMagicOnionOrderedFilter, IMagicOnionFilterMetadata
        where T : IMagicOnionFilterMetadata
    {
        T CreateInstance(IServiceProvider serviceLocator);
    }

    public interface IMagicOnionOrderedFilter : IMagicOnionFilterMetadata
    {
        int Order { get; }
    }
}