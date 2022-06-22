namespace MagicOnion.Client.Tests;

public class DuplexStreamingTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IDuplexStreamingTestService>(callInvokerMock.Object);

        // Assert
        client.Should().NotBeNull();
    }

    public interface IDuplexStreamingTestService : IService<IDuplexStreamingTestService>
    {
        Task<DuplexStreamingResult<int, int>> ValueTypeReturnValueType();
    }

    [Fact]
    public void UnsupportedReturnTypeNonTaskOfDuplexStreamingResult()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfDuplexStreamingResultService>(callInvokerMock.Object));
    }

    public interface IUnsupportedReturnTypeNonTaskOfDuplexStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfDuplexStreamingResultService>
    {
        DuplexStreamingResult<int, int> MethodA();
    }
    
    [Fact]
    public void MethodMustHaveNoParameter()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IMethodMustHaveNoParameterService>(callInvokerMock.Object));
    }

    public interface IMethodMustHaveNoParameterService : IService<IMethodMustHaveNoParameterService>
    {
        DuplexStreamingResult<int, int> MethodA(string arg1);
    }
}
