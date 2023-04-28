using MagicOnion.Serialization;
using System;
using System.Linq;

namespace MagicOnion.Client
{
    /// <summary>
    /// Provides to get a MagicOnionClient factory of the specified service type.
    /// </summary>
    public static class MagicOnionClientFactoryProvider
    {
        /// <summary>
        /// Gets or set the MagicOnionClient factory provider to use by default.
        /// </summary>
        public static IMagicOnionClientFactoryProvider Default { get; set; }
#if ((!ENABLE_IL2CPP || UNITY_EDITOR) && !NET_STANDARD_2_0)
            = DynamicMagicOnionClientFactoryProvider.Instance;
#else
            = DynamicNotSupportedMagicOnionClientFactoryProvider.Instance;
#endif
    }

    public delegate T MagicOnionClientFactoryDelegate<out T>(MagicOnionClientOptions clientOptions, IMagicOnionSerializerProvider serializerProvider) where T : IService<T>;

    /// <summary>
    /// Provides to get a MagicOnionClient factory of the specified service type.
    /// </summary>
    public interface IMagicOnionClientFactoryProvider
    {
        /// <summary>
        /// Gets the MagicOnionClient factory of the specified service type. A return value indicates whether a factory was found or not.
        /// </summary>
        /// <typeparam name="T">A MagicOnion service interface type.</typeparam>
        /// <param name="factory">A MagicOnionClient factory of specified service type.</param>
        /// <returns>The value indicates whether a factory was found or not.</returns>
        bool TryGetFactory<T>(out MagicOnionClientFactoryDelegate<T> factory) where T : IService<T>;
    }
    
    public class ImmutableMagicOnionClientFactoryProvider : IMagicOnionClientFactoryProvider
    {
        readonly IMagicOnionClientFactoryProvider[] providers;

        public ImmutableMagicOnionClientFactoryProvider(params IMagicOnionClientFactoryProvider[] providers)
        {
            this.providers = providers;
        }

        public ImmutableMagicOnionClientFactoryProvider Add(IMagicOnionClientFactoryProvider provider)
        {
            return new ImmutableMagicOnionClientFactoryProvider(providers.Append(provider).ToArray());
        }

        public bool TryGetFactory<T>(out MagicOnionClientFactoryDelegate<T> factory) where T : IService<T>
        {
            foreach (var provider in providers)
            {
                if (provider.TryGetFactory<T>(out factory))
                {
                    return true;
                }
            }

            factory = default;
            return false;
        }
    }

    public class DynamicNotSupportedMagicOnionClientFactoryProvider : IMagicOnionClientFactoryProvider
    {
        public static IMagicOnionClientFactoryProvider Instance { get; } = new DynamicNotSupportedMagicOnionClientFactoryProvider();

        DynamicNotSupportedMagicOnionClientFactoryProvider() { }

        public bool TryGetFactory<T>(out MagicOnionClientFactoryDelegate<T> factory) where T : IService<T>
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

        public bool TryGetFactory<T>(out MagicOnionClientFactoryDelegate<T> factory) where T : IService<T>
        {
            factory = Cache<T>.Factory;
            return true;
        }

        static class Cache<T> where T : IService<T>
        {
            public static readonly MagicOnionClientFactoryDelegate<T> Factory
                = (clientOptions, serializerProvider) => (T)Activator.CreateInstance(DynamicClient.DynamicClientBuilder<T>.ClientType, clientOptions, serializerProvider);
        }
    }
#endif
}
