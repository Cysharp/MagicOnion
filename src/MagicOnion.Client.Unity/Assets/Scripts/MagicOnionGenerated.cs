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

            MagicOnionClientRegistry<Sandbox.NetCoreServer.Hubs.ITestService>.Register((x, y, z) => new Sandbox.NetCoreServer.Hubs.ITestServiceClient(x, y, z));
            MagicOnionClientRegistry<Sandbox.NetCoreServer.Services.IMyFirstService>.Register((x, y, z) => new Sandbox.NetCoreServer.Services.IMyFirstServiceClient(x, y, z));

            StreamingHubClientRegistry<Sandbox.NetCoreServer.Hubs.IGamingHub, Sandbox.NetCoreServer.Hubs.IGamingHubReceiver>.Register((a, _, b, c, d, e) => new Sandbox.NetCoreServer.Hubs.IGamingHubClient(a, b, c, d, e));
            StreamingHubClientRegistry<Sandbox.NetCoreServer.Hubs.IBugReproductionHub, Sandbox.NetCoreServer.Hubs.IBugReproductionHubReceiver>.Register((a, _, b, c, d, e) => new Sandbox.NetCoreServer.Hubs.IBugReproductionHubClient(a, b, c, d, e));
            StreamingHubClientRegistry<Sandbox.NetCoreServer.Hubs.IChatHub, Sandbox.NetCoreServer.Hubs.IMessageReceiver2>.Register((a, _, b, c, d, e) => new Sandbox.NetCoreServer.Hubs.IChatHubClient(a, b, c, d, e));
            StreamingHubClientRegistry<Sandbox.NetCoreServer.Hubs.ITestHub, Sandbox.NetCoreServer.Hubs.IMessageReceiver>.Register((a, _, b, c, d, e) => new Sandbox.NetCoreServer.Hubs.ITestHubClient(a, b, c, d, e));
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
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(12)
            {
                {typeof(global::MagicOnion.DynamicArgumentTuple<global::UnityEngine.Vector3, global::UnityEngine.Quaternion>), 0 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int, int>), 1 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int>), 2 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, string, double>), 3 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, long>), 4 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, string, global::UnityEngine.Vector3, global::UnityEngine.Quaternion>), 5 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, string>), 6 },
                {typeof(global::Sandbox.NetCoreServer.Hubs.Player[]), 7 },
                {typeof(global::Sandbox.NetCoreServer.Hubs.TestObject[]), 8 },
                {typeof(global::Sandbox.NetCoreServer.Services.OreOreResponse[]), 9 },
                {typeof(global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>), 10 },
                {typeof(global::Sandbox.NetCoreServer.Services.TestEnum), 11 },
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
                case 0: return new global::MagicOnion.DynamicArgumentTupleFormatter<global::UnityEngine.Vector3, global::UnityEngine.Quaternion>(default(global::UnityEngine.Vector3), default(global::UnityEngine.Quaternion));
                case 1: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int, int>(default(int), default(int), default(int));
                case 2: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int>(default(int), default(int));
                case 3: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, string, double>(default(int), default(string), default(double));
                case 4: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, long>(default(string), default(long));
                case 5: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, string, global::UnityEngine.Vector3, global::UnityEngine.Quaternion>(default(string), default(string), default(global::UnityEngine.Vector3), default(global::UnityEngine.Quaternion));
                case 6: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, string>(default(string), default(string));
                case 7: return new global::MessagePack.Formatters.ArrayFormatter<global::Sandbox.NetCoreServer.Hubs.Player>();
                case 8: return new global::MessagePack.Formatters.ArrayFormatter<global::Sandbox.NetCoreServer.Hubs.TestObject>();
                case 9: return new global::MessagePack.Formatters.ArrayFormatter<global::Sandbox.NetCoreServer.Services.OreOreResponse>();
                case 10: return new global::MessagePack.Formatters.ListFormatter<global::Sandbox.NetCoreServer.Services.OreOreResponse>();
                case 11: return new MagicOnion.Formatters.TestEnumFormatter();
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

