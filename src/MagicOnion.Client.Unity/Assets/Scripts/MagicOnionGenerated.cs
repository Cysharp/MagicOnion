#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
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

            MagicOnionClientRegistry<Sandbox.NetCoreServer.Services.IMyFirstService>.Register((x, y) => new Sandbox.NetCoreServer.Services.IMyFirstServiceClient(x, y));

            StreamingHubClientRegistry<Sandbox.NetCoreServer.Hubs.IChatHub, Sandbox.NetCoreServer.Hubs.IMessageReceiver2>.Register((a, _, b, c, d, e) => new Sandbox.NetCoreServer.Hubs.IChatHubClient(a, b, c, d, e));
            StreamingHubClientRegistry<Sandbox.NetCoreServer.Hubs.ITestHub, Sandbox.NetCoreServer.Hubs.IMessageReceiver>.Register((a, _, b, c, d, e) => new Sandbox.NetCoreServer.Hubs.ITestHubClient(a, b, c, d, e));
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
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
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(8)
            {
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int, int>), 0 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int>), 1 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, string, double>), 2 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, string>), 3 },
                {typeof(global::Sandbox.NetCoreServer.Hubs.TestObject[]), 4 },
                {typeof(global::Sandbox.NetCoreServer.Services.OreOreResponse[]), 5 },
                {typeof(global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>), 6 },
                {typeof(global::Sandbox.NetCoreServer.Services.TestEnum), 7 },
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
                case 0: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int, int>(default(int), default(int), default(int));
                case 1: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int>(default(int), default(int));
                case 2: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, string, double>(default(int), default(string), default(double));
                case 3: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, string>(default(string), default(string));
                case 4: return new global::MessagePack.Formatters.ArrayFormatter<global::Sandbox.NetCoreServer.Hubs.TestObject>();
                case 5: return new global::MessagePack.Formatters.ArrayFormatter<global::Sandbox.NetCoreServer.Services.OreOreResponse>();
                case 6: return new global::MessagePack.Formatters.ListFormatter<global::Sandbox.NetCoreServer.Services.OreOreResponse>();
                case 7: return new MagicOnion.Formatters.Sandbox.NetCoreServer.Services.TestEnumFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MagicOnion.Formatters.Sandbox.NetCoreServer.Services
{
    using System;
    using MessagePack;

    public sealed class TestEnumFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Sandbox.NetCoreServer.Services.TestEnum>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Sandbox.NetCoreServer.Services.TestEnum value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Sandbox.NetCoreServer.Services.TestEnum Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Sandbox.NetCoreServer.Services.TestEnum)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Sandbox.NetCoreServer.Services {
    using MagicOnion;
    using MagicOnion.Client;
    using Grpc.Core;
    using MessagePack;

    public class IMyFirstServiceClient : MagicOnionClientBase<global::Sandbox.NetCoreServer.Services.IMyFirstService>, global::Sandbox.NetCoreServer.Services.IMyFirstService
    {
        static readonly Method<byte[], byte[]> ZeroAsyncMethod;
        static readonly Method<byte[], byte[]> OneAsyncMethod;
        static readonly Method<byte[], byte[]> SumAsyncMethod;
        static readonly Method<byte[], byte[]> OreOreAsyncMethod;
        static readonly Method<byte[], byte[]> OreOre2AsyncMethod;
        static readonly Method<byte[], byte[]> OreOre3AsyncMethod;
        static readonly Method<byte[], byte[]> LegacyZeroAsyncMethod;
        static readonly Method<byte[], byte[]> LegacyOneAsyncMethod;
        static readonly Method<byte[], byte[]> LegacySumAsyncMethod;
        static readonly Method<byte[], byte[]> LegacyOreOreAsyncMethod;
        static readonly Method<byte[], byte[]> LegacyOreOre2AsyncMethod;
        static readonly Method<byte[], byte[]> LegacyOreOre3AsyncMethod;
        static readonly Method<byte[], byte[]> ClientStreamingSampleAsyncMethod;
        static readonly Method<byte[], byte[]> ServertSreamingSampleAsyncMethod;
        static readonly Method<byte[], byte[]> DuplexStreamingSampleAyncMethod;

        static IMyFirstServiceClient()
        {
            ZeroAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "ZeroAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OneAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OneAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            SumAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "SumAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OreOreAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OreOreAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OreOre2AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OreOre2Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OreOre3AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OreOre3Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyZeroAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyZeroAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOneAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOneAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacySumAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacySumAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOreOreAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOreOreAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOreOre2AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOreOre2Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOreOre3AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOreOre3Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ClientStreamingSampleAsyncMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IMyFirstService", "ClientStreamingSampleAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServertSreamingSampleAsyncMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IMyFirstService", "ServertSreamingSampleAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            DuplexStreamingSampleAyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMyFirstService", "DuplexStreamingSampleAync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IMyFirstServiceClient()
        {
        }

        public IMyFirstServiceClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IMyFirstService> Clone()
        {
            var clone = new IMyFirstServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }

        public new IMyFirstService WithHeaders(Metadata headers)
        {
            return base.WithHeaders(headers);
        }

        public new IMyFirstService WithCancellationToken(System.Threading.CancellationToken cancellationToken)
        {
            return base.WithCancellationToken(cancellationToken);
        }

        public new IMyFirstService WithDeadline(System.DateTime deadline)
        {
            return base.WithDeadline(deadline);
        }

        public new IMyFirstService WithHost(string host)
        {
            return base.WithHost(host);
        }

        public new IMyFirstService WithOptions(CallOptions option)
        {
            return base.WithOptions(option);
        }
   
        public global::MagicOnion.UnaryResult<global::MessagePack.Nil> ZeroAsync()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncUnaryCall(ZeroAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<global::MessagePack.Nil>(__callResult, base.resolver);
        }
        public global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.TestEnum> OneAsync(int z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(OneAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.NetCoreServer.Services.TestEnum>(__callResult, base.resolver);
        }
        public global::MagicOnion.UnaryResult<string> SumAsync(int x, int y)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(SumAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<string>(__callResult, base.resolver);
        }
        public global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse> OreOreAsync(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(OreOreAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse>(__callResult, base.resolver);
        }
        public global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse[]> OreOre2Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(OreOre2AsyncMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse[]>(__callResult, base.resolver);
        }
        public global::MagicOnion.UnaryResult<global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>> OreOre3Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(OreOre3AsyncMethod, base.host, base.option, __request);
            return new UnaryResult<global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>(__callResult, base.resolver);
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::MessagePack.Nil>> LegacyZeroAsync()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncUnaryCall(LegacyZeroAsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new UnaryResult<global::MessagePack.Nil>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.TestEnum>> LegacyOneAsync(int z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LegacyOneAsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new UnaryResult<global::Sandbox.NetCoreServer.Services.TestEnum>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<string>> LegacySumAsync(int x, int y)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LegacySumAsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new UnaryResult<string>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse>> LegacyOreOreAsync(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LegacyOreOreAsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse[]>> LegacyOreOre2Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LegacyOreOre2AsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse[]>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>> LegacyOreOre3Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(z, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LegacyOreOre3AsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new UnaryResult<global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.ClientStreamingResult<int, string>> ClientStreamingSampleAsync()
        {
            var __callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(ClientStreamingSampleAsyncMethod, base.host, base.option);
            return System.Threading.Tasks.Task.FromResult(new ClientStreamingResult<int, string>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.ServerStreamingResult<string>> ServertSreamingSampleAsync(int x, int y, int z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServertSreamingSampleAsyncMethod, base.host, base.option, __request);
            return System.Threading.Tasks.Task.FromResult(new ServerStreamingResult<string>(__callResult, base.resolver));
        }
        public global::System.Threading.Tasks.Task<global::MagicOnion.DuplexStreamingResult<int, string>> DuplexStreamingSampleAync()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingSampleAyncMethod, base.host, base.option);
            return System.Threading.Tasks.Task.FromResult(new DuplexStreamingResult<int, string>(__callResult, base.resolver));
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Sandbox.NetCoreServer.Hubs {
    using Grpc.Core;
    using Grpc.Core.Logging;
    using MagicOnion;
    using MagicOnion.Client;
    using MessagePack;
    using System;
    using System.Threading.Tasks;

    public class IChatHubClient : StreamingHubClientBase<global::Sandbox.NetCoreServer.Hubs.IChatHub, global::Sandbox.NetCoreServer.Hubs.IMessageReceiver2>, global::Sandbox.NetCoreServer.Hubs.IChatHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IChatHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod { get { return method; } }

        readonly global::Sandbox.NetCoreServer.Hubs.IChatHub __fireAndForgetClient;

        public IChatHubClient(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
            : base(callInvoker, host, option, resolver, logger)
        {
            this.__fireAndForgetClient = new FireAndForgetClient(this);
        }
        
        public global::Sandbox.NetCoreServer.Hubs.IChatHub FireAndForget()
        {
            return __fireAndForgetClient;
        }

        protected override Task OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 470021452: // OnReceiveMessage
                {
                    var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<string, string>>(data, resolver);
                    receiver.OnReceiveMessage(result.Item1, result.Item2); return Task.CompletedTask;
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
                default:
                    break;
            }
        }
   
        public global::System.Threading.Tasks.Task JoinAsync(string userName, string roomName)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<string, string>, Nil>(-733403293, new DynamicArgumentTuple<string, string>(userName, roomName));
        }

        public global::System.Threading.Tasks.Task LeaveAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(1368362116, Nil.Default);
        }

        public global::System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            return WriteMessageWithResponseAsync<string, Nil>(-601690414, message);
        }


        class FireAndForgetClient : global::Sandbox.NetCoreServer.Hubs.IChatHub
        {
            readonly IChatHubClient __parent;

            public FireAndForgetClient(IChatHubClient parentClient)
            {
                this.__parent = parentClient;
            }

            public global::Sandbox.NetCoreServer.Hubs.IChatHub FireAndForget()
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

            public global::System.Threading.Tasks.Task JoinAsync(string userName, string roomName)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<string, string>>(-733403293, new DynamicArgumentTuple<string, string>(userName, roomName));
            }

            public global::System.Threading.Tasks.Task LeaveAsync()
            {
                return __parent.WriteMessageAsync<Nil>(1368362116, Nil.Default);
            }

            public global::System.Threading.Tasks.Task SendMessageAsync(string message)
            {
                return __parent.WriteMessageAsync<string>(-601690414, message);
            }

        }
    }

    public class ITestHubClient : StreamingHubClientBase<global::Sandbox.NetCoreServer.Hubs.ITestHub, global::Sandbox.NetCoreServer.Hubs.IMessageReceiver>, global::Sandbox.NetCoreServer.Hubs.ITestHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "ITestHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod { get { return method; } }

        readonly global::Sandbox.NetCoreServer.Hubs.ITestHub __fireAndForgetClient;

        public ITestHubClient(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
            : base(callInvoker, host, option, resolver, logger)
        {
            this.__fireAndForgetClient = new FireAndForgetClient(this);
        }
        
        public global::Sandbox.NetCoreServer.Hubs.ITestHub FireAndForget()
        {
            return __fireAndForgetClient;
        }

        protected override Task OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 908152736: // ZeroArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    return receiver.ZeroArgument();
                }
                case -707027732: // OneArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<int>(data, resolver);
                    return receiver.OneArgument(result);
                }
                case -897846353: // MoreArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string, double>>(data, resolver);
                    return receiver.MoreArgument(result.Item1, result.Item2, result.Item3);
                }
                case 454186482: // VoidZeroArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    receiver.VoidZeroArgument(); return Task.CompletedTask;
                }
                case -1221768450: // VoidOneArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<int>(data, resolver);
                    receiver.VoidOneArgument(result); return Task.CompletedTask;
                }
                case 1213039077: // VoidMoreArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string, double>>(data, resolver);
                    receiver.VoidMoreArgument(result.Item1, result.Item2, result.Item3); return Task.CompletedTask;
                }
                case -2034765446: // OneArgument2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject>(data, resolver);
                    return receiver.OneArgument2(result);
                }
                case 676118308: // VoidOneArgument2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject>(data, resolver);
                    receiver.VoidOneArgument2(result); return Task.CompletedTask;
                }
                case -2017987827: // OneArgument3
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject[]>(data, resolver);
                    return receiver.OneArgument3(result);
                }
                case 692895927: // VoidOneArgument3
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject[]>(data, resolver);
                    receiver.VoidOneArgument3(result); return Task.CompletedTask;
                }
                default:
                    return Task.CompletedTask;
            }
        }

        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 908152736: // ZeroArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -707027732: // OneArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -897846353: // MoreArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 1229270708: // RetrunZeroArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<int>(data, resolver);
                    ((TaskCompletionSource<int>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -2084706656: // RetrunOneArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<string>(data, resolver);
                    ((TaskCompletionSource<string>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -1898269861: // RetrunMoreArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<double>(data, resolver);
                    ((TaskCompletionSource<double>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -2034765446: // OneArgument2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -168691754: // RetrunOneArgument2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject>(data, resolver);
                    ((TaskCompletionSource<global::Sandbox.NetCoreServer.Hubs.TestObject>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -2017987827: // OneArgument3
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -151914135: // RetrunOneArgument3
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject[]>(data, resolver);
                    ((TaskCompletionSource<global::Sandbox.NetCoreServer.Hubs.TestObject[]>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                default:
                    break;
            }
        }
   
        public global::System.Threading.Tasks.Task ZeroArgument()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(908152736, Nil.Default);
        }

        public global::System.Threading.Tasks.Task OneArgument(int x)
        {
            return WriteMessageWithResponseAsync<int, Nil>(-707027732, x);
        }

        public global::System.Threading.Tasks.Task MoreArgument(int x, string y, double z)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<int, string, double>, Nil>(-897846353, new DynamicArgumentTuple<int, string, double>(x, y, z));
        }

        public global::System.Threading.Tasks.Task<int> RetrunZeroArgument()
        {
            return WriteMessageWithResponseAsync<Nil, int> (1229270708, Nil.Default);
        }

        public global::System.Threading.Tasks.Task<string> RetrunOneArgument(int x)
        {
            return WriteMessageWithResponseAsync<int, string> (-2084706656, x);
        }

        public global::System.Threading.Tasks.Task<double> RetrunMoreArgument(int x, string y, double z)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<int, string, double>, double> (-1898269861, new DynamicArgumentTuple<int, string, double>(x, y, z));
        }

        public global::System.Threading.Tasks.Task OneArgument2(global::Sandbox.NetCoreServer.Hubs.TestObject x)
        {
            return WriteMessageWithResponseAsync<global::Sandbox.NetCoreServer.Hubs.TestObject, Nil>(-2034765446, x);
        }

        public global::System.Threading.Tasks.Task<global::Sandbox.NetCoreServer.Hubs.TestObject> RetrunOneArgument2(global::Sandbox.NetCoreServer.Hubs.TestObject x)
        {
            return WriteMessageWithResponseAsync<global::Sandbox.NetCoreServer.Hubs.TestObject, global::Sandbox.NetCoreServer.Hubs.TestObject> (-168691754, x);
        }

        public global::System.Threading.Tasks.Task OneArgument3(global::Sandbox.NetCoreServer.Hubs.TestObject[] x)
        {
            return WriteMessageWithResponseAsync<global::Sandbox.NetCoreServer.Hubs.TestObject[], Nil>(-2017987827, x);
        }

        public global::System.Threading.Tasks.Task<global::Sandbox.NetCoreServer.Hubs.TestObject[]> RetrunOneArgument3(global::Sandbox.NetCoreServer.Hubs.TestObject[] x)
        {
            return WriteMessageWithResponseAsync<global::Sandbox.NetCoreServer.Hubs.TestObject[], global::Sandbox.NetCoreServer.Hubs.TestObject[]> (-151914135, x);
        }


        class FireAndForgetClient : global::Sandbox.NetCoreServer.Hubs.ITestHub
        {
            readonly ITestHubClient __parent;

            public FireAndForgetClient(ITestHubClient parentClient)
            {
                this.__parent = parentClient;
            }

            public global::Sandbox.NetCoreServer.Hubs.ITestHub FireAndForget()
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

            public global::System.Threading.Tasks.Task ZeroArgument()
            {
                return __parent.WriteMessageAsync<Nil>(908152736, Nil.Default);
            }

            public global::System.Threading.Tasks.Task OneArgument(int x)
            {
                return __parent.WriteMessageAsync<int>(-707027732, x);
            }

            public global::System.Threading.Tasks.Task MoreArgument(int x, string y, double z)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<int, string, double>>(-897846353, new DynamicArgumentTuple<int, string, double>(x, y, z));
            }

            public global::System.Threading.Tasks.Task<int> RetrunZeroArgument()
            {
                return __parent.WriteMessageAsyncFireAndForget<Nil, int> (1229270708, Nil.Default);
            }

            public global::System.Threading.Tasks.Task<string> RetrunOneArgument(int x)
            {
                return __parent.WriteMessageAsyncFireAndForget<int, string> (-2084706656, x);
            }

            public global::System.Threading.Tasks.Task<double> RetrunMoreArgument(int x, string y, double z)
            {
                return __parent.WriteMessageAsyncFireAndForget<DynamicArgumentTuple<int, string, double>, double> (-1898269861, new DynamicArgumentTuple<int, string, double>(x, y, z));
            }

            public global::System.Threading.Tasks.Task OneArgument2(global::Sandbox.NetCoreServer.Hubs.TestObject x)
            {
                return __parent.WriteMessageAsync<global::Sandbox.NetCoreServer.Hubs.TestObject>(-2034765446, x);
            }

            public global::System.Threading.Tasks.Task<global::Sandbox.NetCoreServer.Hubs.TestObject> RetrunOneArgument2(global::Sandbox.NetCoreServer.Hubs.TestObject x)
            {
                return __parent.WriteMessageAsyncFireAndForget<global::Sandbox.NetCoreServer.Hubs.TestObject, global::Sandbox.NetCoreServer.Hubs.TestObject> (-168691754, x);
            }

            public global::System.Threading.Tasks.Task OneArgument3(global::Sandbox.NetCoreServer.Hubs.TestObject[] x)
            {
                return __parent.WriteMessageAsync<global::Sandbox.NetCoreServer.Hubs.TestObject[]>(-2017987827, x);
            }

            public global::System.Threading.Tasks.Task<global::Sandbox.NetCoreServer.Hubs.TestObject[]> RetrunOneArgument3(global::Sandbox.NetCoreServer.Hubs.TestObject[] x)
            {
                return __parent.WriteMessageAsyncFireAndForget<global::Sandbox.NetCoreServer.Hubs.TestObject[], global::Sandbox.NetCoreServer.Hubs.TestObject[]> (-151914135, x);
            }

        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
