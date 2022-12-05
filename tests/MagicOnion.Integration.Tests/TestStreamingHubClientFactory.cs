namespace MagicOnion.Integration.Tests;

public record TestStreamingHubClientFactory<T, TReceiver>(string Name, Func<CallInvoker, TReceiver, Task<T>> FactoryMethod)
{
    public override string ToString() => Name;
    public Task<T> CreateAndConnectAsync(ChannelBase channelBase, TReceiver receiver)
        => FactoryMethod(channelBase.CreateCallInvoker(), receiver);
}
