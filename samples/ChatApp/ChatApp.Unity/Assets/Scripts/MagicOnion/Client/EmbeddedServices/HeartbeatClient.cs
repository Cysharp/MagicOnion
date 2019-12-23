using Grpc.Core;
using MagicOnion.Server.EmbeddedServices;
using MessagePack;
using System;
using System.Threading.Tasks;

#if !NON_UNITY

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedHeartbeat : IService<IMagicOnionEmbeddedHeartbeat>
    {
        Task<DuplexStreamingResult<Nil, Nil>> Connect();
    }
}

#endif

namespace MagicOnion.Client.EmbeddedServices
{
    [Ignore]
    public class HeartbeatClient : MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat>, IMagicOnionEmbeddedHeartbeat
    {
        static readonly Method<byte[], byte[]> DuplexStreamingAsyncMethod;
        static readonly MessagePackSerializerOptions BuiltinResolverOptions;

        static HeartbeatClient()
        {
            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMagicOnionEmbeddedHeartbeat", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            BuiltinResolverOptions = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.BuiltinResolver.Instance);
        }

        HeartbeatClient()
        {
        }

        public HeartbeatClient(Channel channel)
            : this(new DefaultCallInvoker(channel))
        {

        }

        public HeartbeatClient(CallInvoker callInvoker)
            : base(callInvoker, null, Array.Empty<IClientFilter>())
        {
        }

        protected override MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat> Clone()
        {
            var clone = new HeartbeatClient(this.callInvoker);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.serializerOptions = this.serializerOptions;
            return clone;
        }

        Task<DuplexStreamingResult<Nil, Nil>> IMagicOnionEmbeddedHeartbeat.Connect()
        {
            return Task.FromResult(Connect());
        }

        public DuplexStreamingResult<Nil, Nil> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return new DuplexStreamingResult<Nil, Nil>(
                __callResult,
                new MarshallingClientStreamWriter<Nil>(__callResult.RequestStream, BuiltinResolverOptions),
                new MarshallingAsyncStreamReader<Nil>(__callResult.ResponseStream, BuiltinResolverOptions),
                BuiltinResolverOptions
            );
        }
    }
}
