﻿// <auto-generated />
#pragma warning disable CS0618 // 'member' is obsolete: 'text'
#pragma warning disable CS0612 // 'member' is obsolete


namespace MyApplication1
{
    using global::System;
    using global::Grpc.Core;
    using global::MagicOnion;
    using global::MagicOnion.Client;
    using global::MessagePack;
    
    [global::MagicOnion.Ignore]
    public class GreeterServiceClient : global::MagicOnion.Client.MagicOnionClientBase<global::MyApplication1.IGreeterService>, global::MyApplication1.IGreeterService
    {
        class ClientCore
        {
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MagicOnion.DynamicArgumentTuple<global::System.String, global::System.Int32>, global::System.String> HelloAsync;
            public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
            {
                this.HelloAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MagicOnion.DynamicArgumentTuple<global::System.String, global::System.Int32>, global::System.String>(global::Grpc.Core.MethodType.Unary, "IGreeterService", "HelloAsync", serializerProvider);
            }
        }
        
        readonly ClientCore core;
        
        public GreeterServiceClient(global::MagicOnion.Client.MagicOnionClientOptions options, global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider) : base(options)
        {
            this.core = new ClientCore(serializerProvider);
        }
        
        private GreeterServiceClient(MagicOnionClientOptions options, ClientCore core) : base(options)
        {
            this.core = core;
        }
        
        protected override global::MagicOnion.Client.MagicOnionClientBase<IGreeterService> Clone(global::MagicOnion.Client.MagicOnionClientOptions options)
            => new GreeterServiceClient(options, core);
        
        public global::MagicOnion.UnaryResult<global::System.String> HelloAsync(global::System.String name, global::System.Int32 age)
            => this.core.HelloAsync.InvokeUnary(this, "IGreeterService/HelloAsync", new global::MagicOnion.DynamicArgumentTuple<global::System.String, global::System.Int32>(name, age));
    }
}
