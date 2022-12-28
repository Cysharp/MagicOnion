using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MagicOnion.Server.Tests.StreamingHubBroadcastTest;

[CollectionDefinition(nameof(StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture))]
public class StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture : ICollectionFixture<StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture.CustomServerFixture>
{
    public class CustomServerFixture : ServerFixture<StreamingHubBroadcastTestHub>
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll<IGroupRepositoryFactory>();
            services.TryAddSingleton<IGroupRepositoryFactory, ImmutableArrayGroupRepositoryFactory>();
        }
        protected override void ConfigureMagicOnion(MagicOnionOptions options)
        {
        }
    }
}

[Collection(nameof(StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture))]
public class ImmutableArrayGroupTest : GroupTestBase
{
    public ImmutableArrayGroupTest(StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture.CustomServerFixture server)
        : base(server)
    {
    }
}
