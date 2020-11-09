using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests.StreamingHubBroadcastTest
{
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
}
