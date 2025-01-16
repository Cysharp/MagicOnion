using MagicOnion.Internal;

namespace MagicOnion.Client.Tests;

public class ServerStreamingTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task OneParameterValueTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<Box<int>>(new [] { Box.Create(1), Box.Create(2) });
        var callInvokerMock = Substitute.For<CallInvoker>();
        var arg1 = 123;
        var sendRequest = default(Box<int>);
        callInvokerMock.AsyncServerStreamingCall(default(Method<Box<int>, Box<int>>)!, default, default, Box.Create(default(int)))
            .ReturnsForAnyArgs(x =>
            {
                var request = x.ArgAt<Box<int>>(3);
                sendRequest = request;
                return new AsyncServerStreamingCall<Box<int>>(
                    clientStreamWriterMock,
                    _ => Task.FromResult(Metadata.Empty),
                    _ => Status.DefaultSuccess,
                    _ => Metadata.Empty,
                    _ => { },
                    new object());
            });
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.ValueTypeReturnValueType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(Box.Create(123), sendRequest);

        Assert.True(moveNext1);
        Assert.Equal(1, current1);
        Assert.True(moveNext2);
        Assert.Equal(2, current2);
        Assert.False(moveNext3);
    }
    
    [Fact]
    public async Task OneParameterRefTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<Box<int>>(new [] { Box.Create(1), Box.Create(2) });
        var callInvokerMock = Substitute.For<CallInvoker>();
        var arg1 = "FooBar";
        var sendRequest = default(string);
        callInvokerMock.AsyncServerStreamingCall(default(Method<string, Box<int>>)!, default, default, default!)
            .ReturnsForAnyArgs(x =>
            {
                var request = x.ArgAt<string>(3);
                sendRequest = request;
                return new AsyncServerStreamingCall<Box<int>>(
                    clientStreamWriterMock,
                    _ => Task.FromResult(Metadata.Empty),
                    _ => Status.DefaultSuccess,
                    _ => Metadata.Empty,
                    _ => { },
                    new object());
            });
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.RefTypeReturnValueType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(client);
        Assert.Equal("FooBar", sendRequest);

        Assert.True(moveNext1);
        Assert.Equal(1, current1);
        Assert.True(moveNext2);
        Assert.Equal(2, current2);
        Assert.False(moveNext3);
    }
    
    [Fact]
    public async Task OneParameterValueTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<string>(new [] { "Foo", "Bar" });
        var callInvokerMock = Substitute.For<CallInvoker>();
        var arg1 = 123;
        var sendRequest = default(Box<int>);
        callInvokerMock.AsyncServerStreamingCall(default(Method<Box<int>, string>)!, default, default, default!)
            .ReturnsForAnyArgs(x =>
            {
                var request = x.ArgAt<Box<int>>(3);
                sendRequest = request;
                return new AsyncServerStreamingCall<string>(
                    clientStreamWriterMock,
                    _ => Task.FromResult(Metadata.Empty),
                    _ => Status.DefaultSuccess,
                    _ => Metadata.Empty,
                    _ => { },
                    new object());
            });
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.ValueTypeReturnRefType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(Box.Create(123), sendRequest);

        Assert.True(moveNext1);
        Assert.Equal("Foo", current1);
        Assert.True(moveNext2);
        Assert.Equal("Bar", current2);
        Assert.False(moveNext3);
    }
    
    [Fact]
    public async Task OneParameterRefTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<string>(new [] { "Foo", "Bar" });
        var callInvokerMock = Substitute.For<CallInvoker>();
        var arg1 = "FooBar";
        var sendRequest = default(string);
        callInvokerMock.AsyncServerStreamingCall(default(Method<string, string>)!, default, default, default!)
            .ReturnsForAnyArgs(x =>
            {
                var request = x.ArgAt<string>(3);
                sendRequest = request;
                return new AsyncServerStreamingCall<string>(
                    clientStreamWriterMock,
                    _ => Task.FromResult(Metadata.Empty),
                    _ => Status.DefaultSuccess,
                    _ => Metadata.Empty,
                    _ => { },
                    new object());
            });
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock);

        // Act
        var result = await client.RefTypeReturnRefType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(client);
        Assert.Equal("FooBar", sendRequest);

        Assert.True(moveNext1);
        Assert.Equal("Foo", current1);
        Assert.True(moveNext2);
        Assert.Equal("Bar", current2);
        Assert.False(moveNext3);
    }

    public interface IServerStreamingTestService : IService<IServerStreamingTestService>
    {
        Task<ServerStreamingResult<int>> ParameterlessReturnValueType();
        Task<ServerStreamingResult<int>> ValueTypeReturnValueType(int arg1);
        Task<ServerStreamingResult<int>> RefTypeReturnValueType(string arg1);
        Task<ServerStreamingResult<string>> ValueTypeReturnRefType(int arg1);
        Task<ServerStreamingResult<string?>> RefTypeReturnRefType(string? arg1);
    }

    [Fact]
    public void UnsupportedReturnTypeNonTaskOfServerStreamingResult()
    {
        // Arrange
        var callInvokerMock = Substitute.For<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfServerStreamingResultService>(callInvokerMock));
    }

    public interface IUnsupportedReturnTypeNonTaskOfServerStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfServerStreamingResultService>
    {
        ServerStreamingResult<int> MethodA();
    }
}
