﻿// <auto-generated />
#pragma warning disable CS0618 // 'member' is obsolete: 'text'
#pragma warning disable CS0612 // 'member' is obsolete
#pragma warning disable CS0414 // The private field 'field' is assigned but its value is never used
#pragma warning disable CS8019 // Unnecessary using directive.
#pragma warning disable CS1522 // Empty switch block

namespace TempProject
{
    partial class MagicOnionInitializer
    {
        static partial class MagicOnionGeneratedClient
        {
            [global::MagicOnion.Ignore]
            public class TempProject_MyHubClient : global::MagicOnion.Client.StreamingHubClientBase<global::TempProject.IMyHub, global::TempProject.IMyHubReceiver>, global::TempProject.IMyHub
            {
                public TempProject_MyHubClient(global::Grpc.Core.CallInvoker callInvoker, global::System.String host, global::Grpc.Core.CallOptions options, global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider, global::MagicOnion.Client.IMagicOnionClientLogger logger)
                    : base("IMyHub", callInvoker, host, options, serializerProvider, logger)
                {
                }

                public global::System.Threading.Tasks.Task A()
                    => base.WriteMessageWithResponseAsync<global::MessagePack.Nil, global::MessagePack.Nil>(-1005848884, global::MessagePack.Nil.Default);
                public global::System.Threading.Tasks.Task B(global::TempProject.MyObject a)
                    => base.WriteMessageWithResponseAsync<global::TempProject.MyObject, global::MessagePack.Nil>(-955516027, a);
                public global::System.Threading.Tasks.Task C(global::TempProject.MyObject a, global::System.String b)
                    => base.WriteMessageWithResponseAsync<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String>, global::MessagePack.Nil>(-972293646, new global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String>(a, b));
                public global::System.Threading.Tasks.Task D(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                    => base.WriteMessageWithResponseAsync<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>, global::MessagePack.Nil>(-1056181741, new global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>(a, b, c));
                public global::System.Threading.Tasks.Task<global::System.Int32> E(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                    => base.WriteMessageWithResponseAsync<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>, global::System.Int32>(-1072959360, new global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>(a, b, c));

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

                    public global::System.Threading.Tasks.Task A()
                        => parent.WriteMessageFireAndForgetAsync<global::MessagePack.Nil, global::MessagePack.Nil>(-1005848884, global::MessagePack.Nil.Default);
                    public global::System.Threading.Tasks.Task B(global::TempProject.MyObject a)
                        => parent.WriteMessageFireAndForgetAsync<global::TempProject.MyObject, global::MessagePack.Nil>(-955516027, a);
                    public global::System.Threading.Tasks.Task C(global::TempProject.MyObject a, global::System.String b)
                        => parent.WriteMessageFireAndForgetAsync<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String>, global::MessagePack.Nil>(-972293646, new global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String>(a, b));
                    public global::System.Threading.Tasks.Task D(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                        => parent.WriteMessageFireAndForgetAsync<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>, global::MessagePack.Nil>(-1056181741, new global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>(a, b, c));
                    public global::System.Threading.Tasks.Task<global::System.Int32> E(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                        => parent.WriteMessageFireAndForgetAsync<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>, global::System.Int32>(-1072959360, new global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>(a, b, c));

                }

                protected override void OnBroadcastEvent(global::System.Int32 methodId, global::System.ArraySegment<global::System.Byte> data)
                {
                    switch (methodId)
                    {
                        case -1262822265: // Void OnMessage()
                            {
                                var value = base.Deserialize<global::MessagePack.Nil>(data);
                                receiver.OnMessage();
                            }
                            break;
                        case 925462767: // Void OnMessage2(global::TempProject.MyObject a)
                            {
                                var value = base.Deserialize<global::TempProject.MyObject>(data);
                                receiver.OnMessage2(value);
                            }
                            break;
                        case 908685148: // Void OnMessage3(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                            {
                                var value = base.Deserialize<global::MagicOnion.DynamicArgumentTuple<global::TempProject.MyObject, global::System.String, global::System.Int32>>(data);
                                receiver.OnMessage3(value.Item1, value.Item2, value.Item3);
                            }
                            break;
                    }
                }

                protected override void OnResponseEvent(global::System.Int32 methodId, global::System.Object taskCompletionSource, global::System.ArraySegment<global::System.Byte> data)
                {
                    switch (methodId)
                    {
                        case -1005848884: // Task A()
                            base.SetResultForResponse<global::MessagePack.Nil>(taskCompletionSource, data);
                            break;
                        case -955516027: // Task B(global::TempProject.MyObject a)
                            base.SetResultForResponse<global::MessagePack.Nil>(taskCompletionSource, data);
                            break;
                        case -972293646: // Task C(global::TempProject.MyObject a, global::System.String b)
                            base.SetResultForResponse<global::MessagePack.Nil>(taskCompletionSource, data);
                            break;
                        case -1056181741: // Task D(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                            base.SetResultForResponse<global::MessagePack.Nil>(taskCompletionSource, data);
                            break;
                        case -1072959360: // Task<Int32> E(global::TempProject.MyObject a, global::System.String b, global::System.Int32 c)
                            base.SetResultForResponse<global::System.Int32>(taskCompletionSource, data);
                            break;
                    }
                }

            }
        }
    }
}
