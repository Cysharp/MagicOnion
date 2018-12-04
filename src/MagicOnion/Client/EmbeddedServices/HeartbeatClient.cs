#if NON_UNITY

using Grpc.Core;
using MagicOnion.Server.EmbeddedServices;
using MessagePack;
using System.Threading.Tasks;

namespace MagicOnion.Client.EmbeddedServices
{
    [Ignore]
    public class HeartbeatClient : MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat>, IMagicOnionEmbeddedHeartbeat
    {
        static readonly Method<byte[], byte[]> DuplexStreamingAsyncMethod;
        readonly string connectionId;

        static HeartbeatClient()
        {
            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMagicOnionEmbeddedHeartbeat", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        HeartbeatClient()
        {
        }

        public HeartbeatClient(Channel channel, string connectionId)
            : this(new DefaultCallInvoker(channel), connectionId)
        {

        }

        public HeartbeatClient(CallInvoker callInvoker, string connectionId)
            : base(callInvoker, null)
        {
            this.connectionId = connectionId;
#pragma warning disable CS0618
            this.option = this.option.WithHeaders(new Metadata { { ChannelContext.HeaderKey, connectionId } });
#pragma warning restore CS0618
        }

        protected override MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat> Clone()
        {
            var clone = new HeartbeatClient(this.callInvoker, connectionId);
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

#endif