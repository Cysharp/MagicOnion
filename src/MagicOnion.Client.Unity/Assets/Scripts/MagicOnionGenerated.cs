#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace globalname
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
            var request = SumAsyncRequestMarshaller.Serializer(new DynamicArgumentTuple<int, int>(x, y));
            var callResult = callInvoker.AsyncUnaryCall(SumAsyncMethod, host, option, request);
            return new UnaryResult<string>(callResult, SumAsyncResponseMarshaller);
        }

        public UnaryResult<string> SumAsync2(int x, int y)
        {
            var request = SumAsync2RequestMarshaller.Serializer(new DynamicArgumentTuple<int, int>(x, y));
            var callResult = callInvoker.AsyncUnaryCall(SumAsync2Method, host, option, request);
            return new UnaryResult<string>(callResult, SumAsync2ResponseMarshaller);
        }

        public ClientStreamingResult<int, string> StreamingOne()
        {
            var callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(StreamingOneMethod, host, option);
            return new ClientStreamingResult<int, string>(callResult, StreamingOneRequestMarshaller, StreamingOneResponseMarshaller);
        }

        public ServerStreamingResult<string> StreamingTwo(int x, int y, int z)
        {
            var request = StreamingTwoRequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, int>(x, y, z));
            var callResult = callInvoker.AsyncServerStreamingCall(StreamingTwoMethod, host, option, request);
            return new ServerStreamingResult<string>(callResult, StreamingTwoResponseMarshaller);
        }

        public ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999)
        {
            var request = StreamingTwo2RequestMarshaller.Serializer(new DynamicArgumentTuple<int, int, int>(x, y, z));
            var callResult = callInvoker.AsyncServerStreamingCall(StreamingTwo2Method, host, option, request);
            return new ServerStreamingResult<string>(callResult, StreamingTwo2ResponseMarshaller);
        }

        public DuplexStreamingResult<int, string> StreamingThree()
        {
            var callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(StreamingThreeMethod, host, option);
            return new DuplexStreamingResult<int, string>(callResult, StreamingThreeRequestMarshaller, StreamingThreeResponseMarshaller);
        }

    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
