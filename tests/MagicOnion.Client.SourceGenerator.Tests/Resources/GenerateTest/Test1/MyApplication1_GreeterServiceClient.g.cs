﻿// <auto-generated />
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

// NOTE: Disable warnings for nullable reference types.
// `#nullable disable` causes compile error on old C# compilers (-7.3)
#pragma warning disable 8603 // Possible null reference return.
#pragma warning disable 8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
#pragma warning disable 8625 // Cannot convert null literal to non-nullable reference type.

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
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::MessagePack.Nil> PingAsync;
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::System.Boolean> CanGreetAsync;
            public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
            {
                this.HelloAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MagicOnion.DynamicArgumentTuple<global::System.String, global::System.Int32>, global::System.String>(global::Grpc.Core.MethodType.Unary, "IGreeterService", "HelloAsync", serializerProvider);
                this.PingAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_ValueType<global::MessagePack.Nil, global::MessagePack.Nil>(global::Grpc.Core.MethodType.Unary, "IGreeterService", "PingAsync", serializerProvider);
                this.CanGreetAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_ValueType<global::MessagePack.Nil, global::System.Boolean>(global::Grpc.Core.MethodType.Unary, "IGreeterService", "CanGreetAsync", serializerProvider);
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
        public global::MagicOnion.UnaryResult PingAsync()
            => this.core.PingAsync.InvokeUnaryNonGeneric(this, "IGreeterService/PingAsync", global::MessagePack.Nil.Default);
        public global::MagicOnion.UnaryResult<global::System.Boolean> CanGreetAsync()
            => this.core.CanGreetAsync.InvokeUnary(this, "IGreeterService/CanGreetAsync", global::MessagePack.Nil.Default);
    }
}

