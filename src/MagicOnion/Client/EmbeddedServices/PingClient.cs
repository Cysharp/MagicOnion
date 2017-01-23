using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.EmbeddedServices;
using System;
using System.Threading.Tasks;
using ZeroFormatter.Formatters;
using System.Threading;

namespace MagicOnion.Client.EmbeddedServices
{
    [Ignore]
    public class PingClient : MagicOnionClientBase<IMagicOnionEmbeddedPing>, IMagicOnionEmbeddedPing
    {
        static readonly Method<byte[], byte[]> Method;
        static readonly Marshaller<DateTime> RequestMarshaller;
        static readonly Marshaller<double> ResponseMarshaller;

        static PingClient()
        {
            Method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMagicOnionEmbeddedPing", "Ping", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);
            RequestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, DateTime>.Default);
            ResponseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(Formatter<ZeroFormatter.Formatters.DefaultResolver, double>.Default);
        }

        PingClient()
        {
        }

        public PingClient(Channel channel)
            : this(new DefaultCallInvoker(channel))
        {

        }

        public PingClient(CallInvoker callInvoker)
            : base(callInvoker)
        {
        }

        protected override MagicOnionClientBase<IMagicOnionEmbeddedPing> Clone()
        {
            var clone = new PingClient(this.callInvoker);
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            return clone;
        }

        public UnaryResult<double> Ping()
        {
            return (this as IMagicOnionEmbeddedPing).Ping(DateTime.UtcNow);
        }

        UnaryResult<double> IMagicOnionEmbeddedPing.Ping(DateTime utcSendBegin)
        {
            var bytes = RequestMarshaller.Serializer(utcSendBegin);
            var __callResult = callInvoker.AsyncUnaryCall<byte[], byte[]>(Method, base.host, base.option, bytes);
            return new UnaryResult<double>(__callResult, ResponseMarshaller);
        }
    }
}