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

        public UnaryResult<string> HelloAsync() => core.HelloAsync.Invoke(this, "IGreeterService/HelloAsync", Nil.Default);

        public Task<ServerStreamingResult<string>> ServerAsync(string name, int age) => core.ServerAsync.Invoke(this, "IGreeterService/ServerAsync", new DynamicArgumentTuple<string, int>(name, age));

        public Task<ClientStreamingResult<int, string>> ClientAsync() => core.ClientAsync.Invoke(this, "IGreeterService/ClientAsync");

        public Task<DuplexStreamingResult<int, string>> DuplexAsync() => core.DuplexAsync.Invoke(this, "IGreeterService/DuplexAsync");

        public static Func<MagicOnionClientOptions, MagicOnionClientBase<IGreeterService>> CreateFactory(MessagePackSerializerOptions serializerOptions)
        {
            var core = new ClientCore(serializerOptions);
            return (options) => new __IGreeterService_GeneratedClient__(options, core);
        }

        class ClientCore
        {
            public readonly UnaryMethodRawInvoker<Nil, string> HelloAsync;
            public readonly ServerStreamingMethodRawInvoker<DynamicArgumentTuple<string, int>, string> ServerAsync;
            public readonly ClientStreamingMethodRawInvoker<int, string> ClientAsync;
            public readonly DuplexStreamingMethodRawInvoker<int, string> DuplexAsync;

            public ClientCore(MessagePackSerializerOptions serializerOptions)
            {
                HelloAsync = UnaryMethodRawInvoker.Create_ValueType_RefType<Nil, string>(nameof(IGreeterService), nameof(HelloAsync), serializerOptions);
                ServerAsync = ServerStreamingMethodRawInvoker.Create_ValueType_RefType<DynamicArgumentTuple<string, int>, string>(nameof(IGreeterService), nameof(ServerAsync), serializerOptions);
                ClientAsync = ClientStreamingMethodRawInvoker.Create_ValueType_RefType<int, string>(nameof(IGreeterService), nameof(ClientAsync), serializerOptions);
                DuplexAsync = DuplexStreamingMethodRawInvoker.Create_ValueType_RefType<int, string>(nameof(IGreeterService), nameof(DuplexAsync), serializerOptions);
            }
        }

    }
}