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
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IArgumentPattern>.Register(x => new Sandbox.ConsoleServer.IArgumentPatternClient(x), x => new Sandbox.ConsoleServer.IArgumentPatternClient(x));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IStandard>.Register(x => new Sandbox.ConsoleServer.IStandardClient(x), x => new Sandbox.ConsoleServer.IStandardClient(x));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IMyFirstService>.Register(x => new Sandbox.ConsoleServer.IMyFirstServiceClient(x), x => new Sandbox.ConsoleServer.IMyFirstServiceClient(x));
        }
    }
}
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Sandbox.ConsoleServer {
    using MagicOnion;
    using MagicOnion.Client;
    using UniRx;
    using Grpc.Core;
    using ZeroFormatter.Formatters;


    public interface IArgumentPattern : IService<IArgumentPattern>
    {
   
        UnaryResult<global::SharedLibrary.MyHugeResponse> Unary1(int x, int y, string z = "unknown", global::SharedLibrary.MyEnum e = SharedLibrary.MyEnum.Orange, global::SharedLibrary.MyStructResponse soho = default(global::SharedLibrary.MyStructResponse), ulong zzz = 9, global::SharedLibrary.MyRequest req = null);
   
        UnaryResult<global::SharedLibrary.MyResponse> Unary2(global::SharedLibrary.MyRequest req);
   
        UnaryResult<global::SharedLibrary.MyResponse> Unary3();
   
        UnaryResult<global::SharedLibrary.MyStructResponse> Unary5(global::SharedLibrary.MyStructRequest req);
   
        ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult1(int x, int y, string z = "unknown");
   
        ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult2(global::SharedLibrary.MyRequest req);
   
        ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult3();
   
        ServerStreamingResult<global::SharedLibrary.Nil> ServerStreamingResult4();
   
        ServerStreamingResult<global::SharedLibrary.MyStructResponse> ServerStreamingResult5(global::SharedLibrary.MyStructRequest req);
   
        UnaryResult<bool> UnaryS1(global::System.DateTime dt, global::System.DateTimeOffset dt2);
   
        UnaryResult<bool> UnaryS2(int[] arrayPattern);
   
        UnaryResult<bool> UnaryS3(int[] arrayPattern1, string[] arrayPattern2, global::SharedLibrary.MyEnum[] arrayPattern3);
    }
    
    public class IArgumentPatternClient : MagicOnionClientBase<IArgumentPattern>, IArgumentPattern
    {
        static readonly Method<byte[], byte[]> Unary1Method;
        static readonly Marshaller<DynamicArgumentTuple<int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>> Unary1RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyHugeResponse> Unary1ResponseMarshaller;

        static readonly Method<byte[], byte[]> Unary2Method;
        static readonly Marshaller<global::SharedLibrary.MyRequest> Unary2RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyResponse> Unary2ResponseMarshaller;

        static readonly Method<byte[], byte[]> Unary3Method;
        static readonly Marshaller<byte[]> Unary3RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyResponse> Unary3ResponseMarshaller;

        static readonly Method<byte[], byte[]> Unary5Method;
        static readonly Marshaller<global::SharedLibrary.MyStructRequest> Unary5RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyStructResponse> Unary5ResponseMarshaller;

        static readonly Method<byte[], byte[]> ServerStreamingResult1Method;
        static readonly Marshaller<DynamicArgumentTuple<int, int, string>> ServerStreamingResult1RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyResponse> ServerStreamingResult1ResponseMarshaller;

        static readonly Method<byte[], byte[]> ServerStreamingResult2Method;
        static readonly Marshaller<global::SharedLibrary.MyRequest> ServerStreamingResult2RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyResponse> ServerStreamingResult2ResponseMarshaller;

        static readonly Method<byte[], byte[]> ServerStreamingResult3Method;
        static readonly Marshaller<byte[]> ServerStreamingResult3RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyResponse> ServerStreamingResult3ResponseMarshaller;

        static readonly Method<byte[], byte[]> ServerStreamingResult4Method;
        static readonly Marshaller<byte[]> ServerStreamingResult4RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.Nil> ServerStreamingResult4ResponseMarshaller;

        static readonly Method<byte[], byte[]> ServerStreamingResult5Method;
        static readonly Marshaller<global::SharedLibrary.MyStructRequest> ServerStreamingResult5RequestMarshaller;
        static readonly Marshaller<global::SharedLibrary.MyStructResponse> ServerStreamingResult5ResponseMarshaller;

        static readonly Method<byte[], byte[]> UnaryS1Method;
        static readonly Marshaller<DynamicArgumentTuple<global::System.DateTime, global::System.DateTimeOffset>> UnaryS1RequestMarshaller;
        static readonly Marshaller<bool> UnaryS1ResponseMarshaller;

        static readonly Method<byte[], byte[]> UnaryS2Method;
        static readonly Marshaller<int[]> UnaryS2RequestMarshaller;
        static readonly Marshaller<bool> UnaryS2ResponseMarshaller;

        static readonly Method<byte[], byte[]> UnaryS3Method;
        static readonly Marshaller<DynamicArgumentTuple<int[], string[], global::SharedLibrary.MyEnum[]>> UnaryS3RequestMarshaller;
        static readonly Marshaller<bool> UnaryS3ResponseMarshaller;


        static IArgumentPatternClient()
        {
            Unary1Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary1", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            Unary1RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>(default(int), default(int), "unknown", SharedLibrary.MyEnum.Orange, default(global::SharedLibrary.MyStructResponse), 9, null));
            Unary1ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyHugeResponse>.Default);

            Unary2Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary2", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            Unary2RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyRequest>.Default);
            Unary2ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyResponse>.Default);

            Unary3Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary3", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            Unary3RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, byte[]>.Default);
            Unary3ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyResponse>.Default);

            Unary5Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary5", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            Unary5RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyStructRequest>.Default);
            Unary5ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyStructResponse>.Default);

            ServerStreamingResult1Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult1", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ServerStreamingResult1RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int, string>(default(int), default(int), "unknown"));
            ServerStreamingResult1ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyResponse>.Default);

            ServerStreamingResult2Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult2", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ServerStreamingResult2RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyRequest>.Default);
            ServerStreamingResult2ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyResponse>.Default);

            ServerStreamingResult3Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult3", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ServerStreamingResult3RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, byte[]>.Default);
            ServerStreamingResult3ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyResponse>.Default);

            ServerStreamingResult4Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult4", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ServerStreamingResult4RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, byte[]>.Default);
            ServerStreamingResult4ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.Nil>.Default);

            ServerStreamingResult5Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult5", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ServerStreamingResult5RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyStructRequest>.Default);
            ServerStreamingResult5ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, global::SharedLibrary.MyStructResponse>.Default);

            UnaryS1Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "UnaryS1", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            UnaryS1RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, global::System.DateTime, global::System.DateTimeOffset>(default(global::System.DateTime), default(global::System.DateTimeOffset)));
            UnaryS1ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, bool>.Default);

            UnaryS2Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "UnaryS2", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            UnaryS2RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, int[]>.Default);
            UnaryS2ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, bool>.Default);

            UnaryS3Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "UnaryS3", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            UnaryS3RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int[], string[], global::SharedLibrary.MyEnum[]>(default(int[]), default(string[]), default(global::SharedLibrary.MyEnum[])));
            UnaryS3ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, bool>.Default);

        }

        IArgumentPatternClient()
        {
        }

        public IArgumentPatternClient(Channel channel)
            : this(new DefaultCallInvoker(channel))
        {

        }

        public IArgumentPatternClient(CallInvoker callInvoker)
            : base(callInvoker)
        {
        }

        protected override MagicOnionClientBase<IArgumentPattern> Clone()
        {
            var clone = new IArgumentPatternClient(this.callInvoker);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            return clone;
        }
   
        public UnaryResult<global::SharedLibrary.MyHugeResponse> Unary1(int x, int y, string z = "unknown", global::SharedLibrary.MyEnum e = SharedLibrary.MyEnum.Orange, global::SharedLibrary.MyStructResponse soho = default(global::SharedLibrary.MyStructResponse), ulong zzz = 9, global::SharedLibrary.MyRequest req = null)
        {
            var __request = Unary1RequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>(x, y, z, e, soho, zzz, req));
            var __callResult = callInvoker.AsyncUnaryCall(Unary1Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyHugeResponse>(__callResult, Unary1ResponseMarshaller);
        }

        public UnaryResult<global::SharedLibrary.MyResponse> Unary2(global::SharedLibrary.MyRequest req)
        {
            var __request = Unary2RequestMarshaller.Serializer(req);
            var __callResult = callInvoker.AsyncUnaryCall(Unary2Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyResponse>(__callResult, Unary2ResponseMarshaller);
        }

        public UnaryResult<global::SharedLibrary.MyResponse> Unary3()
        {
            var __request = Unary3RequestMarshaller.Serializer(MagicOnionMarshallers.EmptyBytes);
            var __callResult = callInvoker.AsyncUnaryCall(Unary3Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyResponse>(__callResult, Unary3ResponseMarshaller);
        }

        public UnaryResult<global::SharedLibrary.MyStructResponse> Unary5(global::SharedLibrary.MyStructRequest req)
        {
            var __request = Unary5RequestMarshaller.Serializer(req);
            var __callResult = callInvoker.AsyncUnaryCall(Unary5Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyStructResponse>(__callResult, Unary5ResponseMarshaller);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult1(int x, int y, string z = "unknown")
        {
            var __request = ServerStreamingResult1RequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, string>(x, y, z));
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult1Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, ServerStreamingResult1ResponseMarshaller);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult2(global::SharedLibrary.MyRequest req)
        {
            var __request = ServerStreamingResult2RequestMarshaller.Serializer(req);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult2Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, ServerStreamingResult2ResponseMarshaller);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult3()
        {
            var __request = ServerStreamingResult3RequestMarshaller.Serializer(MagicOnionMarshallers.EmptyBytes);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult3Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, ServerStreamingResult3ResponseMarshaller);
        }

        public ServerStreamingResult<global::SharedLibrary.Nil> ServerStreamingResult4()
        {
            var __request = ServerStreamingResult4RequestMarshaller.Serializer(MagicOnionMarshallers.EmptyBytes);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult4Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.Nil>(__callResult, ServerStreamingResult4ResponseMarshaller);
        }

        public ServerStreamingResult<global::SharedLibrary.MyStructResponse> ServerStreamingResult5(global::SharedLibrary.MyStructRequest req)
        {
            var __request = ServerStreamingResult5RequestMarshaller.Serializer(req);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult5Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyStructResponse>(__callResult, ServerStreamingResult5ResponseMarshaller);
        }

        public UnaryResult<bool> UnaryS1(global::System.DateTime dt, global::System.DateTimeOffset dt2)
        {
            var __request = UnaryS1RequestMarshaller.Serializer(new DynamicArgumentTuple<global::System.DateTime, global::System.DateTimeOffset>(dt, dt2));
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS1Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, UnaryS1ResponseMarshaller);
        }

        public UnaryResult<bool> UnaryS2(int[] arrayPattern)
        {
            var __request = UnaryS2RequestMarshaller.Serializer(arrayPattern);
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS2Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, UnaryS2ResponseMarshaller);
        }

        public UnaryResult<bool> UnaryS3(int[] arrayPattern1, string[] arrayPattern2, global::SharedLibrary.MyEnum[] arrayPattern3)
        {
            var __request = UnaryS3RequestMarshaller.Serializer(new DynamicArgumentTuple<int[], string[], global::SharedLibrary.MyEnum[]>(arrayPattern1, arrayPattern2, arrayPattern3));
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS3Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, UnaryS3ResponseMarshaller);
        }

    }

    public interface IStandard : IService<IStandard>
    {
   
        UnaryResult<int> Unary1Async(int x, int y);
   
        ClientStreamingResult<int, string> ClientStreaming1Async();
   
        ServerStreamingResult<int> ServerStreamingAsync(int x, int y, int z);
   
        DuplexStreamingResult<int, int> DuplexStreamingAsync();
    }
    
    public class IStandardClient : MagicOnionClientBase<IStandard>, IStandard
    {
        static readonly Method<byte[], byte[]> Unary1AsyncMethod;
        static readonly Marshaller<DynamicArgumentTuple<int, int>> Unary1AsyncRequestMarshaller;
        static readonly Marshaller<int> Unary1AsyncResponseMarshaller;

        static readonly Method<byte[], byte[]> ClientStreaming1AsyncMethod;
        static readonly Marshaller<int> ClientStreaming1AsyncRequestMarshaller;
        static readonly Marshaller<string> ClientStreaming1AsyncResponseMarshaller;

        static readonly Method<byte[], byte[]> ServerStreamingAsyncMethod;
        static readonly Marshaller<DynamicArgumentTuple<int, int, int>> ServerStreamingAsyncRequestMarshaller;
        static readonly Marshaller<int> ServerStreamingAsyncResponseMarshaller;

        static readonly Method<byte[], byte[]> DuplexStreamingAsyncMethod;
        static readonly Marshaller<int> DuplexStreamingAsyncRequestMarshaller;
        static readonly Marshaller<int> DuplexStreamingAsyncResponseMarshaller;


        static IStandardClient()
        {
            Unary1AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IStandard", "Unary1Async", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            Unary1AsyncRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int>(default(int), default(int)));
            Unary1AsyncResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);

            ClientStreaming1AsyncMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IStandard", "ClientStreaming1Async", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ClientStreaming1AsyncRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);
            ClientStreaming1AsyncResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

            ServerStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IStandard", "ServerStreamingAsync", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            ServerStreamingAsyncRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int, int>(default(int), default(int), default(int)));
            ServerStreamingAsyncResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);

            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IStandard", "DuplexStreamingAsync", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            DuplexStreamingAsyncRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);
            DuplexStreamingAsyncResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);

        }

        IStandardClient()
        {
        }

        public IStandardClient(Channel channel)
            : this(new DefaultCallInvoker(channel))
        {

        }

        public IStandardClient(CallInvoker callInvoker)
            : base(callInvoker)
        {
        }

        protected override MagicOnionClientBase<IStandard> Clone()
        {
            var clone = new IStandardClient(this.callInvoker);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            return clone;
        }
   
        public UnaryResult<int> Unary1Async(int x, int y)
        {
            var __request = Unary1AsyncRequestMarshaller.Serializer(new DynamicArgumentTuple<int, int>(x, y));
            var __callResult = callInvoker.AsyncUnaryCall(Unary1AsyncMethod, base.host, base.option, __request);
            return new UnaryResult<int>(__callResult, Unary1AsyncResponseMarshaller);
        }

        public ClientStreamingResult<int, string> ClientStreaming1Async()
        {
            var __callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(ClientStreaming1AsyncMethod, base.host, base.option);
            return new ClientStreamingResult<int, string>(__callResult, ClientStreaming1AsyncRequestMarshaller, ClientStreaming1AsyncResponseMarshaller);
        }

        public ServerStreamingResult<int> ServerStreamingAsync(int x, int y, int z)
        {
            var __request = ServerStreamingAsyncRequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, int>(x, y, z));
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingAsyncMethod, base.host, base.option, __request);
            return new ServerStreamingResult<int>(__callResult, ServerStreamingAsyncResponseMarshaller);
        }

        public DuplexStreamingResult<int, int> DuplexStreamingAsync()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return new DuplexStreamingResult<int, int>(__callResult, DuplexStreamingAsyncRequestMarshaller, DuplexStreamingAsyncResponseMarshaller);
        }

    }

    public interface IMyFirstService : IService<IMyFirstService>
    {
   
        UnaryResult<string> SumAsync(int x, int y);
   
        UnaryResult<string> SumAsync2(int x, int y);
   
        ClientStreamingResult<int, string> StreamingOne();
   
        ServerStreamingResult<string> StreamingTwo(int x, int y, int z);
   
        ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999);
   
        DuplexStreamingResult<int, string> StreamingThree();
    }
    
    public class IMyFirstServiceClient : MagicOnionClientBase<IMyFirstService>, IMyFirstService
    {
        static readonly Method<byte[], byte[]> SumAsyncMethod;
        static readonly Marshaller<DynamicArgumentTuple<int, int>> SumAsyncRequestMarshaller;
        static readonly Marshaller<string> SumAsyncResponseMarshaller;

        static readonly Method<byte[], byte[]> SumAsync2Method;
        static readonly Marshaller<DynamicArgumentTuple<int, int>> SumAsync2RequestMarshaller;
        static readonly Marshaller<string> SumAsync2ResponseMarshaller;

        static readonly Method<byte[], byte[]> StreamingOneMethod;
        static readonly Marshaller<int> StreamingOneRequestMarshaller;
        static readonly Marshaller<string> StreamingOneResponseMarshaller;

        static readonly Method<byte[], byte[]> StreamingTwoMethod;
        static readonly Marshaller<DynamicArgumentTuple<int, int, int>> StreamingTwoRequestMarshaller;
        static readonly Marshaller<string> StreamingTwoResponseMarshaller;

        static readonly Method<byte[], byte[]> StreamingTwo2Method;
        static readonly Marshaller<DynamicArgumentTuple<int, int, int>> StreamingTwo2RequestMarshaller;
        static readonly Marshaller<string> StreamingTwo2ResponseMarshaller;

        static readonly Method<byte[], byte[]> StreamingThreeMethod;
        static readonly Marshaller<int> StreamingThreeRequestMarshaller;
        static readonly Marshaller<string> StreamingThreeResponseMarshaller;


        static IMyFirstServiceClient()
        {
            SumAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "SumAsync", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            SumAsyncRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int>(default(int), default(int)));
            SumAsyncResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

            SumAsync2Method = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "SumAsync2", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            SumAsync2RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int>(default(int), default(int)));
            SumAsync2ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

            StreamingOneMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IMyFirstService", "StreamingOne", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            StreamingOneRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);
            StreamingOneResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

            StreamingTwoMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IMyFirstService", "StreamingTwo", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            StreamingTwoRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int, int>(default(int), default(int), default(int)));
            StreamingTwoResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

            StreamingTwo2Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IMyFirstService", "StreamingTwo2", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            StreamingTwo2RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(new DynamicArgumentTupleFormatter<ZeroFormatter.Formatters.DefaultResolver, int, int, int>(default(int), default(int), 9999));
            StreamingTwo2ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

            StreamingThreeMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMyFirstService", "StreamingThree", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            StreamingThreeRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, int>.Default);
            StreamingThreeResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, string>.Default);

        }

        IMyFirstServiceClient()
        {
        }

        public IMyFirstServiceClient(Channel channel)
            : this(new DefaultCallInvoker(channel))
        {

        }

        public IMyFirstServiceClient(CallInvoker callInvoker)
            : base(callInvoker)
        {
        }

        protected override MagicOnionClientBase<IMyFirstService> Clone()
        {
            var clone = new IMyFirstServiceClient(this.callInvoker);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            return clone;
        }
   
        public UnaryResult<string> SumAsync(int x, int y)
        {
            var __request = SumAsyncRequestMarshaller.Serializer(new DynamicArgumentTuple<int, int>(x, y));
            var __callResult = callInvoker.AsyncUnaryCall(SumAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<string>(__callResult, SumAsyncResponseMarshaller);
        }

        public UnaryResult<string> SumAsync2(int x, int y)
        {
            var __request = SumAsync2RequestMarshaller.Serializer(new DynamicArgumentTuple<int, int>(x, y));
            var __callResult = callInvoker.AsyncUnaryCall(SumAsync2Method, base.host, base.option, __request);
            return new UnaryResult<string>(__callResult, SumAsync2ResponseMarshaller);
        }

        public ClientStreamingResult<int, string> StreamingOne()
        {
            var __callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(StreamingOneMethod, base.host, base.option);
            return new ClientStreamingResult<int, string>(__callResult, StreamingOneRequestMarshaller, StreamingOneResponseMarshaller);
        }

        public ServerStreamingResult<string> StreamingTwo(int x, int y, int z)
        {
            var __request = StreamingTwoRequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, int>(x, y, z));
            var __callResult = callInvoker.AsyncServerStreamingCall(StreamingTwoMethod, base.host, base.option, __request);
            return new ServerStreamingResult<string>(__callResult, StreamingTwoResponseMarshaller);
        }

        public ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999)
        {
            var __request = StreamingTwo2RequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, int>(x, y, z));
            var __callResult = callInvoker.AsyncServerStreamingCall(StreamingTwo2Method, base.host, base.option, __request);
            return new ServerStreamingResult<string>(__callResult, StreamingTwo2ResponseMarshaller);
        }

        public DuplexStreamingResult<int, string> StreamingThree()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(StreamingThreeMethod, base.host, base.option);
            return new DuplexStreamingResult<int, string>(__callResult, StreamingThreeRequestMarshaller, StreamingThreeResponseMarshaller);
        }

    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
