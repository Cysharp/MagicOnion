using MagicOnion.Internal;

namespace MagicOnion.Client.Tests;

public class ClientStreamingTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock.Object);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task WriteAndCompleteArgumentValueTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<Box<int>>();
        var callInvokerMock = new Mock<CallInvoker>();
        var response = 9876;
        callInvokerMock.Setup(x => x.AsyncClientStreamingCall(It.IsAny<Method<Box<int>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>()))
            .Returns(() => new AsyncClientStreamingCall<Box<int>, Box<int>>(
                clientStreamWriterMock,
                Task.FromResult(Box.Create(response)),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.ValueTypeReturnValueType();
        await result.RequestStream.WriteAsync(123);
        await result.RequestStream.WriteAsync(456);
        await result.RequestStream.CompleteAsync();

        // Assert
        client.Should().NotBeNull();
        (await result.ResponseAsync).Should().Be(9876);
        callInvokerMock.Verify();
        clientStreamWriterMock.Completed.Should().BeTrue();
        clientStreamWriterMock.Written.Should().BeEquivalentTo(new[] { Box.Create(123), Box.Create(456) });
    }
    
    [Fact]
    public async Task WriteAndCompleteArgumentRefTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<string>();
        var callInvokerMock = new Mock<CallInvoker>();
        var response = 9876;
        callInvokerMock.Setup(x => x.AsyncClientStreamingCall(It.IsAny<Method<string, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>()))
            .Returns(() => new AsyncClientStreamingCall<string, Box<int>>(
                clientStreamWriterMock,
                Task.FromResult(Box.Create(response)),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.RefTypeReturnValueType();
        await result.RequestStream.WriteAsync("foo");
        await result.RequestStream.WriteAsync("bar");
        await result.RequestStream.CompleteAsync();

        // Assert
        client.Should().NotBeNull();
        (await result.ResponseAsync).Should().Be(9876);
        callInvokerMock.Verify();
        clientStreamWriterMock.Completed.Should().BeTrue();
        clientStreamWriterMock.Written.Should().BeEquivalentTo(new[] { "foo", "bar" });
    }

    [Fact]
    public async Task WriteAndCompleteArgumentValueTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<Box<int>>();
        var callInvokerMock = new Mock<CallInvoker>();
        var response = "OK";
        callInvokerMock.Setup(x => x.AsyncClientStreamingCall(It.IsAny<Method<Box<int>, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>()))
            .Returns(() => new AsyncClientStreamingCall<Box<int>, string>(
                clientStreamWriterMock,
                Task.FromResult(response),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.ValueTypeReturnRefType();
        await result.RequestStream.WriteAsync(123);
        await result.RequestStream.WriteAsync(456);
        await result.RequestStream.CompleteAsync();

        // Assert
        client.Should().NotBeNull();
        (await result.ResponseAsync).Should().Be("OK");
        callInvokerMock.Verify();
        clientStreamWriterMock.Completed.Should().BeTrue();
        clientStreamWriterMock.Written.Should().BeEquivalentTo(new[] { Box.Create(123), Box.Create(456) });
    }
    
    [Fact]
    public async Task WriteAndCompleteArgumentRefTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<Tuple<string, string>>();
        var callInvokerMock = new Mock<CallInvoker>();
        var response = "OK";
        callInvokerMock.Setup(x => x.AsyncClientStreamingCall(It.IsAny<Method<Tuple<string, string>, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>()))
            .Returns(() => new AsyncClientStreamingCall<Tuple<string, string>, string>(
                clientStreamWriterMock,
                Task.FromResult(response),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.RefTypeReturnRefType();
        await result.RequestStream.WriteAsync(Tuple.Create("Foo", "Bar"));
        await result.RequestStream.WriteAsync(Tuple.Create("Baz", "Hello"));
        await result.RequestStream.CompleteAsync();

        // Assert
        client.Should().NotBeNull();
        (await result.ResponseAsync).Should().Be("OK");
        callInvokerMock.Verify();
        clientStreamWriterMock.Completed.Should().BeTrue();
        clientStreamWriterMock.Written.Should().BeEquivalentTo(new[] { Tuple.Create("Foo", "Bar"), Tuple.Create("Baz", "Hello") });
    }

    public interface IClientStreamingTestService : IService<IClientStreamingTestService>
    {
        Task<ClientStreamingResult<int, int>> ValueTypeReturnValueType();
        Task<ClientStreamingResult<string, int>> RefTypeReturnValueType();
        Task<ClientStreamingResult<int, string>> ValueTypeReturnRefType();
        Task<ClientStreamingResult<Tuple<string, string>, string>> RefTypeReturnRefType();
    }

    [Fact]
    public void UnsupportedReturnTypeNonTaskOfClientStreamingResult()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfClientStreamingResultService>(callInvokerMock.Object));
    }

    public interface IUnsupportedReturnTypeNonTaskOfClientStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfClientStreamingResultService>
    {
        ClientStreamingResult<int, int> MethodA();
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
        ClientStreamingResult<int, int> MethodA(string arg1);
    }
}
