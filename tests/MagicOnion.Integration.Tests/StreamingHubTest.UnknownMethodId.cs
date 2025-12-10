using Grpc.Net.Client;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Integration.Tests
{
    public partial class StreamingHubTest
    {
        [Theory]
        [MemberData(nameof(EnumerateStreamingHubClientFactory))]
        public async Task UnknownMethodId(TestStreamingHubClientFactory clientFactory)
        {
            // Arrange
            var httpClient = factory.CreateDefaultClient();
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

            var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
            var client = await clientFactory.CreateAndConnectAsync<MagicOnion.Integration.Tests.UnknownMethodIdTest.IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

            // Act
            var ex = await Record.ExceptionAsync(() => client.CustomMethodId().WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken));

            // Assert
            Assert.NotNull(ex);
            var rpcException = Assert.IsType<RpcException>(ex);
            Assert.Equal(StatusCode.Unimplemented, rpcException.Status.StatusCode);
            Assert.Contains(factory.Logs.GetSnapshot(), x =>  x.Id.Name == "HubMethodNotFound" && x.Message.Contains("StreamingHub method '-1'"));
        }
    }

}

namespace MagicOnion.Integration.Tests.UnknownMethodIdTest
{
    public interface IStreamingHubTestHub : IStreamingHub<IStreamingHubTestHub, IStreamingHubTestHubReceiver>
    {
        [MethodId(-1)]
        Task CustomMethodId();
    }
}
