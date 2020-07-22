#if NON_UNITY
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Net.Client;
using MessagePack;

namespace MagicOnion.Client
{
    public static partial class MagicOnionClient
    {
        public static T Create<T>(GrpcChannel channel)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), MessagePackSerializer.DefaultOptions, emptyFilters);
        }

        public static T Create<T>(GrpcChannel channel, IClientFilter[] clientFilters)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), MessagePackSerializer.DefaultOptions, clientFilters);
        }

        public static T Create<T>(GrpcChannel channel, MessagePackSerializerOptions serializerOptions)
            where T : IService<T>
        {
            return Create<T>(channel.CreateCallInvoker(), serializerOptions, emptyFilters);
        }
    }
}
#endif