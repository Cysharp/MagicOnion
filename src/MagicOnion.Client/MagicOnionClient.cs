using Grpc.Core;
using MagicOnion.Serialization;
using System;

namespace MagicOnion.Client
{
    public static partial class MagicOnionClient
    {
        static readonly IClientFilter[] emptyFilters = Array.Empty<IClientFilter>();

        public static T Create<T>(ChannelBase channel)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), MagicOnionSerializerProvider.Default, emptyFilters);
        }

        public static T Create<T>(ChannelBase channel, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), MagicOnionSerializerProvider.Default, clientFilters);
        }

        public static T Create<T>(ChannelBase channel, IMagicOnionSerializerProvider serializerProvider)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), serializerProvider, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<T>(invoker, MagicOnionSerializerProvider.Default, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(invoker, MagicOnionSerializerProvider.Default, clientFilters);
        }

        public static T Create<T>(CallInvoker invoker, IMagicOnionSerializerProvider serializerProvider)
            where T : IService<T>
        {
            return Create<T>(invoker, serializerProvider, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker, IMagicOnionSerializerProvider serializerProvider, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));

            var clientOptions = new MagicOnionClientOptions(invoker, default, default, clientFilters);
            return Create<T>(clientOptions, serializerProvider);
        }

        public static T Create<T>(MagicOnionClientOptions clientOptions, IMagicOnionSerializerProvider serializerProvider)
            where T : IService<T>
        {
            var ctor = MagicOnionClientRegistry<T>.constructor;
            if (ctor == null)
            {
#if ((ENABLE_IL2CPP && !UNITY_EDITOR) || NET_STANDARD_2_0)
                throw new InvalidOperationException($"Unable to find a client factory of type '{typeof(T)}'. If the application is running on IL2CPP or AOT, dynamic code generation is not supported. Please use the code generator (moc).");
#else
                var t = MagicOnion.Client.DynamicClient.DynamicClientBuilder<T>.ClientType;
                return (T)Activator.CreateInstance(t, clientOptions, serializerProvider);
#endif
            }
            else
            {
                return ctor(clientOptions, serializerProvider);
            }
        }
    }

    public static class MagicOnionClientRegistry<T>
        where T : IService<T>
    {
        internal static Func<MagicOnionClientOptions, IMagicOnionSerializerProvider, T> constructor;

        public static void Register(Func<MagicOnionClientOptions, IMagicOnionSerializerProvider, T> ctor)
        {
            constructor = ctor;
        }
    }
}
