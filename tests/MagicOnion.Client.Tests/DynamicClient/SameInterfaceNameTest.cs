using MagicOnion.Client.DynamicClient;
using MagicOnion.Internal;
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
            var clientA = factoryA(receiverA, callInvoker, new("", default, MagicOnionSerializerProvider.Default, NullMagicOnionClientLogger.Instance));

            DynamicStreamingHubClientFactoryProvider.Instance.TryGetFactory<MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHub, MagicOnion.Client.Tests.DynamicClient.AreaB.IBazHubReceiver>(out var factoryB);
            Assert.NotNull(factoryB);
            var clientB = factoryB(receiverB, callInvoker, new ("", default, MagicOnionSerializerProvider.Default, NullMagicOnionClientLogger.Instance));
        }

        [Fact]
        public void ServiceClientDefinition_WithServiceNameAttribute_UsesDifferentPaths()
        {
            var defA = ServiceClientDefinition.CreateFromType<MagicOnion.Client.Tests.DynamicClient.AreaA.IAttributedFoo>();
            var defB = ServiceClientDefinition.CreateFromType<MagicOnion.Client.Tests.DynamicClient.AreaB.IAttributedFoo>();

            var methodA = defA.Methods.First();
            var methodB = defB.Methods.First();

            // Service names should use the [ServiceName] attribute values
            Assert.Equal("AreaA.IAttributedFoo", methodA.ServiceName);
            Assert.Equal("AreaB.IAttributedFoo", methodB.ServiceName);

            // Paths should include the distinct service name
            Assert.Equal("AreaA.IAttributedFoo/DoAsync", methodA.Path);
            Assert.Equal("AreaB.IAttributedFoo/DoAsync", methodB.Path);

            Assert.NotEqual(methodA.ServiceName, methodB.ServiceName);
            Assert.NotEqual(methodA.Path, methodB.Path);
        }

        [Fact]
        public void ServiceClientDefinition_WithoutServiceNameAttribute_UsesSameShortName()
        {
            var defA = ServiceClientDefinition.CreateFromType<MagicOnion.Client.Tests.DynamicClient.AreaA.IFoo>();
            var defB = ServiceClientDefinition.CreateFromType<MagicOnion.Client.Tests.DynamicClient.AreaB.IFoo>();

            // Without [ServiceName], both use the same short type name
            Assert.Equal("IFoo", defA.Methods.FirstOrDefault()?.ServiceName ?? ServiceNameHelper.GetServiceName(typeof(AreaA.IFoo)));
            Assert.Equal("IFoo", defB.Methods.FirstOrDefault()?.ServiceName ?? ServiceNameHelper.GetServiceName(typeof(AreaB.IFoo)));
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

    [ServiceName("AreaA.IAttributedFoo")]
    public interface IAttributedFoo : IService<IAttributedFoo>
    {
        UnaryResult<string> DoAsync();
    }
}

namespace MagicOnion.Client.Tests.DynamicClient.AreaB
{
    public interface IFoo : IService<IFoo>
    {}
    public interface IBazHub : IStreamingHub<IBazHub, IBazHubReceiver>
    {}
    public interface IBazHubReceiver
    {}

    [ServiceName("AreaB.IAttributedFoo")]
    public interface IAttributedFoo : IService<IAttributedFoo>
    {
        UnaryResult<string> DoAsync();
    }
}
