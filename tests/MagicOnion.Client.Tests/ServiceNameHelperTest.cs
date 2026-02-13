using MagicOnion.Internal;

namespace MagicOnion.Client.Tests
{
    public class ServiceNameHelperTest
    {
        [Fact]
        public void NoAttribute_ReturnsTypeName()
        {
            var result = ServiceNameHelper.GetServiceName(typeof(INoAttributeService));
            Assert.Equal("INoAttributeService", result);
        }

        [Fact]
        public void WithAttribute_ReturnsAttributeName()
        {
            var result = ServiceNameHelper.GetServiceName(typeof(IAttributedService));
            Assert.Equal("Custom.ServiceName", result);
        }

        [Fact]
        public void SameShortName_DifferentNamespaces_DistinguishedByAttribute()
        {
            var resultA = ServiceNameHelper.GetServiceName(typeof(ServiceNameHelperAreaA.IProfileAccess));
            var resultB = ServiceNameHelper.GetServiceName(typeof(ServiceNameHelperAreaB.IProfileAccess));
            Assert.Equal("ServiceNameHelperAreaA.IProfileAccess", resultA);
            Assert.Equal("ServiceNameHelperAreaB.IProfileAccess", resultB);
            Assert.NotEqual(resultA, resultB);
        }
    }

    public interface INoAttributeService : IService<INoAttributeService>
    {
        UnaryResult<string> HelloAsync();
    }

    [ServiceName("Custom.ServiceName")]
    public interface IAttributedService : IService<IAttributedService>
    {
        UnaryResult<string> HelloAsync();
    }
}

namespace ServiceNameHelperAreaA
{
    [MagicOnion.ServiceName("ServiceNameHelperAreaA.IProfileAccess")]
    public interface IProfileAccess : MagicOnion.IService<IProfileAccess>
    {
        MagicOnion.UnaryResult<string> GetProfileAsync();
    }
}

namespace ServiceNameHelperAreaB
{
    [MagicOnion.ServiceName("ServiceNameHelperAreaB.IProfileAccess")]
    public interface IProfileAccess : MagicOnion.IService<IProfileAccess>
    {
        MagicOnion.UnaryResult<string> GetProfileAsync();
    }
}
