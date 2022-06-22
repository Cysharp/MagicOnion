namespace MagicOnion.Client.Tests;

public class ServerStreamingTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock.Object);

        // Assert
        client.Should().NotBeNull();
    }

    public interface IServerStreamingTestService : IService<IServerStreamingTestService>
    {
        Task<ServerStreamingResult<int>> ParameterlessReturnValueType();
        Task<ServerStreamingResult<int>> ValueTypeReturnValueType(int arg0);
    }

    [Fact]
    public void UnsupportedReturnTypeNonTaskOfServerStreamingResult()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfServerStreamingResultService>(callInvokerMock.Object));
    }

    public interface IUnsupportedReturnTypeNonTaskOfServerStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfServerStreamingResultService>
    {
        ServerStreamingResult<int> MethodA();
    }
}
