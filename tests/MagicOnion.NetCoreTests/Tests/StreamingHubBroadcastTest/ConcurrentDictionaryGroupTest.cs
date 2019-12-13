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
    [CollectionDefinition(nameof(StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture))]
    public class StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture : ICollectionFixture<StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture.CustomServerFixture>
    {
        public class CustomServerFixture : ServerFixture
        {
            protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            {
                options.DefaultGroupRepositoryFactory = new ConcurrentDictionaryGroupRepositoryFactory();
                return MagicOnionEngine.BuildServerServiceDefinition(new[] {typeof(StreamingHubBroadcastTestHub)}, options);
            }
        }
    }

    [Collection(nameof(StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture))]
    public class ConcurrentDictionaryGroupTest : GroupTestBase
    {
        public ConcurrentDictionaryGroupTest(StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture.CustomServerFixture server)
            : base(server)
        {
        }
    }
}
