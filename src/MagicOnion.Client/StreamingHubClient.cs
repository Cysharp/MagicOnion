using Grpc.Core;
using MagicOnion.Serialization;

namespace MagicOnion.Client;

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

    public static Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(ChannelBase channel, TReceiver receiver, string? host = null, CallOptions option = default(CallOptions), IMagicOnionSerializerProvider? serializerProvider = null, IStreamingHubClientFactoryProvider? factoryProvider = null, IMagicOnionClientLogger? logger = null, CancellationToken cancellationToken = default)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        var options = StreamingHubClientOptions.CreateWithDefault(host, option, serializerProvider, logger);
        return ConnectAsync<TStreamingHub, TReceiver>(channel, receiver, options, factoryProvider, cancellationToken);
    }

    public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(ChannelBase channel, TReceiver receiver, StreamingHubClientOptions options, IStreamingHubClientFactoryProvider? factoryProvider = null, CancellationToken cancellationToken = default)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        var hubClient = await ConnectAsync<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, options, factoryProvider, cancellationToken);
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
        var options = StreamingHubClientOptions.CreateWithDefault(host, option);
        var client = CreateClient<TStreamingHub, TReceiver>(receiver, callInvoker, options, factoryProvider);

        async void ConnectAndForget()
        {
            var task = client.__ConnectAndSubscribeAsync(CancellationToken.None);
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

    public static Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, string? host = null, CallOptions option = default(CallOptions), IMagicOnionSerializerProvider? serializerProvider = null, IStreamingHubClientFactoryProvider? factoryProvider = null, IMagicOnionClientLogger? logger = null, CancellationToken cancellationToken = default)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        var options = StreamingHubClientOptions.CreateWithDefault(host, option, serializerProvider, logger);
        return ConnectAsync<TStreamingHub, TReceiver>(callInvoker, receiver, options, factoryProvider, cancellationToken);
    }

    public static async Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(CallInvoker callInvoker, TReceiver receiver, StreamingHubClientOptions options, IStreamingHubClientFactoryProvider? factoryProvider = null, CancellationToken cancellationToken = default)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        var client = CreateClient<TStreamingHub, TReceiver>(receiver, callInvoker, options, factoryProvider);
        await client.__ConnectAndSubscribeAsync(cancellationToken).ConfigureAwait(false);
        return (TStreamingHub)(object)client;
    }

    static StreamingHubClientBase<TStreamingHub, TReceiver> CreateClient<TStreamingHub, TReceiver>(TReceiver receiver, CallInvoker callInvoker, StreamingHubClientOptions options, IStreamingHubClientFactoryProvider? factoryProvider)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        factoryProvider ??= StreamingHubClientFactoryProvider.Default;

        if (!factoryProvider.TryGetFactory<TStreamingHub, TReceiver>(out var factory))
        {
            throw new NotSupportedException($"Unable to get client factory for StreamingHub type '{typeof(TStreamingHub).FullName}'.");
        }

        return (StreamingHubClientBase<TStreamingHub, TReceiver>)(object)factory(receiver, callInvoker, options);
    }
}
