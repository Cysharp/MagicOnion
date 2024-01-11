﻿// <auto-generated />
#pragma warning disable CS0618 // 'member' is obsolete: 'text'
#pragma warning disable CS0612 // 'member' is obsolete
#pragma warning disable CS8019 // Unnecessary using directive.

namespace MyApplication1
{
    partial class MagicOnionInitializer
    {
        static partial class MagicOnionGeneratedClient
        {
            [global::MagicOnion.Ignore]
            public class MyApplication1_GreeterServiceClient : global::MagicOnion.Client.MagicOnionClientBase<global::MyApplication1.IGreeterService>, global::MyApplication1.IGreeterService
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

                public MyApplication1_GreeterServiceClient(global::MagicOnion.Client.MagicOnionClientOptions options, global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider) : base(options)
                {
                    this.core = new ClientCore(serializerProvider);
                }

                private MyApplication1_GreeterServiceClient(global::MagicOnion.Client.MagicOnionClientOptions options, ClientCore core) : base(options)
                {
                    this.core = core;
                }

                protected override global::MagicOnion.Client.MagicOnionClientBase<global::MyApplication1.IGreeterService> Clone(global::MagicOnion.Client.MagicOnionClientOptions options)
                    => new MyApplication1_GreeterServiceClient(options, core);

                public global::MagicOnion.UnaryResult<global::System.String> HelloAsync(global::System.String name, global::System.Int32 age)
                    => this.core.HelloAsync.InvokeUnary(this, "IGreeterService/HelloAsync", new global::MagicOnion.DynamicArgumentTuple<global::System.String, global::System.Int32>(name, age));
                public global::MagicOnion.UnaryResult PingAsync()
                    => this.core.PingAsync.InvokeUnaryNonGeneric(this, "IGreeterService/PingAsync", global::MessagePack.Nil.Default);
                public global::MagicOnion.UnaryResult<global::System.Boolean> CanGreetAsync()
                    => this.core.CanGreetAsync.InvokeUnary(this, "IGreeterService/CanGreetAsync", global::MessagePack.Nil.Default);
            }
        }
    }
}