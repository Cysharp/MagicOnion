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
            return Create<T>(channel, MessagePackSerializer.DefaultResolver);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<T>(invoker, MessagePackSerializer.DefaultResolver);
        }

        // TODO:resolver!!!
        public static T Create<T>(Channel channel, IFormatterResolver resolver)
            where T : IService<T>
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            var t = DynamicClientBuilder<T>.ClientType;
            return (T)Activator.CreateInstance(t, channel);
        }

        public static T Create<T>(CallInvoker invoker, IFormatterResolver resolver)
            where T : IService<T>
        {
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));
            var t = DynamicClientBuilder<T>.ClientType;
            return (T)Activator.CreateInstance(t, invoker);
        }
    }
}