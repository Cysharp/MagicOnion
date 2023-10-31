using System;
using System.Diagnostics.CodeAnalysis;

namespace MagicOnion.Client.DynamicClient
{
    public class DynamicNotSupportedMagicOnionClientFactoryProvider : IMagicOnionClientFactoryProvider
    {
        public static IMagicOnionClientFactoryProvider Instance { get; } = new DynamicNotSupportedMagicOnionClientFactoryProvider();

        DynamicNotSupportedMagicOnionClientFactoryProvider() { }

        public bool TryGetFactory<T>([NotNullWhen(true)] out MagicOnionClientFactoryDelegate<T>? factory) where T : IService<T>
        {
            throw new InvalidOperationException($"Unable to find a client factory of type '{typeof(T)}'. If the application is running on IL2CPP or AOT, dynamic code generation is not supported. Please use the code generator (moc).");
        }
    }

#if ((!ENABLE_IL2CPP || UNITY_EDITOR) && !NET_STANDARD_2_0)
    /// <summary>
    /// Provides to get a MagicOnionClient factory of the specified service type. The provider is backed by DynamicMagicOnionClientBuilder.
    /// </summary>
    public class DynamicMagicOnionClientFactoryProvider : IMagicOnionClientFactoryProvider
    {
        public static IMagicOnionClientFactoryProvider Instance { get; } = new DynamicMagicOnionClientFactoryProvider();

        DynamicMagicOnionClientFactoryProvider() { }

        public bool TryGetFactory<T>([NotNullWhen(true)] out MagicOnionClientFactoryDelegate<T>? factory) where T : IService<T>
        {
            factory = Cache<T>.Factory;
            return true;
        }

        static class Cache<T> where T : IService<T>
        {
            public static readonly MagicOnionClientFactoryDelegate<T> Factory
                = (clientOptions, serializerProvider) => (T)Activator.CreateInstance(DynamicClientBuilder<T>.ClientType, clientOptions, serializerProvider)!;
        }
    }
#endif
}
