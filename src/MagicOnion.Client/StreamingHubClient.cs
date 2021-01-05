using Grpc.Core;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        [Obsolete("Use ConnectAsync instead.")]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var client = CreateClient<TStreamingHub, TReceiver>(callInvoker, receiver, host, option, serializerOptions, logger);

            async void ConnectAndForget()
            {
                var task = client.__ConnectAndSubscribeAsync(receiver);
                try
                {
                    await task;
                }
                catch (Exception e)
                {
                    logger?.Error(e, "An error occurred while connecting to the server.");
                }
            }

            ConnectAndForget();

            return (TStreamingHub)(object)client;
        }

        public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var client = CreateClient<TStreamingHub, TReceiver>(callInvoker, receiver, host, option, serializerOptions, logger);
            await client.__ConnectAndSubscribeAsync(receiver).ConfigureAwait(false);
            return (TStreamingHub)(object)client;
        }
        
        private static StreamingHubClientBase<TStreamingHub, TReceiver> CreateClient<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host, CallOptions option, MessagePackSerializerOptions serializerOptions, IMagicOnionClientLogger logger)
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

            return client;
        }
    }

    public static class StreamingHubClientRegistry<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        public static Func<CallInvoker, TReceiver, string, CallOptions, MessagePackSerializerOptions, IMagicOnionClientLogger, TStreamingHub> consturtor;

        public static void Register(Func<CallInvoker, TReceiver, string, CallOptions, MessagePackSerializerOptions, IMagicOnionClientLogger, TStreamingHub> ctor)
        {
            consturtor = ctor;
        }
    }
}
