using Grpc.Core;
using MagicOnion.Serialization;
using MessagePack;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        [Obsolete("Use ConnectAsync instead.")]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(ChannelBase channel, TReceiver receiver, string? host = null, CallOptions option = default(CallOptions), IMagicOnionSerializerProvider? serializerProvider = null, IStreamingHubClientFactoryProvider? factoryProvider = null, IMagicOnionClientLogger? logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var hubClient = Connect<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, serializerProvider, factoryProvider, logger);
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (channel is IMagicOnionAwareGrpcChannel magicOnionAwareGrpcChannel)
            {
                magicOnionAwareGrpcChannel.ManageStreamingHubClient(typeof(TStreamingHub), hubClient, hubClient.DisposeAsync, hubClient.WaitForDisconnect());
            }
            return hubClient;
        }

        public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(ChannelBase channel, TReceiver receiver, string? host = null, CallOptions option = default(CallOptions), IMagicOnionSerializerProvider? serializerProvider = null, IStreamingHubClientFactoryProvider? factoryProvider = null, IMagicOnionClientLogger? logger = null, CancellationToken cancellationToken = default)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var hubClient = await ConnectAsync<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, serializerProvider, factoryProvider, logger, cancellationToken);
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (channel is IMagicOnionAwareGrpcChannel magicOnionAwareGrpcChannel)
            {
                magicOnionAwareGrpcChannel.ManageStreamingHubClient(typeof(TStreamingHub), hubClient, hubClient.DisposeAsync, hubClient.WaitForDisconnect());
            }
            return hubClient;
        }

        [Obsolete("Use ConnectAsync instead.")]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string? host = null, CallOptions option = default(CallOptions), IMagicOnionSerializerProvider? serializerProvider = null, IStreamingHubClientFactoryProvider? factoryProvider = null, IMagicOnionClientLogger? logger = null)
             where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var client = CreateClient<TStreamingHub, TReceiver>(callInvoker, receiver, host, option, serializerProvider, factoryProvider, logger);

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

        public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string? host = null, CallOptions option = default(CallOptions), IMagicOnionSerializerProvider? serializerProvider = null, IStreamingHubClientFactoryProvider? factoryProvider = null, IMagicOnionClientLogger? logger = null, CancellationToken cancellationToken = default)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            var client = CreateClient<TStreamingHub, TReceiver>(callInvoker, receiver, host, option, serializerProvider, factoryProvider, logger);
            await client.__ConnectAndSubscribeAsync(receiver, cancellationToken).ConfigureAwait(false);
            return (TStreamingHub)(object)client;
        }

        static StreamingHubClientBase<TStreamingHub, TReceiver> CreateClient<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string? host, CallOptions option, IMagicOnionSerializerProvider? serializerProvider, IStreamingHubClientFactoryProvider? factoryProvider, IMagicOnionClientLogger? logger)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            serializerProvider ??= MagicOnionSerializerProvider.Default;
            factoryProvider ??= StreamingHubClientFactoryProvider.Default;
            logger ??= NullMagicOnionClientLogger.Instance;

            if (!factoryProvider.TryGetFactory<TStreamingHub, TReceiver>(out var factory))
            {
                throw new NotSupportedException($"Unable to get client factory for StreamingHub type '{typeof(TStreamingHub).FullName}'.");
            }

            return (StreamingHubClientBase<TStreamingHub, TReceiver>)(object)factory(callInvoker, receiver, host, option, serializerProvider, logger);
        }
    }
}
