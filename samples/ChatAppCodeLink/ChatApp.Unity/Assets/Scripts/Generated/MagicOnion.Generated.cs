#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace MagicOnion
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::MagicOnion;
    using global::MagicOnion.Client;

    public static partial class MagicOnionInitializer
    {
        static bool isRegistered = false;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            if(isRegistered) return;
            isRegistered = true;

            MagicOnionClientRegistry<Assets.Scripts.ServerShared.Services.IChatService>.Register((x, y) => new Assets.Scripts.ServerShared.Services.IChatServiceClient(x, y));

            StreamingHubClientRegistry<Assets.Scripts.ServerShared.Hubs.IChatHub, Assets.Scripts.ServerShared.Hubs.IChatHubReceiver>.Register((a, _, b, c, d, e) => new Assets.Scripts.ServerShared.Hubs.IChatHubClient(a, b, c, d, e));
        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace MagicOnion.Resolvers
{
    using System;
    using MessagePack;

    public class MagicOnionResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new MagicOnionResolver();

        MagicOnionResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = MagicOnionResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class MagicOnionResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static MagicOnionResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(3)
            {
                {typeof(global::MagicOnion.DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, string>>), 0 },
                {typeof(global::System.Collections.Generic.Dictionary<int, string>), 1 },
                {typeof(global::System.Collections.Generic.List<int>), 2 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MagicOnion.DynamicArgumentTupleFormatter<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, string>>(default(global::System.Collections.Generic.List<int>), default(global::System.Collections.Generic.Dictionary<int, string>));
                case 1: return new global::MessagePack.Formatters.DictionaryFormatter<int, string>();
                case 2: return new global::MessagePack.Formatters.ListFormatter<int>();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Assets.Scripts.ServerShared.Services {
    using MagicOnion;
    using MagicOnion.Client;
    using Grpc.Core;
    using MessagePack;

    public class IChatServiceClient : MagicOnionClientBase<global::Assets.Scripts.ServerShared.Services.IChatService>, global::Assets.Scripts.ServerShared.Services.IChatService
    {
        static readonly Method<byte[], byte[]> SendReportAsyncMethod;

        static IChatServiceClient()
        {
            SendReportAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IChatService", "SendReportAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IChatServiceClient()
        {
        }

        public IChatServiceClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IChatService> Clone()
        {
            var clone = new IChatServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }

        public new IChatService WithHeaders(Metadata headers)
        {
            return base.WithHeaders(headers);
        }

        public new IChatService WithCancellationToken(System.Threading.CancellationToken cancellationToken)
        {
            return base.WithCancellationToken(cancellationToken);
        }

        public new IChatService WithDeadline(System.DateTime deadline)
        {
            return base.WithDeadline(deadline);
        }

        public new IChatService WithHost(string host)
        {
            return base.WithHost(host);
        }

        public new IChatService WithOptions(CallOptions option)
        {
            return base.WithOptions(option);
        }
   
        public global::MagicOnion.UnaryResult<global::MessagePack.Nil> SendReportAsync(string message)
        {
            var __request = LZ4MessagePackSerializer.Serialize(message, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(SendReportAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<global::MessagePack.Nil>(__callResult, base.resolver);
        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Assets.Scripts.ServerShared.Hubs {
    using Grpc.Core;
    using Grpc.Core.Logging;
    using MagicOnion;
    using MagicOnion.Client;
    using MessagePack;
    using System;
    using System.Threading.Tasks;

    public class IChatHubClient : StreamingHubClientBase<global::Assets.Scripts.ServerShared.Hubs.IChatHub, global::Assets.Scripts.ServerShared.Hubs.IChatHubReceiver>, global::Assets.Scripts.ServerShared.Hubs.IChatHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IChatHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod { get { return method; } }

        readonly global::Assets.Scripts.ServerShared.Hubs.IChatHub __fireAndForgetClient;

        public IChatHubClient(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
            : base(callInvoker, host, option, resolver, logger)
        {
            this.__fireAndForgetClient = new FireAndForgetClient(this);
        }
        
        public global::Assets.Scripts.ServerShared.Hubs.IChatHub FireAndForget()
        {
            return __fireAndForgetClient;
        }

        protected override Task OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case -1297457280: // OnJoin
                {
                    var result = LZ4MessagePackSerializer.Deserialize<string>(data, resolver);
                    receiver.OnJoin(result); return Task.CompletedTask;
                }
                case 532410095: // OnLeave
                {
                    var result = LZ4MessagePackSerializer.Deserialize<string>(data, resolver);
                    receiver.OnLeave(result); return Task.CompletedTask;
                }
                case -552695459: // OnSendMessage
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Assets.Scripts.ServerShared.MessagePackObjects.MessageResponse>(data, resolver);
                    receiver.OnSendMessage(result); return Task.CompletedTask;
                }
                default:
                    return Task.CompletedTask;
            }
        }

        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case -733403293: // JoinAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 1368362116: // LeaveAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -601690414: // SendMessageAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -852153394: // SampleMethod
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                default:
                    break;
            }
        }
   
        public global::System.Threading.Tasks.Task JoinAsync(global::Assets.Scripts.ServerShared.MessagePackObjects.JoinRequest request)
        {
            return WriteMessageWithResponseAsync<global::Assets.Scripts.ServerShared.MessagePackObjects.JoinRequest, Nil>(-733403293, request);
        }

        public global::System.Threading.Tasks.Task LeaveAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(1368362116, Nil.Default);
        }

        public global::System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            return WriteMessageWithResponseAsync<string, Nil>(-601690414, message);
        }

        public global::System.Threading.Tasks.Task SampleMethod(global::System.Collections.Generic.List<int> sampleList, global::System.Collections.Generic.Dictionary<int, string> sampleDictionary)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, string>>, Nil>(-852153394, new DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, string>>(sampleList, sampleDictionary));
        }


        class FireAndForgetClient : global::Assets.Scripts.ServerShared.Hubs.IChatHub
        {
            readonly IChatHubClient __parent;

            public FireAndForgetClient(IChatHubClient parentClient)
            {
                this.__parent = parentClient;
            }

            public global::Assets.Scripts.ServerShared.Hubs.IChatHub FireAndForget()
            {
                throw new NotSupportedException();
            }

            public Task DisposeAsync()
            {
                throw new NotSupportedException();
            }

            public Task WaitForDisconnect()
            {
                throw new NotSupportedException();
            }

            public global::System.Threading.Tasks.Task JoinAsync(global::Assets.Scripts.ServerShared.MessagePackObjects.JoinRequest request)
            {
                return __parent.WriteMessageAsync<global::Assets.Scripts.ServerShared.MessagePackObjects.JoinRequest>(-733403293, request);
            }

            public global::System.Threading.Tasks.Task LeaveAsync()
            {
                return __parent.WriteMessageAsync<Nil>(1368362116, Nil.Default);
            }

            public global::System.Threading.Tasks.Task SendMessageAsync(string message)
            {
                return __parent.WriteMessageAsync<string>(-601690414, message);
            }

            public global::System.Threading.Tasks.Task SampleMethod(global::System.Collections.Generic.List<int> sampleList, global::System.Collections.Generic.Dictionary<int, string> sampleDictionary)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, string>>>(-852153394, new DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, string>>(sampleList, sampleDictionary));
            }

        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
