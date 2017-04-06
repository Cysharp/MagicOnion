using System;
using Grpc.Core;
using MagicOnion.Server.EmbeddedServices;
using UniRx;
using MessagePack;

namespace MagicOnion.Client.EmbeddedServices
{
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
            clone.resolver = this.resolver;
            return clone;
        }

        public IObservable<DuplexStreamingResult<Nil, Nil>> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return Observable.Return(new DuplexStreamingResult<Nil, Nil>(__callResult, resolver));
        }
    }
}