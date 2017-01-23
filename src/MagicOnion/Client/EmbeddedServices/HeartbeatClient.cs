using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.EmbeddedServices;
using System;
using System.Threading.Tasks;
using ZeroFormatter.Formatters;

namespace MagicOnion.Client.EmbeddedServices
{
    [Ignore]
    public class HeartbeatClient : MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat>, IMagicOnionEmbeddedHeartbeat
    {
        static readonly Method<byte[], byte[]> DuplexStreamingAsyncMethod;
        static readonly Marshaller<bool> DuplexStreamingAsyncRequestMarshaller;
        static readonly Marshaller<bool> DuplexStreamingAsyncResponseMarshaller;
        readonly string connectionId;

        static HeartbeatClient()
        {
            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMagicOnionEmbeddedHeartbeat", "Connect", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            DuplexStreamingAsyncRequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, bool>.Default);
            DuplexStreamingAsyncResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, bool>.Default);
        }

        HeartbeatClient()
        {
        }

        public HeartbeatClient(Channel channel, string connectionId)
            : this(new DefaultCallInvoker(channel), connectionId)
        {

        }

        public HeartbeatClient(CallInvoker callInvoker, string connectionId)
            : base(callInvoker)
        {
            this.connectionId = connectionId;
            this.option = this.option.WithHeaders(new Metadata { { ConnectionContext.HeaderKey, connectionId } });
        }

        protected override MagicOnionClientBase<IMagicOnionEmbeddedHeartbeat> Clone()
        {
            var clone = new HeartbeatClient(this.callInvoker, connectionId);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            return clone;
        }

        public Task<DuplexStreamingResult<bool, bool>> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return Task.FromResult(new DuplexStreamingResult<bool, bool>(__callResult, DuplexStreamingAsyncRequestMarshaller, DuplexStreamingAsyncResponseMarshaller));
        }
    }
}