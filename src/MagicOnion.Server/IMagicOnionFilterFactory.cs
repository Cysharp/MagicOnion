using System;

namespace MagicOnion.Server
{
    public interface IMagicOnionFilterFactory<T>
    {
        T CreateInstance(IServiceProvider serviceLocator);
        int Order { get; }
    }
}