﻿// <auto-generated />
#pragma warning disable

namespace TempProject
{
    partial class MagicOnionInitializer
    {
        static partial class MagicOnionGeneratedClient
        {
            [global::MagicOnion.Ignore]
            public class TempProject_MyHubClient : global::MagicOnion.Client.StreamingHubClientBase<global::TempProject.IMyHub, global::TempProject.IMyHubReceiver>, global::TempProject.IMyHub
            {
                public TempProject_MyHubClient(global::TempProject.IMyHubReceiver receiver, global::Grpc.Core.CallInvoker callInvoker, global::MagicOnion.Client.StreamingHubClientOptions options)
                    : base("IMyHub", receiver, callInvoker, options)
                {
                }

                public global::System.Threading.Tasks.Task A(global::System.Int32 a)
                    => this.WriteMessageWithResponseTaskAsync<global::System.Int32, global::MessagePack.Nil>(-1005848884, a);

                public global::TempProject.IMyHub FireAndForget()
                    => new FireAndForgetClient(this);

                [global::MagicOnion.Ignore]
                class FireAndForgetClient : global::TempProject.IMyHub
                {
                    readonly TempProject_MyHubClient parent;

                    public FireAndForgetClient(TempProject_MyHubClient parent)
                        => this.parent = parent;

                    public global::TempProject.IMyHub FireAndForget() => this;
                    public global::System.Threading.Tasks.Task DisposeAsync() => throw new global::System.NotSupportedException();
                    public global::System.Threading.Tasks.Task WaitForDisconnect() => throw new global::System.NotSupportedException();

                    public global::System.Threading.Tasks.Task A(global::System.Int32 a)
                        => parent.WriteMessageFireAndForgetTaskAsync<global::System.Int32, global::MessagePack.Nil>(-1005848884, a);

                }

                protected override void OnBroadcastEvent(global::System.Int32 methodId, global::System.ReadOnlyMemory<global::System.Byte> data)
                {
                    switch (methodId)
                    {
                        case -1262822265: // Void OnMessage(global::System.Int32 a)
                            {
                                var value = base.Deserialize<global::System.Int32>(data);
                                receiver.OnMessage(value);
                            }
                            break;
                    }
                }

                protected override void OnResponseEvent(global::System.Int32 methodId, global::System.Object taskSource, global::System.ReadOnlyMemory<global::System.Byte> data)
                {
                    switch (methodId)
                    {
                        case -1005848884: // Task A(global::System.Int32 a)
                            base.SetResultForResponse<global::MessagePack.Nil>(taskSource, data);
                            break;
                    }
                }

                protected override void OnClientResultEvent(global::System.Int32 methodId, global::System.Guid messageId, global::System.ReadOnlyMemory<global::System.Byte> data)
                {
                }
            }
        }
    }
}
