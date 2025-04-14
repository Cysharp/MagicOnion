using MagicOnion.Client;
using MagicOnion.Serialization;

namespace MagicOnion.Integration.Tests;

public record TestStreamingHubClientFactory(string Name, IStreamingHubClientFactoryProvider FactoryProvider)
{
    public override string ToString() => Name;
    public Task<T> CreateAndConnectAsync<T, TReceiver>(ChannelBase channelBase, TReceiver receiver, IMagicOnionSerializerProvider? serializerProvider = default, CallOptions callOptions = default)
        where T : IStreamingHub<T, TReceiver>
        => StreamingHubClient.ConnectAsync<T, TReceiver>(
            channelBase.CreateCallInvoker(),
            receiver,
            serializerProvider: serializerProvider,
            factoryProvider: FactoryProvider,
            option: callOptions,
            cancellationToken: TestContext.Current.CancellationToken
        );
}
