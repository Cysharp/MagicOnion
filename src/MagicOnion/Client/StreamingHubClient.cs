using Grpc.Core;
using Grpc.Core.Logging;
using MessagePack;
using System;

namespace MagicOnion.Client
{
    public static class StreamingHubClient
    {
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(Channel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), IFormatterResolver resolver = null, ILogger logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return Connect<TStreamingHub, TReceiver>(new DefaultCallInvoker(channel), receiver, host, option, resolver, logger);
        }

        public static TStreamingHub Connect<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), IFormatterResolver resolver = null, ILogger logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var type = StreamingHubClientBuilder<TStreamingHub, TReceiver>.ClientType;
            var client = (StreamingHubClientBase<TStreamingHub, TReceiver>)Activator.CreateInstance(type, new object[] { callInvoker, host, option, resolver, logger });
            client.__ConnectAndSubscribe(receiver);
            return (TStreamingHub)(object)client;
        }
    }
}
