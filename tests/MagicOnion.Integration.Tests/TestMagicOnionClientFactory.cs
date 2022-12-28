using MagicOnion.Client;
using MagicOnion.Serialization;

namespace MagicOnion.Integration.Tests;

public record TestMagicOnionClientFactory(string Name, IMagicOnionClientFactoryProvider FactoryProvider)
{
    public override string ToString() => Name;

    public T Create<T>(ChannelBase channelBase, IMagicOnionSerializerProvider? messageSerializer = default) where T : IService<T>
        => Create<T>(channelBase, Array.Empty<IClientFilter>(), messageSerializer);
    public T Create<T>(ChannelBase channelBase, IEnumerable<IClientFilter> filters, IMagicOnionSerializerProvider? messageSerializer = default) where T : IService<T>
        => MagicOnionClient.Create<T>(new MagicOnionClientOptions(channelBase.CreateCallInvoker(), string.Empty, new CallOptions(), filters.ToArray()), messageSerializer ?? MagicOnionSerializerProvider.Default, FactoryProvider);
}
