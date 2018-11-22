using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace MagicOnion.Client
{
    public static class StreamingHubClient
    {
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var type = StreamingHubClientBuilder<TStreamingHub, TReceiver>.ClientType;
            var client = (StreamingHubClientBase<TStreamingHub, TReceiver>)Activator.CreateInstance(type, new object[] { callInvoker, host, option, resolver, logger });
            client.__ConnectAndSubscribe(receiver);
            return (TStreamingHub)(object)client;
        }

    }
}
