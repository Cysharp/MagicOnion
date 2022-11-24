using Grpc.Core;
using MessagePack;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        [Obsolete("Use ConnectAsync instead.")]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(ChannelBase channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), IMagicOnionMessageSerializer messageSerializer = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var hubClient = Connect<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, messageSerializer, logger);
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (channel is IMagicOnionAwareGrpcChannel magicOnionAwareGrpcChannel)
            {
                magicOnionAwareGrpcChannel.ManageStreamingHubClient(typeof(TStreamingHub), hubClient, hubClient.DisposeAsync, hubClient.WaitForDisconnect());
            }
            return hubClient;
        }

        public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(ChannelBase channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), IMagicOnionMessageSerializer messageSerializer = null, IMagicOnionClientLogger logger = null, CancellationToken cancellationToken = default)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var hubClient = await ConnectAsync<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, messageSerializer, logger, cancellationToken);
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (channel is IMagicOnionAwareGrpcChannel magicOnionAwareGrpcChannel)
            {
                magicOnionAwareGrpcChannel.ManageStreamingHubClient(typeof(TStreamingHub), hubClient, hubClient.DisposeAsync, hubClient.WaitForDisconnect());
            }
            return hubClient;
        }

        [Obsolete("Use ConnectAsync instead.")]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), IMagicOnionMessageSerializer messageSerializer = null, IMagicOnionClientLogger logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var client = CreateClient<TStreamingHub, TReceiver>(callInvoker, receiver, host, option, messageSerializer, logger);

            async void ConnectAndForget()
            {
                var task = client.__ConnectAndSubscribeAsync(receiver, CancellationToken.None);
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger?.Error(e, "An error occurred while connecting to the server.");
                }
            }

            ConnectAndForget();

            return (TStreamingHub)(object)client;
        }

        public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), IMagicOnionMessageSerializer messageSerializer = null, IMagicOnionClientLogger logger = null, CancellationToken cancellationToken = default)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var client = CreateClient<TStreamingHub, TReceiver>(callInvoker, receiver, host, option, messageSerializer, logger);
            await client.__ConnectAndSubscribeAsync(receiver, cancellationToken).ConfigureAwait(false);
            return (TStreamingHub)(object)client;
        }

        static StreamingHubClientBase<TStreamingHub, TReceiver> CreateClient<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string host, CallOptions option, IMagicOnionMessageSerializer messageSerializer, IMagicOnionClientLogger logger)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            messageSerializer = messageSerializer ?? MagicOnionMessageSerializer.Default;

            var ctor = StreamingHubClientRegistry<TStreamingHub, TReceiver>.consturtor;
            StreamingHubClientBase<TStreamingHub, TReceiver> client = null;
            if (ctor == null)
            {
#if ((ENABLE_IL2CPP && !UNITY_EDITOR) || NET_STANDARD_2_0)
                throw new InvalidOperationException("Does not registered client factory, dynamic code generation is not supported on IL2CPP. Please use code generator(moc).");
#else
                var type = DynamicClient.DynamicStreamingHubClientBuilder<TStreamingHub, TReceiver>.ClientType;
                client = (StreamingHubClientBase<TStreamingHub, TReceiver>)Activator.CreateInstance(type, new object[] { callInvoker, host, option, messageSerializer, logger });
#endif
            }
            else
            {
                client = (StreamingHubClientBase<TStreamingHub, TReceiver>)(object)ctor(callInvoker, receiver, host, option, messageSerializer, logger);
            }

            return client;
        }
    }

    public static class StreamingHubClientRegistry<TStreamingHub, TReceiver>
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        public static Func<CallInvoker, TReceiver, string, CallOptions, IMagicOnionMessageSerializer, IMagicOnionClientLogger, TStreamingHub> consturtor;

        public static void Register(Func<CallInvoker, TReceiver, string, CallOptions, IMagicOnionMessageSerializer, IMagicOnionClientLogger, TStreamingHub> ctor)
        {
            consturtor = ctor;
        }
    }
}
