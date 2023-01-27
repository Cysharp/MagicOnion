using MagicOnion.Internal;

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

    [Fact]
    public async Task OneParameterValueTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<Box<int>>(new [] { Box.Create(1), Box.Create(2) });
        var callInvokerMock = new Mock<CallInvoker>();
        var arg1 = 123;
        var sendRequest = default(Box<int>);
        callInvokerMock.Setup(x => x.AsyncServerStreamingCall(It.IsAny<Method<Box<int>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<int>>()))
            .Callback<Method<Box<int>, Box<int>>, string, CallOptions, Box<int>>((method, host, callOptions, request) =>
            {
                sendRequest = request;
            })
            .Returns(() => new AsyncServerStreamingCall<Box<int>>(
                clientStreamWriterMock,
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.ValueTypeReturnValueType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(default);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(default);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(default);

        // Assert
        client.Should().NotBeNull();
        sendRequest.Should().Be(Box.Create(123));

        moveNext1.Should().BeTrue();
        current1.Should().Be(1);
        moveNext2.Should().BeTrue();
        current2.Should().Be(2);
        moveNext3.Should().BeFalse();
    }
    
    [Fact]
    public async Task OneParameterRefTypeReturnValueType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<Box<int>>(new [] { Box.Create(1), Box.Create(2) });
        var callInvokerMock = new Mock<CallInvoker>();
        var arg1 = "FooBar";
        var sendRequest = default(string);
        callInvokerMock.Setup(x => x.AsyncServerStreamingCall(It.IsAny<Method<string, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<string>()))
            .Callback<Method<string, Box<int>>, string, CallOptions, string>((method, host, callOptions, request) =>
            {
                sendRequest = request;
            })
            .Returns(() => new AsyncServerStreamingCall<Box<int>>(
                clientStreamWriterMock,
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.RefTypeReturnValueType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(default);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(default);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(default);

        // Assert
        client.Should().NotBeNull();
        sendRequest.Should().Be("FooBar");

        moveNext1.Should().BeTrue();
        current1.Should().Be(1);
        moveNext2.Should().BeTrue();
        current2.Should().Be(2);
        moveNext3.Should().BeFalse();
    }
    
    [Fact]
    public async Task OneParameterValueTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<string>(new [] { "Foo", "Bar" });
        var callInvokerMock = new Mock<CallInvoker>();
        var arg1 = 123;
        var sendRequest = default(Box<int>);
        callInvokerMock.Setup(x => x.AsyncServerStreamingCall(It.IsAny<Method<Box<int>, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<int>>()))
            .Callback<Method<Box<int>, string>, string, CallOptions, Box<int>>((method, host, callOptions, request) =>
            {
                sendRequest = request;
            })
            .Returns(() => new AsyncServerStreamingCall<string>(
                clientStreamWriterMock,
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.ValueTypeReturnRefType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(default);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(default);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(default);

        // Assert
        client.Should().NotBeNull();
        sendRequest.Should().Be(Box.Create(123));

        moveNext1.Should().BeTrue();
        current1.Should().Be("Foo");
        moveNext2.Should().BeTrue();
        current2.Should().Be("Bar");
        moveNext3.Should().BeFalse();
    }
    
    [Fact]
    public async Task OneParameterRefTypeReturnRefType()
    {
        // Arrange
        var clientStreamWriterMock = new MockAsyncStreamReader<string>(new [] { "Foo", "Bar" });
        var callInvokerMock = new Mock<CallInvoker>();
        var arg1 = "FooBar";
        var sendRequest = default(string);
        callInvokerMock.Setup(x => x.AsyncServerStreamingCall(It.IsAny<Method<string, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<string>()))
            .Callback<Method<string, string>, string, CallOptions, string>((method, host, callOptions, request) =>
            {
                sendRequest = request;
            })
            .Returns(() => new AsyncServerStreamingCall<string>(
                clientStreamWriterMock,
                _ => Task.FromResult(Metadata.Empty),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                default)
            )
            .Verifiable();
        var client = MagicOnionClient.Create<IServerStreamingTestService>(callInvokerMock.Object);

        // Act
        var result = await client.RefTypeReturnRefType(arg1);
        var moveNext1 = await result.ResponseStream.MoveNext(default);
        var current1 = result.ResponseStream.Current;
        var moveNext2 = await result.ResponseStream.MoveNext(default);
        var current2 = result.ResponseStream.Current;
        var moveNext3 = await result.ResponseStream.MoveNext(default);

        // Assert
        client.Should().NotBeNull();
        sendRequest.Should().Be("FooBar");

        moveNext1.Should().BeTrue();
        current1.Should().Be("Foo");
        moveNext2.Should().BeTrue();
        current2.Should().Be("Bar");
        moveNext3.Should().BeFalse();
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
        var callInvokerMock = new Mock<CallInvoker>();

        // Act & Assert
        var client = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IUnsupportedReturnTypeNonTaskOfServerStreamingResultService>(callInvokerMock.Object));
    }

    public interface IUnsupportedReturnTypeNonTaskOfServerStreamingResultService : IService<IUnsupportedReturnTypeNonTaskOfServerStreamingResultService>
    {
        ServerStreamingResult<int> MethodA();
    }
}
