using Grpc.Core;
using System;
using ZeroFormatter.Formatters;

namespace MagicOnion.Client
{
    public static class MagicOnionClient
    {
        public static T Create<T>(Channel channel)
            where T : IService<T>
        {
            return MagicOnionClientRegistry<T>.Create(channel);
        }

        public static T Create<T>(CallInvoker invoker)
            where T : IService<T>
        {
            return MagicOnionClientRegistry<T>.Create(invoker);
        }
    }

    public static class MagicOnionClientRegistry<T>
        where T : IService<T>
    {
        static Func<Channel, T> consturtor1;
        static Func<CallInvoker, T> consturtor2;

        public static void Register(Func<Channel, T> ctor1, Func<CallInvoker, T> ctor2)
        {
            consturtor1 = ctor1;
            consturtor2 = ctor2;
        }

        public static T Create(Channel channel)
        {
            return consturtor1(channel);
        }

        public static T Create(CallInvoker invoker)
        {
            return consturtor2(invoker);
        }
    }
}