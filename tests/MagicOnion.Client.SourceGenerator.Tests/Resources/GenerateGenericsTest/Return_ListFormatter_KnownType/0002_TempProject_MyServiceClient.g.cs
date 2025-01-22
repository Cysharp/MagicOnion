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
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::System.Collections.Generic.List<global::System.String>> GetStringValuesAsync;
                    public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::System.Collections.Generic.List<global::System.Int32>> GetIntValuesAsync;
                    public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
                    {
                        this.GetStringValuesAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::System.Collections.Generic.List<global::System.String>>(global::Grpc.Core.MethodType.Unary, "IMyService", "GetStringValuesAsync", serializerProvider);
                        this.GetIntValuesAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::System.Collections.Generic.List<global::System.Int32>>(global::Grpc.Core.MethodType.Unary, "IMyService", "GetIntValuesAsync", serializerProvider);
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

                public global::MagicOnion.UnaryResult<global::System.Collections.Generic.List<global::System.String>> GetStringValuesAsync()
                    => this.core.GetStringValuesAsync.InvokeUnary(this, "IMyService/GetStringValuesAsync", global::MessagePack.Nil.Default);
                public global::MagicOnion.UnaryResult<global::System.Collections.Generic.List<global::System.Int32>> GetIntValuesAsync()
                    => this.core.GetIntValuesAsync.InvokeUnary(this, "IMyService/GetIntValuesAsync", global::MessagePack.Nil.Default);
            }
        }
    }
}
