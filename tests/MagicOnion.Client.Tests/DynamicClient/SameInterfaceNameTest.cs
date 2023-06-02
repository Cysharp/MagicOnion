using MagicOnion.Serialization;

namespace MagicOnion.Client.Tests.DynamicClient
{
    public class SameInterfaceNameTest
    {
        [Fact]
        public void Create_MagicOnionClient()
        {
            var callInvoker = Mock.Of<CallInvoker>();
            MagicOnionClient.Create<MagicOnion.Client.Tests.DynamicClient.AreaA.IFoo>(callInvoker);
            MagicOnionClient.Create<MagicOnion.Client.Tests.DynamicClient.AreaB.IFoo>(callInvoker);
        }

        [Fact]
        public async Task DynamicStreamingHubClientFactoryProvider_TryGetFactory()
        {
            var callInvoker = Mock.Of<CallInvoker>();
            var receiverA = Mock.Of<MagicOnion.Client.Tests.DynamicClient.AreaA.IBazHubReceiver>();
            var receiverB = Mock.Of<MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHubReceiver>();

            DynamicStreamingHubClientFactoryProvider.Instance.TryGetFactory<MagicOnion.Client.Tests.DynamicClient.AreaA.IBazHub, MagicOnion.Client.Tests.DynamicClient.AreaA.IBazHubReceiver>(out var factoryA);
            var clientA = factoryA(callInvoker, receiverA, "", default, MagicOnionSerializerProvider.Default, NullMagicOnionClientLogger.Instance);

            DynamicStreamingHubClientFactoryProvider.Instance.TryGetFactory<MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHub, MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHubReceiver>(out var factoryB);
            var clientB = factoryB(callInvoker, receiverB, "", default, MagicOnionSerializerProvider.Default, NullMagicOnionClientLogger.Instance);
        }
    }
}

namespace MagicOnion.Client.Tests.DynamicClient.AreaA
{
    public interface IFoo : IService<IFoo>
    {}
    public interface IBazHub : IStreamingHub<IBazHub, IBazHubReceiver>
    {}
    public interface IBazHubReceiver
    {}
}

namespace MagicOnion.Client.Tests.DynamicClient.AreaB
{
    public interface IFoo : IService<IFoo>
    {}
    public interface IBazHub : IStreamingHub<IBazHub, IBazHubReceiver>
    {}
    public interface IBazHubReceiver
    {}
}