namespace MagicOnion.Formatters
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
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Sandbox.NetCoreServer.Hubs {
    using System;
	using MagicOnion;
    using MagicOnion.Client;
    using Grpc.Core;
    using MessagePack;

    public class ITestServiceClient : MagicOnionClientBase<global::Sandbox.NetCoreServer.Hubs.ITestService>, global::Sandbox.NetCoreServer.Hubs.ITestService
    {
        static readonly Method<byte[], byte[]> FooBarBazMethod;
        static readonly Func<RequestContext, ResponseContext> FooBarBazDelegate;

        static ITestServiceClient()
        {
            FooBarBazMethod = new Method<byte[], byte[]>(MethodType.Unary, "ITestService", "FooBarBaz", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            FooBarBazDelegate = _FooBarBaz;
        }

        ITestServiceClient()
        {
        }

        public ITestServiceClient(CallInvoker callInvoker, IFormatterResolver resolver, IClientFilter[] filters)
            : base(callInvoker, resolver, filters)
        {
        }

        protected override MagicOnionClientBase<ITestService> Clone()
        {
            var clone = new ITestServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            clone.filters = filters;
            return clone;
        }

        public new ITestService WithHeaders(Metadata headers)
        {
            return base.WithHeaders(headers);
        }

        public new ITestService WithCancellationToken(System.Threading.CancellationToken cancellationToken)
        {
            return base.WithCancellationToken(cancellationToken);
        }

        public new ITestService WithDeadline(System.DateTime deadline)
        {
            return base.WithDeadline(deadline);
        }

        public new ITestService WithHost(string host)
        {
            return base.WithHost(host);
        }

        public new ITestService WithOptions(CallOptions option)
        {
            return base.WithOptions(option);
        }
   
        static ResponseContext _FooBarBaz(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<string, long>, long[]>(__context, FooBarBazMethod);
        }

        public global::MagicOnion.UnaryResult<long[]> FooBarBaz(string x, long y)
        {
            return InvokeAsync<DynamicArgumentTuple<string, long>, long[]>("ITestService/FooBarBaz", new DynamicArgumentTuple<string, long>(x, y), FooBarBazDelegate);
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

namespace Sandbox.NetCoreServer.Services {
    using System;
	using MagicOnion;
    using MagicOnion.Client;
    using Grpc.Core;
    using MessagePack;

    public class IMyFirstServiceClient : MagicOnionClientBase<global::Sandbox.NetCoreServer.Services.IMyFirstService>, global::Sandbox.NetCoreServer.Services.IMyFirstService
    {
        static readonly Method<byte[], byte[]> ZeroAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> ZeroAsyncDelegate;
        static readonly Method<byte[], byte[]> OneAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> OneAsyncDelegate;
        static readonly Method<byte[], byte[]> SumAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> SumAsyncDelegate;
        static readonly Method<byte[], byte[]> OreOreAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> OreOreAsyncDelegate;
        static readonly Method<byte[], byte[]> OreOre2AsyncMethod;
        static readonly Func<RequestContext, ResponseContext> OreOre2AsyncDelegate;
        static readonly Method<byte[], byte[]> OreOre3AsyncMethod;
        static readonly Func<RequestContext, ResponseContext> OreOre3AsyncDelegate;
        static readonly Method<byte[], byte[]> LegacyZeroAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> LegacyZeroAsyncDelegate;
        static readonly Method<byte[], byte[]> LegacyOneAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> LegacyOneAsyncDelegate;
        static readonly Method<byte[], byte[]> LegacySumAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> LegacySumAsyncDelegate;
        static readonly Method<byte[], byte[]> LegacyOreOreAsyncMethod;
        static readonly Func<RequestContext, ResponseContext> LegacyOreOreAsyncDelegate;
        static readonly Method<byte[], byte[]> LegacyOreOre2AsyncMethod;
        static readonly Func<RequestContext, ResponseContext> LegacyOreOre2AsyncDelegate;
        static readonly Method<byte[], byte[]> LegacyOreOre3AsyncMethod;
        static readonly Func<RequestContext, ResponseContext> LegacyOreOre3AsyncDelegate;
        static readonly Method<byte[], byte[]> ClientStreamingSampleAsyncMethod;
        static readonly Method<byte[], byte[]> ServertSreamingSampleAsyncMethod;
        static readonly Method<byte[], byte[]> DuplexStreamingSampleAyncMethod;

        static IMyFirstServiceClient()
        {
            ZeroAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "ZeroAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ZeroAsyncDelegate = _ZeroAsync;
            OneAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OneAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OneAsyncDelegate = _OneAsync;
            SumAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "SumAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            SumAsyncDelegate = _SumAsync;
            OreOreAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OreOreAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OreOreAsyncDelegate = _OreOreAsync;
            OreOre2AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OreOre2Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OreOre2AsyncDelegate = _OreOre2Async;
            OreOre3AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "OreOre3Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OreOre3AsyncDelegate = _OreOre3Async;
            LegacyZeroAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyZeroAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyZeroAsyncDelegate = _LegacyZeroAsync;
            LegacyOneAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOneAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOneAsyncDelegate = _LegacyOneAsync;
            LegacySumAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacySumAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacySumAsyncDelegate = _LegacySumAsync;
            LegacyOreOreAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOreOreAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOreOreAsyncDelegate = _LegacyOreOreAsync;
            LegacyOreOre2AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOreOre2Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOreOre2AsyncDelegate = _LegacyOreOre2Async;
            LegacyOreOre3AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "LegacyOreOre3Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LegacyOreOre3AsyncDelegate = _LegacyOreOre3Async;
            ClientStreamingSampleAsyncMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IMyFirstService", "ClientStreamingSampleAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServertSreamingSampleAsyncMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IMyFirstService", "ServertSreamingSampleAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            DuplexStreamingSampleAyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMyFirstService", "DuplexStreamingSampleAync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IMyFirstServiceClient()
        {
        }

        public IMyFirstServiceClient(CallInvoker callInvoker, IFormatterResolver resolver, IClientFilter[] filters)
            : base(callInvoker, resolver, filters)
        {
        }

        protected override MagicOnionClientBase<IMyFirstService> Clone()
        {
            var clone = new IMyFirstServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            clone.filters = filters;
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
   
        static ResponseContext _ZeroAsync(RequestContext __context)
        {
            return CreateResponseContext<global::MessagePack.Nil>(__context, ZeroAsyncMethod);
        }

        public global::MagicOnion.UnaryResult<global::MessagePack.Nil> ZeroAsync()
        {
            return InvokeAsync<Nil, global::MessagePack.Nil>("IMyFirstService/ZeroAsync", Nil.Default, ZeroAsyncDelegate);
        }
        static ResponseContext _OneAsync(RequestContext __context)
        {
            return CreateResponseContext<int, global::Sandbox.NetCoreServer.Services.TestEnum>(__context, OneAsyncMethod);
        }

        public global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.TestEnum> OneAsync(int z)
        {
            return InvokeAsync<int, global::Sandbox.NetCoreServer.Services.TestEnum>("IMyFirstService/OneAsync", z, OneAsyncDelegate);
        }
        static ResponseContext _SumAsync(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<int, int>, string>(__context, SumAsyncMethod);
        }

        public global::MagicOnion.UnaryResult<string> SumAsync(int x, int y)
        {
            return InvokeAsync<DynamicArgumentTuple<int, int>, string>("IMyFirstService/SumAsync", new DynamicArgumentTuple<int, int>(x, y), SumAsyncDelegate);
        }
        static ResponseContext _OreOreAsync(RequestContext __context)
        {
            return CreateResponseContext<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse>(__context, OreOreAsyncMethod);
        }

        public global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse> OreOreAsync(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            return InvokeAsync<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse>("IMyFirstService/OreOreAsync", z, OreOreAsyncDelegate);
        }
        static ResponseContext _OreOre2Async(RequestContext __context)
        {
            return CreateResponseContext<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse[]>(__context, OreOre2AsyncMethod);
        }

        public global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse[]> OreOre2Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            return InvokeAsync<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse[]>("IMyFirstService/OreOre2Async", z, OreOre2AsyncDelegate);
        }
        static ResponseContext _OreOre3Async(RequestContext __context)
        {
            return CreateResponseContext<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>(__context, OreOre3AsyncMethod);
        }

        public global::MagicOnion.UnaryResult<global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>> OreOre3Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            return InvokeAsync<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>("IMyFirstService/OreOre3Async", z, OreOre3AsyncDelegate);
        }
        static ResponseContext _LegacyZeroAsync(RequestContext __context)
        {
            return CreateResponseContext<global::MessagePack.Nil>(__context, LegacyZeroAsyncMethod);
        }

        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::MessagePack.Nil>> LegacyZeroAsync()
        {
            return InvokeTaskAsync<Nil, global::MessagePack.Nil>("IMyFirstService/LegacyZeroAsync", Nil.Default, LegacyZeroAsyncDelegate);
        }
        static ResponseContext _LegacyOneAsync(RequestContext __context)
        {
            return CreateResponseContext<int, global::Sandbox.NetCoreServer.Services.TestEnum>(__context, LegacyOneAsyncMethod);
        }

        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.TestEnum>> LegacyOneAsync(int z)
        {
            return InvokeTaskAsync<int, global::Sandbox.NetCoreServer.Services.TestEnum>("IMyFirstService/LegacyOneAsync", z, LegacyOneAsyncDelegate);
        }
        static ResponseContext _LegacySumAsync(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<int, int>, string>(__context, LegacySumAsyncMethod);
        }

        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<string>> LegacySumAsync(int x, int y)
        {
            return InvokeTaskAsync<DynamicArgumentTuple<int, int>, string>("IMyFirstService/LegacySumAsync", new DynamicArgumentTuple<int, int>(x, y), LegacySumAsyncDelegate);
        }
        static ResponseContext _LegacyOreOreAsync(RequestContext __context)
        {
            return CreateResponseContext<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse>(__context, LegacyOreOreAsyncMethod);
        }

        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse>> LegacyOreOreAsync(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            return InvokeTaskAsync<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse>("IMyFirstService/LegacyOreOreAsync", z, LegacyOreOreAsyncDelegate);
        }
        static ResponseContext _LegacyOreOre2Async(RequestContext __context)
        {
            return CreateResponseContext<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse[]>(__context, LegacyOreOre2AsyncMethod);
        }

        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::Sandbox.NetCoreServer.Services.OreOreResponse[]>> LegacyOreOre2Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            return InvokeTaskAsync<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::Sandbox.NetCoreServer.Services.OreOreResponse[]>("IMyFirstService/LegacyOreOre2Async", z, LegacyOreOre2AsyncDelegate);
        }
        static ResponseContext _LegacyOreOre3Async(RequestContext __context)
        {
            return CreateResponseContext<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>(__context, LegacyOreOre3AsyncMethod);
        }

        public global::System.Threading.Tasks.Task<global::MagicOnion.UnaryResult<global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>> LegacyOreOre3Async(global::Sandbox.NetCoreServer.Services.OreOreRequest z)
        {
            return InvokeTaskAsync<global::Sandbox.NetCoreServer.Services.OreOreRequest, global::System.Collections.Generic.List<global::Sandbox.NetCoreServer.Services.OreOreResponse>>("IMyFirstService/LegacyOreOre3Async", z, LegacyOreOre3AsyncDelegate);
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
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Sandbox.NetCoreServer.Hubs {
    using Grpc.Core;
    using Grpc.Core.Logging;
    using MagicOnion;
    using MagicOnion.Client;
    using MessagePack;
    using System;
    using System.Threading.Tasks;

    public class IGamingHubClient : StreamingHubClientBase<global::Sandbox.NetCoreServer.Hubs.IGamingHub, global::Sandbox.NetCoreServer.Hubs.IGamingHubReceiver>, global::Sandbox.NetCoreServer.Hubs.IGamingHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IGamingHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod { get { return method; } }

        readonly global::Sandbox.NetCoreServer.Hubs.IGamingHub __fireAndForgetClient;

        public IGamingHubClient(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
            : base(callInvoker, host, option, resolver, logger)
        {
            this.__fireAndForgetClient = new FireAndForgetClient(this);
        }
        
        public global::Sandbox.NetCoreServer.Hubs.IGamingHub FireAndForget()
        {
            return __fireAndForgetClient;
        }

        protected override void OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case -1297457280: // OnJoin
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.Player>(data, resolver);
                    receiver.OnJoin(result); break;
                }
                case 532410095: // OnLeave
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.Player>(data, resolver);
                    receiver.OnLeave(result); break;
                }
                case 1429874301: // OnMove
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.Player>(data, resolver);
                    receiver.OnMove(result); break;
                }
                default:
                    break;
            }
        }

        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case -733403293: // JoinAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.Player[]>(data, resolver);
                    ((TaskCompletionSource<global::Sandbox.NetCoreServer.Hubs.Player[]>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 1368362116: // LeaveAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -99261176: // MoveAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                default:
                    break;
            }
        }
   
        public global::System.Threading.Tasks.Task<global::Sandbox.NetCoreServer.Hubs.Player[]> JoinAsync(string roomName, string userName, global::UnityEngine.Vector3 position, global::UnityEngine.Quaternion rotation)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<string, string, global::UnityEngine.Vector3, global::UnityEngine.Quaternion>, global::Sandbox.NetCoreServer.Hubs.Player[]> (-733403293, new DynamicArgumentTuple<string, string, global::UnityEngine.Vector3, global::UnityEngine.Quaternion>(roomName, userName, position, rotation));
        }

        public global::System.Threading.Tasks.Task LeaveAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(1368362116, Nil.Default);
        }

        public global::System.Threading.Tasks.Task MoveAsync(global::UnityEngine.Vector3 position, global::UnityEngine.Quaternion rotation)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<global::UnityEngine.Vector3, global::UnityEngine.Quaternion>, Nil>(-99261176, new DynamicArgumentTuple<global::UnityEngine.Vector3, global::UnityEngine.Quaternion>(position, rotation));
        }


        class FireAndForgetClient : global::Sandbox.NetCoreServer.Hubs.IGamingHub
        {
            readonly IGamingHubClient __parent;

            public FireAndForgetClient(IGamingHubClient parentClient)
            {
                this.__parent = parentClient;
            }

            public global::Sandbox.NetCoreServer.Hubs.IGamingHub FireAndForget()
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

            public global::System.Threading.Tasks.Task<global::Sandbox.NetCoreServer.Hubs.Player[]> JoinAsync(string roomName, string userName, global::UnityEngine.Vector3 position, global::UnityEngine.Quaternion rotation)
            {
                return __parent.WriteMessageAsyncFireAndForget<DynamicArgumentTuple<string, string, global::UnityEngine.Vector3, global::UnityEngine.Quaternion>, global::Sandbox.NetCoreServer.Hubs.Player[]> (-733403293, new DynamicArgumentTuple<string, string, global::UnityEngine.Vector3, global::UnityEngine.Quaternion>(roomName, userName, position, rotation));
            }

            public global::System.Threading.Tasks.Task LeaveAsync()
            {
                return __parent.WriteMessageAsync<Nil>(1368362116, Nil.Default);
            }

            public global::System.Threading.Tasks.Task MoveAsync(global::UnityEngine.Vector3 position, global::UnityEngine.Quaternion rotation)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<global::UnityEngine.Vector3, global::UnityEngine.Quaternion>>(-99261176, new DynamicArgumentTuple<global::UnityEngine.Vector3, global::UnityEngine.Quaternion>(position, rotation));
            }

        }
    }

    public class IBugReproductionHubClient : StreamingHubClientBase<global::Sandbox.NetCoreServer.Hubs.IBugReproductionHub, global::Sandbox.NetCoreServer.Hubs.IBugReproductionHubReceiver>, global::Sandbox.NetCoreServer.Hubs.IBugReproductionHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IBugReproductionHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod { get { return method; } }

        readonly global::Sandbox.NetCoreServer.Hubs.IBugReproductionHub __fireAndForgetClient;

        public IBugReproductionHubClient(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
            : base(callInvoker, host, option, resolver, logger)
        {
            this.__fireAndForgetClient = new FireAndForgetClient(this);
        }
        
        public global::Sandbox.NetCoreServer.Hubs.IBugReproductionHub FireAndForget()
        {
            return __fireAndForgetClient;
        }

        protected override void OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case -735457744: // OnCall
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    receiver.OnCall(); break;
                }
                default:
                    break;
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
                case 904214259: // CallAsync
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                default:
                    break;
            }
        }
   
        public global::System.Threading.Tasks.Task JoinAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(-733403293, Nil.Default);
        }

        public global::System.Threading.Tasks.Task CallAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(904214259, Nil.Default);
        }


        class FireAndForgetClient : global::Sandbox.NetCoreServer.Hubs.IBugReproductionHub
        {
            readonly IBugReproductionHubClient __parent;

            public FireAndForgetClient(IBugReproductionHubClient parentClient)
            {
                this.__parent = parentClient;
            }

            public global::Sandbox.NetCoreServer.Hubs.IBugReproductionHub FireAndForget()
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

            public global::System.Threading.Tasks.Task JoinAsync()
            {
                return __parent.WriteMessageAsync<Nil>(-733403293, Nil.Default);
            }

            public global::System.Threading.Tasks.Task CallAsync()
            {
                return __parent.WriteMessageAsync<Nil>(904214259, Nil.Default);
            }

        }
    }

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

        protected override void OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 470021452: // OnReceiveMessage
                {
                    var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<string, string>>(data, resolver);
                    receiver.OnReceiveMessage(result.Item1, result.Item2); break;
                }
                case -277016929: // Foo2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Foo>(data, resolver);
                    receiver.Foo2(result); break;
                }
                default:
                    break;
            }
        }

        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 100: // JoinAsync
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
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<string, string>, Nil>(100, new DynamicArgumentTuple<string, string>(userName, roomName));
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
                return __parent.WriteMessageAsync<DynamicArgumentTuple<string, string>>(100, new DynamicArgumentTuple<string, string>(userName, roomName));
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

        protected override void OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 908152736: // ZeroArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    receiver.ZeroArgument(); break;
                }
                case -707027732: // OneArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<int>(data, resolver);
                    receiver.OneArgument(result); break;
                }
                case -897846353: // MoreArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string, double>>(data, resolver);
                    receiver.MoreArgument(result.Item1, result.Item2, result.Item3); break;
                }
                case 454186482: // VoidZeroArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<Nil>(data, resolver);
                    receiver.VoidZeroArgument(); break;
                }
                case -1221768450: // VoidOneArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<int>(data, resolver);
                    receiver.VoidOneArgument(result); break;
                }
                case 1213039077: // VoidMoreArgument
                {
                    var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string, double>>(data, resolver);
                    receiver.VoidMoreArgument(result.Item1, result.Item2, result.Item3); break;
                }
                case -2034765446: // OneArgument2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject>(data, resolver);
                    receiver.OneArgument2(result); break;
                }
                case 676118308: // VoidOneArgument2
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject>(data, resolver);
                    receiver.VoidOneArgument2(result); break;
                }
                case -2017987827: // OneArgument3
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject[]>(data, resolver);
                    receiver.OneArgument3(result); break;
                }
                case 692895927: // VoidOneArgument3
                {
                    var result = LZ4MessagePackSerializer.Deserialize<global::Sandbox.NetCoreServer.Hubs.TestObject[]>(data, resolver);
                    receiver.VoidOneArgument3(result); break;
                }
                default:
                    break;
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
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
