using Grpc.Core;
using MagicOnion.Client;
using System;
using ZeroFormatter.Formatters;

namespace MagicOnion.Client
{
    public static class MagicOnionClient
    {
        public static T Create<T>(Channel channel)
            where T : IService<T>
        {
            return Create<DefaultResolver, T>(channel);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return Create<DefaultResolver, T>(invoker);
        }

        public static T Create<TTypeResolver, T>(Channel channel)
            where TTypeResolver : ITypeResolver, new()
            where T : IService<T>
        {
            var t = DynamicClientBuilder<TTypeResolver, T>.ClientType;
            return (T)Activator.CreateInstance(t, channel);
        }

        public static T Create<TTypeResolver, T>(CallInvoker invoker)
            where TTypeResolver : ITypeResolver, new()
            where T : IService<T>
        {
            var t = DynamicClientBuilder<TTypeResolver, T>.ClientType;
            return (T)Activator.CreateInstance(t, invoker);
        }
    }
}