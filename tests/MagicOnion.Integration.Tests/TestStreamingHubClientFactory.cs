using MagicOnion.Serialization;

namespace MagicOnion.Integration.Tests;

public record TestStreamingHubClientFactory<T, TReceiver>(string Name, Func<CallInvoker, TReceiver, IMagicOnionMessageSerializerProvider?, Task<T>> FactoryMethod)
{
    public override string ToString() => Name;
    public Task<T> CreateAndConnectAsync(ChannelBase channelBase, TReceiver receiver, IMagicOnionMessageSerializerProvider? messageSerializer = default)
        => FactoryMethod(channelBase.CreateCallInvoker(), receiver, messageSerializer);
}
