using MagicOnion.Client.DynamicClient;
using MagicOnion.Serialization;

namespace MagicOnion.Client.Tests.DynamicClient
{
    public class SameInterfaceNameTest
    {
        [Fact]
        public void Create_MagicOnionClient()
        {
            var callInvoker = Substitute.For<CallInvoker>();
            MagicOnionClient.Create<MagicOnion.Client.Tests.DynamicClient.AreaA.IFoo>(callInvoker);
            MagicOnionClient.Create<MagicOnion.Client.Tests.DynamicClient.AreaB.IFoo>(callInvoker);
        }

        [Fact]
        public void DynamicStreamingHubClientFactoryProvider_TryGetFactory()
        {
            var callInvoker = Substitute.For<CallInvoker>();
            var receiverA = Substitute.For<MagicOnion.Client.Tests.DynamicClient.AreaA.IBazHubReceiver>();
            var receiverB = Substitute.For<MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHubReceiver>();

            DynamicStreamingHubClientFactoryProvider.Instance.TryGetFactory<MagicOnion.Client.Tests.DynamicClient.AreaA.IBazHub, MagicOnion.Client.Tests.DynamicClient.AreaA.IBazHubReceiver>(out var factoryA);
            Assert.NotNull(factoryA);
            var clientA = factoryA(callInvoker, receiverA, "", default, MagicOnionSerializerProvider.Default, NullMagicOnionClientLogger.Instance);

            DynamicStreamingHubClientFactoryProvider.Instance.TryGetFactory<MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHub, MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHubReceiver>(out var factoryB);
            Assert.NotNull(factoryB);
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
