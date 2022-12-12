using MagicOnion.Client;
using MagicOnion.Serialization;

namespace MagicOnion.Integration.Tests;

public record TestMagicOnionClientFactory<T>(string Name, Func<MagicOnionClientOptions, IMagicOnionMessageSerializerProvider?, T> FactoryMethod)
{
    public TestMagicOnionClientFactory(string name, Func<MagicOnionClientOptions, T> factoryMethod)
        : this(name, (x, _) => factoryMethod(x))
    { }

    public override string ToString() => Name;

    public T Create(ChannelBase channelBase, IMagicOnionMessageSerializerProvider? messageSerializer = default)
        => Create(channelBase, Array.Empty<IClientFilter>(), messageSerializer);
    public T Create(ChannelBase channelBase, IEnumerable<IClientFilter> filters, IMagicOnionMessageSerializerProvider? messageSerializer = default)
        => FactoryMethod(new MagicOnionClientOptions(channelBase.CreateCallInvoker(), string.Empty, new CallOptions(), filters.ToArray()), messageSerializer);
}
