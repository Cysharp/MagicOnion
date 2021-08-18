using Grpc.Core;
using MessagePack;
using System;

namespace MagicOnion.Client
{
    public static partial class MagicOnionClient
    {
        static readonly IClientFilter[] emptyFilters = Array.Empty<IClientFilter>();

        public static T Create<T>(ChannelBase channel)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), MessagePackSerializer.DefaultOptions, emptyFilters);
        }

        public static T Create<T>(ChannelBase channel, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), MessagePackSerializer.DefaultOptions, clientFilters);
        }

        public static T Create<T>(ChannelBase channel, MessagePackSerializerOptions serializerOptions)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), serializerOptions, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultOptions, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultOptions, clientFilters);
        }

        public static T Create<T>(CallInvoker invoker, MessagePackSerializerOptions serializerOptions)
            where T : IService<T>
        {
            return Create<T>(invoker, serializerOptions, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker, MessagePackSerializerOptions serializerOptions, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));

            var ctor = MagicOnionClientRegistry<T>.consturtor;
            if (ctor == null)
            {
#if ((ENABLE_IL2CPP && !UNITY_EDITOR) || NET_STANDARD_2_0)
                throw new InvalidOperationException($"Unable to find a client factory of type '{typeof(T)}'. If the application is running on IL2CPP or AOT, dynamic code generation is not supported. Please use the code generator (moc).");
#else
                var t = DynamicClientBuilder<T>.ClientType;
                return (T)Activator.CreateInstance(t, invoker, serializerOptions, clientFilters);
#endif
            }
            else
            {
                return ctor(invoker, serializerOptions, clientFilters);
            }
        }
    }

    public static class MagicOnionClientRegistry<T>
        where T : IService<T>
    {
        public static Func<CallInvoker, MessagePackSerializerOptions, IClientFilter[], T> consturtor;

        public static void Register(Func<CallInvoker, MessagePackSerializerOptions, IClientFilter[], T> ctor)
        {
            consturtor = ctor;
        }
    }
}