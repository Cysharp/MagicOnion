using Grpc.Core;
using Grpc.Core.Logging;
using MessagePack;
using System;

namespace MagicOnion.Client
{
    public static class StreamingHubClient
    {
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(Channel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, ILogger logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return Connect<TStreamingHub, TReceiver>(new DefaultCallInvoker(channel), receiver, host, option, serializerOptions, logger);
        }

        public static TStreamingHub Connect<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, ILogger logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var ctor = StreamingHubClientRegistry<TStreamingHub, TReceiver>.consturtor;
            StreamingHubClientBase<TStreamingHub, TReceiver> client = null;
            if (ctor == null)
            {
#if ((ENABLE_IL2CPP && !UNITY_EDITOR) || NET_STANDARD_2_0)
                throw new InvalidOperationException("Does not registered client factory, dynamic code generation is not supported on IL2CPP. Please use code generator(moc).");
#else
                var type = StreamingHubClientBuilder<TStreamingHub, TReceiver>.ClientType;
                client = (StreamingHubClientBase<TStreamingHub, TReceiver>)Activator.CreateInstance(type, new object[] { callInvoker, host, option, serializerOptions, logger });
#endif
            }
            else
            {
                client = (StreamingHubClientBase<TStreamingHub, TReceiver>)(object)ctor(callInvoker, receiver, host, option, serializerOptions, logger);
            }

            client.__ConnectAndSubscribe(receiver);
            return (TStreamingHub)(object)client;
        }
    }

    public static class StreamingHubClientRegistry<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        public static Func<CallInvoker, TReceiver, string, CallOptions, MessagePackSerializerOptions, ILogger, TStreamingHub> consturtor;

        public static void Register(Func<CallInvoker, TReceiver, string, CallOptions, MessagePackSerializerOptions, ILogger, TStreamingHub> ctor)
        {
            consturtor = ctor;
        }
    }
}
