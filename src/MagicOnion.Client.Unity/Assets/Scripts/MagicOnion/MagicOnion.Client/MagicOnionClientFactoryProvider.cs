using MagicOnion.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
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
            = DynamicClient.DynamicMagicOnionClientFactoryProvider.Instance;
#else
            =  DynamicClient.DynamicNotSupportedMagicOnionClientFactoryProvider.Instance;
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
        bool TryGetFactory<T>([NotNullWhen(true)] out MagicOnionClientFactoryDelegate<T>? factory) where T : IService<T>;
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

        public bool TryGetFactory<T>([NotNullWhen(true)] out MagicOnionClientFactoryDelegate<T>? factory) where T : IService<T>
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
}
