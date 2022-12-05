using MagicOnion.Client;

namespace MagicOnion.Integration.Tests;

public record TestMagicOnionClientFactory<T>(string Name, Func<MagicOnionClientOptions, T> FactoryMethod)
{
    public override string ToString() => Name;

    public T Create(ChannelBase channelBase)
        => Create(channelBase, Array.Empty<IClientFilter>());
    public T Create(ChannelBase channelBase, IEnumerable<IClientFilter> filters)
        => FactoryMethod(new MagicOnionClientOptions(channelBase.CreateCallInvoker(), string.Empty, new CallOptions(), filters.ToArray()));
}
