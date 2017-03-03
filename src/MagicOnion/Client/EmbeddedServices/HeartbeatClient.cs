using Grpc.Core;
using MagicOnion.Server.EmbeddedServices;
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
            this.option = this.option.WithHeaders(new Metadata { { ChannelContext.HeaderKey, connectionId } });
        }

        protected override MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat> Clone()
        {
            var clone = new HeartbeatClient(this.callInvoker, connectionId);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            return clone;
        }

        Task<DuplexStreamingResult<bool, bool>> IMagicOnionEmbeddedHeartbeat.Connect()
        {
            return Task.FromResult(Connect());
        }

        public DuplexStreamingResult<bool, bool> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return new DuplexStreamingResult<bool, bool>(__callResult, MessagePack.Resolvers.BuiltinResolver.Instance); // <bool> is builtin only.
        }
    }
}