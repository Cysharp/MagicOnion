using MagicOnion.Internal;

namespace MagicOnion.Client.Tests;

public class ClientStreamingTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task WriteAndCompleteArgumentValueTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<Box<int>>();
        var callInvokerMock = Substitute.For<CallInvoker>();
        var response = 9876;
        callInvokerMock.AsyncClientStreamingCall(default(Method<Box<int>, Box<int>>)!, default, default)
            .ReturnsForAnyArgs(new AsyncClientStreamingCall<Box<int>, Box<int>>(
                clientStreamWriterMock,
                Task.FromResult(Box.Create(response)),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                new object()));
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.ValueTypeReturnValueType();
        await result.RequestStream.WriteAsync(123, CancellationToken.None);
        await result.RequestStream.WriteAsync(456, CancellationToken.None);
        await result.RequestStream.CompleteAsync();

        // Assert
        Assert.NotNull(client);
        Assert.Equal(9876, (await result.ResponseAsync));
        callInvokerMock.Received();
        Assert.True(clientStreamWriterMock.Completed);
        Assert.Equal(new[] { Box.Create(123), Box.Create(456) }, clientStreamWriterMock.Written);
    }

    [Fact]
    public async Task WriteAndCompleteArgumentRefTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<string>();
        var callInvokerMock = Substitute.For<CallInvoker>();
        var response = 9876;
        callInvokerMock.AsyncClientStreamingCall(default(Method<string, Box<int>>)!, default, default)
            .ReturnsForAnyArgs(new AsyncClientStreamingCall<string, Box<int>>(
                clientStreamWriterMock,
                Task.FromResult(Box.Create(response)),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                new object()));

        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.RefTypeReturnValueType();
        await result.RequestStream.WriteAsync("foo", CancellationToken.None);
        await result.RequestStream.WriteAsync("bar", CancellationToken.None);
        await result.RequestStream.CompleteAsync();

        // Assert
        Assert.NotNull(client);
        Assert.Equal(9876, (await result.ResponseAsync));
        callInvokerMock.Received();
        Assert.True(clientStreamWriterMock.Completed);
        Assert.Equal(new[] { "foo", "bar" }, clientStreamWriterMock.Written);
    }

    [Fact]
    public async Task WriteAndCompleteArgumentValueTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<Box<int>>();
        var callInvokerMock = Substitute.For<CallInvoker>();
        var response = "OK";
        callInvokerMock.AsyncClientStreamingCall(default(Method<Box<int>, string>)!, default, default)
            .ReturnsForAnyArgs(new AsyncClientStreamingCall<Box<int>, string>(
                clientStreamWriterMock,
                Task.FromResult(response),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                new object()));
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.ValueTypeReturnRefType();
        await result.RequestStream.WriteAsync(123, CancellationToken.None);
        await result.RequestStream.WriteAsync(456, CancellationToken.None);
        await result.RequestStream.CompleteAsync();

        // Assert
        Assert.NotNull(client);
        Assert.Equal("OK", (await result.ResponseAsync));
        callInvokerMock.Received();
        Assert.True(clientStreamWriterMock.Completed);
        Assert.Equal(new[] { Box.Create(123), Box.Create(456) }, clientStreamWriterMock.Written);
    }
    
    [Fact]
    public async Task WriteAndCompleteArgumentRefTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockClientStreamWriter<Tuple<string, string>>();
        var callInvokerMock = Substitute.For<CallInvoker>();
        var response = "OK";
        callInvokerMock.AsyncClientStreamingCall(default(Method<Tuple<string, string>, string>)!, default, default)
            .ReturnsForAnyArgs(new AsyncClientStreamingCall<Tuple<string, string>, string>(
                clientStreamWriterMock,
                Task.FromResult(response),
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                new object()));
        var client = MagicOnionClient.Create<IClientStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.RefTypeReturnRefType();
        await result.RequestStream.WriteAsync(Tuple.Create("Foo", "Bar"), CancellationToken.None);
        await result.RequestStream.WriteAsync(Tuple.Create("Baz", "Hello"), CancellationToken.None);
        await result.RequestStream.CompleteAsync();

        // Assert
        Assert.NotNull(client);
        Assert.Equal("OK", (await result.ResponseAsync));
        callInvokerMock.Received();
        Assert.True(clientStreamWriterMock.Completed);
        Assert.Equal(new[] { Tuple.Create("Foo", "Bar"), Tuple.Create("Baz", "Hello") }, clientStreamWriterMock.Written);
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
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfClientStreamingResultService>(callInvokerMock));
    }

    public interface IUnsupportedReturnTypeNonTaskOfClientStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfClientStreamingResultService>
    {
        ClientStreamingResult<int, int> MethodA();
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
        ClientStreamingResult<int, int> MethodA(string arg1);
    }
}
