using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using JwtAuthApp.Shared;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Client.Internal;
using MessagePack;

namespace JwtAuthApp.Client
{
    [Ignore]
    class __IGreeterService_GeneratedClient__ : MagicOnionClientBase<IGreeterService>, IGreeterService
    {
        readonly ClientCore core;

        public __IGreeterService_GeneratedClient__(CallInvoker callInvoker, IReadOnlyList<IClientFilter> clientFilter, MessagePackSerializerOptions serializerOptions)
            : this(new MagicOnionClientOptions(callInvoker, null, default, clientFilter), serializerOptions)
        {
            this.core = new ClientCore(serializerOptions);
        }

        public __IGreeterService_GeneratedClient__(MagicOnionClientOptions options, MessagePackSerializerOptions serializerOptions)
            : base(options)
        {
            this.core = new ClientCore(serializerOptions);
        }
        private __IGreeterService_GeneratedClient__(MagicOnionClientOptions options, ClientCore core)
            : base(options)
        {
            this.core = core;
        }

        protected override MagicOnionClientBase<IGreeterService> Clone(MagicOnionClientOptions options)
            => new __IGreeterService_GeneratedClient__(options, core);

        public UnaryResult<string> HelloAsync() => core.HelloAsync.InvokeUnary(this, "IGreeterService/HelloAsync", Nil.Default);

        public Task<ServerStreamingResult<string>> ServerAsync(string name, int age) => core.ServerAsync.InvokeServerStreaming(this, "IGreeterService/ServerAsync", new DynamicArgumentTuple<string, int>(name, age));

        public Task<ClientStreamingResult<int, string>> ClientAsync() => core.ClientAsync.InvokeClientStreaming(this, "IGreeterService/ClientAsync");

        public Task<DuplexStreamingResult<int, string>> DuplexAsync() => core.DuplexAsync.InvokeDuplexStreaming(this, "IGreeterService/DuplexAsync");

        public static Func<MagicOnionClientOptions, IGreeterService> CreateFactory(MessagePackSerializerOptions serializerOptions)
        {
            var core = new ClientCore(serializerOptions);
            return (options) => new __IGreeterService_GeneratedClient__(options, core);
        }

        class ClientCore
        {
            public readonly RawMethodInvoker<Nil, string> HelloAsync;
            public readonly RawMethodInvoker<DynamicArgumentTuple<string, int>, string> ServerAsync;
            public readonly RawMethodInvoker<int, string> ClientAsync;
            public readonly RawMethodInvoker<int, string> DuplexAsync;

            public ClientCore(MessagePackSerializerOptions serializerOptions)
            {
                HelloAsync = RawMethodInvoker.Create_ValueType_RefType<Nil, string>(MethodType.Unary, nameof(IGreeterService), nameof(HelloAsync), serializerOptions);
                ServerAsync = RawMethodInvoker.Create_ValueType_RefType<DynamicArgumentTuple<string, int>, string>(MethodType.ServerStreaming, nameof(IGreeterService), nameof(ServerAsync), serializerOptions);
                ClientAsync = RawMethodInvoker.Create_ValueType_RefType<int, string>(MethodType.ClientStreaming, nameof(IGreeterService), nameof(ClientAsync), serializerOptions);
                DuplexAsync = RawMethodInvoker.Create_ValueType_RefType<int, string>(MethodType.DuplexStreaming, nameof(IGreeterService), nameof(DuplexAsync), serializerOptions);
            }
        }

    }
}