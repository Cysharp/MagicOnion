using Grpc.Core;
using MessagePack;
using System;

namespace MagicOnion.Client
{
    public static class MagicOnionClient
    {
        static readonly IClientFilter[] emptyFilters = Array.Empty<IClientFilter>();

        public static T Create<T>(Channel channel)
            where T : IService<T>
        {
            return Create<T>(new DefaultCallInvoker(channel), MessagePackSerializer.DefaultResolver, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultResolver, emptyFilters);
        }

        public static T Create<T>(Channel channel, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(new DefaultCallInvoker(channel), MessagePackSerializer.DefaultResolver, clientFilters);
        }

        public static T Create<T>(CallInvoker invoker, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultResolver, clientFilters);
        }

        public static T Create<T>(Channel channel, IFormatterResolver resolver)
            where T : IService<T>
        {
            return Create<T>(new DefaultCallInvoker(channel), resolver, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker, IFormatterResolver resolver)
            where T : IService<T>
        {
            return Create<T>(invoker, resolver, emptyFilters);
        }

        public static T Create<T>(CallInvoker invoker, IFormatterResolver resolver, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));

            var ctor = MagicOnionClientRegistry<T>.consturtor;
            if (ctor == null)
            {
#if ((ENABLE_IL2CPP && !UNITY_EDITOR) || NET_STANDARD_2_0)
                throw new InvalidOperationException("Does not registered client factory, dynamic code generation is not supported on IL2CPP. Please use code generator(moc).");
#else
                var t = DynamicClientBuilder<T>.ClientType;
                return (T)Activator.CreateInstance(t, invoker, resolver, clientFilters);
#endif
            }
            else
            {
                return ctor(invoker, resolver, clientFilters);
            }
        }
    }

    public static class MagicOnionClientRegistry<T>
        where T : IService<T>
    {
        public static Func<CallInvoker, IFormatterResolver, IClientFilter[], T> consturtor;

        public static void Register(Func<CallInvoker, IFormatterResolver, IClientFilter[], T> ctor)
        {
            consturtor = ctor;
        }
    }
}