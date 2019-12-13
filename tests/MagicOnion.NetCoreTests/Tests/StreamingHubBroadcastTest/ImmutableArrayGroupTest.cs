using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MagicOnion.Tests;
using Xunit;

namespace MagicOnion.NetCoreTests.Tests.StreamingHubBroadcastTest
{
    [CollectionDefinition(nameof(StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture))]
    public class StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture : ICollectionFixture<StreamingHubBroadcastImmutableArrayGroupTestGrpcServerFixture.CustomServerFixture>
    {
        public class CustomServerFixture : ServerFixture
        {
            protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            {
                options.DefaultGroupRepositoryFactory = new ImmutableArrayGroupRepositoryFactory();
                return MagicOnionEngine.BuildServerServiceDefinition(new[] {typeof(StreamingHubBroadcastTestHub)}, options);
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
