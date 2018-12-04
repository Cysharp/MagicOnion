using Grpc.Core;
using MagicOnion.Client;
using MessagePack;
using System;

namespace MagicOnion.Client
{
    public static class MagicOnionClient
    {
        public static T Create<T>(Channel channel)
            where T : IService<T>
        {
            return Create<T>(new DefaultCallInvoker(channel), MessagePackSerializer.DefaultResolver);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultResolver);
        }

        public static T Create<T>(Channel channel, IFormatterResolver resolver)
            where T : IService<T>
        {
            return Create<T>(new DefaultCallInvoker(channel), resolver);
        }

        public static T Create<T>(CallInvoker invoker, IFormatterResolver resolver)
            where T : IService<T>
        {
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));

#if NON_UNITY
            var t = DynamicClientBuilder<T>.ClientType;
            return (T)Activator.CreateInstance(t, invoker, resolver);
#else
            return MagicOnionClientRegistry<T>.Create(invoker, resolver);
#endif
        }
    }

#if !NON_UNITY

    public static class MagicOnionClientRegistry<T>
        where T : IService<T>
    {
        static Func<CallInvoker, IFormatterResolver, T> consturtor;

        public static void Register(Func<CallInvoker, IFormatterResolver, T> ctor)
        {
            consturtor = ctor;
        }

        public static T Create(CallInvoker invoker, IFormatterResolver resolver)
        {
            return consturtor(invoker, resolver);
        }
    }

#endif
}