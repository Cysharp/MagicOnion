﻿// <auto-generated />
#pragma warning disable

namespace TempProject
{
    partial class MagicOnionInitializer
    {
        static partial class MagicOnionGeneratedClient
        {
            [global::MagicOnion.Ignore]
            public class TempProject_MyServiceClient : global::MagicOnion.Client.MagicOnionClientBase<global::TempProject.IMyService>, global::TempProject.IMyService
            {
                class ClientCore
                {
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>> A;
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>> B;
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::System.Int32>>>> C;
                    public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
                    {
                        this.A = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>(global::Grpc.Core.MethodType.Unary, "IMyService", "A", serializerProvider);
                        this.B = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>>(global::Grpc.Core.MethodType.Unary, "IMyService", "B", serializerProvider);
                        this.C = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::System.Int32>>>>(global::Grpc.Core.MethodType.Unary, "IMyService", "C", serializerProvider);
                    }
                 }

                readonly ClientCore core;

                public TempProject_MyServiceClient(global::MagicOnion.Client.MagicOnionClientOptions options, global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider) : base(options)
                {
                    this.core = new ClientCore(serializerProvider);
                }

                private TempProject_MyServiceClient(global::MagicOnion.Client.MagicOnionClientOptions options, ClientCore core) : base(options)
                {
                    this.core = core;
                }

                protected override global::MagicOnion.Client.MagicOnionClientBase<global::TempProject.IMyService> Clone(global::MagicOnion.Client.MagicOnionClientOptions options)
                    => new TempProject_MyServiceClient(options, core);

                public global::MagicOnion.UnaryResult<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>> A()
                    => this.core.A.InvokeUnary(this, "IMyService/A", global::MessagePack.Nil.Default);
                public global::MagicOnion.UnaryResult<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>> B()
                    => this.core.B.InvokeUnary(this, "IMyService/B", global::MessagePack.Nil.Default);
                public global::MagicOnion.UnaryResult<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::System.Int32>>>> C()
                    => this.core.C.InvokeUnary(this, "IMyService/C", global::MessagePack.Nil.Default);
            }
        }
    }
}
