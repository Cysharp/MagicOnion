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
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::System.Collections.Generic.List<global::System.String>, global::MessagePack.Nil> GetStringValuesAsync;
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::System.Collections.Generic.List<global::System.Int32>, global::MessagePack.Nil> GetIntValuesAsync;
                    public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
                    {
                        this.GetStringValuesAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_RefType_ValueType<global::System.Collections.Generic.List<global::System.String>, global::MessagePack.Nil>(global::Grpc.Core.MethodType.Unary, "IMyService", "GetStringValuesAsync", serializerProvider);
                        this.GetIntValuesAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_RefType_ValueType<global::System.Collections.Generic.List<global::System.Int32>, global::MessagePack.Nil>(global::Grpc.Core.MethodType.Unary, "IMyService", "GetIntValuesAsync", serializerProvider);
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

                public global::MagicOnion.UnaryResult<global::MessagePack.Nil> GetStringValuesAsync(global::System.Collections.Generic.List<global::System.String> arg0)
                    => this.core.GetStringValuesAsync.InvokeUnary(this, "IMyService/GetStringValuesAsync", arg0);
                public global::MagicOnion.UnaryResult<global::MessagePack.Nil> GetIntValuesAsync(global::System.Collections.Generic.List<global::System.Int32> arg0)
                    => this.core.GetIntValuesAsync.InvokeUnary(this, "IMyService/GetIntValuesAsync", arg0);
            }
        }
    }
}
