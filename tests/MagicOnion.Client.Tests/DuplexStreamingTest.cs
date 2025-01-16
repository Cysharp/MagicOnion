namespace MagicOnion.Client.Tests;

public class DuplexStreamingTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IDuplexStreamingTestService>(callInvokerMock);

        // Assert
        Assert.NotNull(client);
    }

    public interface IDuplexStreamingTestService : IService<IDuplexStreamingTestService>
    {
        Task<DuplexStreamingResult<int, int>> ValueTypeReturnValueType();
    }

    [Fact]
    public void UnsupportedReturnTypeNonTaskOfDuplexStreamingResult()
    {
        // Arrange
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfDuplexStreamingResultService>(callInvokerMock));
    }

    public interface IUnsupportedReturnTypeNonTaskOfDuplexStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfDuplexStreamingResultService>
    {
        DuplexStreamingResult<int, int> MethodA();
    }
    
    [Fact]
    public void MethodMustHaveNoParameter()
    {
        // Arrange
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IMethodMustHaveNoParameterService>(callInvokerMock));
    }

    public interface IMethodMustHaveNoParameterService : IService<IMethodMustHaveNoParameterService>
    {
        DuplexStreamingResult<int, int> MethodA(string arg1);
    }
}
