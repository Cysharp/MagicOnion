using Grpc.Core;
using System;
using MessagePack;

namespace MagicOnion.Client
{
    public static class MagicOnionClient
    {
        public static T Create<T>(Channel channel)
           where T : IService<T>
        {
            return Create<T>(channel, MessagePackSerializer.DefaultResolver);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultResolver);
        }

        public static T Create<T>(Channel channel, IFormatterResolver resolver)
            where T : IService<T>
        {
#if UNITY_EDITOR
            var invoker = new EditorWindowSupportsCallInvoker(channel);
#else
            var invoker = new DefaultCallInvoker(channel);
#endif

            return Create<T>(invoker, resolver);
        }

        public static T Create<T>(CallInvoker invoker, IFormatterResolver resolver)
            where T : IService<T>
        {
            return MagicOnionClientRegistry<T>.Create(invoker, resolver);
        }
    }

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
}