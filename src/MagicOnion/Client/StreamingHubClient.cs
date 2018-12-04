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
#if NON_UNITY
            var client = (StreamingHubClientBase<TStreamingHub, TReceiver>)Activator.CreateInstance(type, new object[] { callInvoker, host, option, resolver, logger });
#else
            var client = (StreamingHubClientBase<TStreamingHub, TReceiver>)(object)StreamingHubClientRegistry<TStreamingHub, TReceiver>.Create(callInvoker, receiver, host, option, resolver, logger);
#endif
            client.__ConnectAndSubscribe(receiver);
            return (TStreamingHub)(object)client;
        }
    }

    public static class StreamingHubClientRegistry<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        static Func<CallInvoker, TReceiver, string, CallOptions, IFormatterResolver, ILogger, TStreamingHub> consturtor;

        public static void Register(Func<CallInvoker, TReceiver, string, CallOptions, IFormatterResolver, ILogger, TStreamingHub> ctor)
        {
            consturtor = ctor;
        }

        public static TStreamingHub Create(CallInvoker callInvoker, TReceiver receiver, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
        {
            return consturtor(callInvoker, receiver, host, option, resolver, logger);
        }
    }
}
