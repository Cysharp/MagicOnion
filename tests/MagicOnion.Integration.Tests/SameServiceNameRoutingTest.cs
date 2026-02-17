using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server;

namespace MagicOnion.Integration.Tests
{
    public class SameServiceNameRoutingTest : IClassFixture<SameServiceNameRoutingTest.SameNameServiceFactory>
    {
        readonly SameNameServiceFactory factory;

        public SameServiceNameRoutingTest(SameNameServiceFactory factory)
        {
            this.factory = factory;
        }

        public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
        {
            yield return [new TestMagicOnionClientFactory("Dynamic", DynamicMagicOnionClientFactoryProvider.Instance)];
        }

        [Theory]
        [MemberData(nameof(EnumerateMagicOnionClientFactory))]
        public async Task AreaA_Service_RoutesCorrectly(TestMagicOnionClientFactory clientFactory)
        {
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
            var client = clientFactory.Create<SameNameRouting.AreaA.IProfileAccess>(channel);
            var result = await client.GetProfileAsync();
            Assert.Equal("AreaA-Profile", result);
        }

        [Theory]
        [MemberData(nameof(EnumerateMagicOnionClientFactory))]
        public async Task AreaB_Service_RoutesCorrectly(TestMagicOnionClientFactory clientFactory)
        {
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
            var client = clientFactory.Create<SameNameRouting.AreaB.IProfileAccess>(channel);
            var result = await client.GetProfileAsync();
            Assert.Equal("AreaB-Profile", result);
        }

        [Theory]
        [MemberData(nameof(EnumerateMagicOnionClientFactory))]
        public async Task Both_Services_ReturnDifferentResults(TestMagicOnionClientFactory clientFactory)
        {
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
            var clientA = clientFactory.Create<SameNameRouting.AreaA.IProfileAccess>(channel);
            var clientB = clientFactory.Create<SameNameRouting.AreaB.IProfileAccess>(channel);

            var resultA = await clientA.GetProfileAsync();
            var resultB = await clientB.GetProfileAsync();

            Assert.Equal("AreaA-Profile", resultA);
            Assert.Equal("AreaB-Profile", resultB);
            Assert.NotEqual(resultA, resultB);
        }

        public class SameNameServiceFactory : MagicOnionApplicationFactory
        {
            protected override IEnumerable<Type> GetServiceImplementationTypes()
            {
                yield return typeof(SameNameRouting.AreaA.ProfileAccessService);
                yield return typeof(SameNameRouting.AreaB.ProfileAccessService);
            }
        }
    }
}

namespace SameNameRouting.AreaA
{
    [MagicOnion.ServiceName("SameNameRouting.AreaA.IProfileAccess")]
    public interface IProfileAccess : MagicOnion.IService<IProfileAccess>
    {
        MagicOnion.UnaryResult<string> GetProfileAsync();
    }

    public class ProfileAccessService : MagicOnion.Server.ServiceBase<IProfileAccess>, IProfileAccess
    {
        public MagicOnion.UnaryResult<string> GetProfileAsync()
            => MagicOnion.UnaryResult.FromResult("AreaA-Profile");
    }
}

namespace SameNameRouting.AreaB
{
    [MagicOnion.ServiceName("SameNameRouting.AreaB.IProfileAccess")]
    public interface IProfileAccess : MagicOnion.IService<IProfileAccess>
    {
        MagicOnion.UnaryResult<string> GetProfileAsync();
    }

    public class ProfileAccessService : MagicOnion.Server.ServiceBase<IProfileAccess>, IProfileAccess
    {
        public MagicOnion.UnaryResult<string> GetProfileAsync()
            => MagicOnion.UnaryResult.FromResult("AreaB-Profile");
    }
}
