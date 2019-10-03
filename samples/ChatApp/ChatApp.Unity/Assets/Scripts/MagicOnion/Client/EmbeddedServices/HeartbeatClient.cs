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

        static HeartbeatClient()
        {
            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMagicOnionEmbeddedHeartbeat", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
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
            clone.resolver = this.resolver;
            return clone;
        }

        Task<DuplexStreamingResult<Nil, Nil>> IMagicOnionEmbeddedHeartbeat.Connect()
        {
            return Task.FromResult(Connect());
        }

        public DuplexStreamingResult<Nil, Nil> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return new DuplexStreamingResult<Nil, Nil>(__callResult, MessagePack.Resolvers.BuiltinResolver.Instance);
        }
    }
}
