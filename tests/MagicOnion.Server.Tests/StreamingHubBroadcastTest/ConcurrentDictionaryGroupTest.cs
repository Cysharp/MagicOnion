namespace MagicOnion.Server.Tests.StreamingHubBroadcastTest;

[CollectionDefinition(nameof(StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture))]
public class StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture : ICollectionFixture<StreamingHubBroadcastConcurrentDictionaryGroupTestGrpcServerFixture.CustomServerFixture>
{
    public class CustomServerFixture : ServerFixture<StreamingHubBroadcastTestHub>
    {
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
