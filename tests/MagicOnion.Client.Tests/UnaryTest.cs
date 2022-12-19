using MagicOnion.Internal;

namespace MagicOnion.Client.Tests;

public class UnaryTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task Clone_WithOptions()
    {
        // Arrange
        var actualCallOptions = default(CallOptions);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<Nil>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<Nil>, Box<Nil>>, string, CallOptions, Box<Nil>>((method, host, callOptions, request) =>
            {
                actualCallOptions = callOptions;
            })
            .Verifiable();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);

        // Act
        client = client.WithOptions(new CallOptions(new Metadata() { { "foo", "bar" } }));
        await client.ParameterlessNonGenericReturnType();

        // Assert
        client.Should().NotBeNull();
        callInvokerMock.Verify();
        actualCallOptions.Headers.Should().Contain(x => x.Key == "foo" && x.Value == "bar");
    }

    [Fact]
    public async Task Clone_WithHost()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.SetReturnsDefault(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }));
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), "www.example.com", It.IsAny<CallOptions>(), It.IsAny<Box<Nil>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);

        // Act
        client = client.WithHost("www.example.com");
        await client.ParameterlessNonGenericReturnType();

        // Assert
        client.Should().NotBeNull();
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task Clone_WithCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var actualCancellationToken = default(CancellationToken);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.SetReturnsDefault(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }));
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<Nil>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<Nil>, Box<Nil>>, string, CallOptions, Box<Nil>>((method, host, callOptions, request) =>
            {
                actualCancellationToken = callOptions.CancellationToken;
            })
            .Verifiable();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);

        // Act
        client = client.WithCancellationToken(cts.Token);
        await client.ParameterlessNonGenericReturnType();

        // Assert
        client.Should().NotBeNull();
        callInvokerMock.Verify();
        actualCancellationToken.Should().Be(cts.Token);
    }
    
    [Fact]
    public async Task Clone_WithHeaders()
    {
        // Arrange
        var actualHeaders = default(Metadata);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.SetReturnsDefault(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }));
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<Nil>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<Nil>, Box<Nil>>, string, CallOptions, Box<Nil>>((method, host, callOptions, request) =>
            {
                actualHeaders = callOptions.Headers;
            })
            .Verifiable();
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);

        // Act
        client = client.WithHeaders(new Metadata() { { "foo", "bar" }});
        await client.ParameterlessNonGenericReturnType();

        // Assert
        client.Should().NotBeNull();
        callInvokerMock.Verify();
        actualHeaders.Should().Contain(x => x.Key == "foo" && x.Value == "bar");
    }

    [Fact]
    public async Task ParameterlessRequestsNil()
    {
        // Arrange
        var serializedResponse = new ReadOnlyMemory<byte>();
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<Nil>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<Nil>, Box<Nil>>, string, CallOptions, Box<Nil>>((method, host, callOptions, request) =>
            {
                var serializationContext = new MockSerializationContext();
                method.RequestMarshaller.ContextualSerializer(Box.Create(Nil.Default), serializationContext);
                serializedResponse = serializationContext.ToMemory();
            })
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.ParameterlessReturnNil();

        // Assert
        result.Should().Be(Nil.Default);
        callInvokerMock.Verify();
        serializedResponse.ToArray().Should().BeEquivalentTo(new[] { MessagePackCode.Nil });
    }

    [Fact]
    public async Task ParameterlessReturnNil()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.Is<Box<Nil>>(x => x.Value.Equals(Nil.Default))))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.ParameterlessReturnNil();

        // Assert
        result.Should().Be(Nil.Default);
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task ParameterlessReturnValueType()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.Is<Box<Nil>>(x => x.Value.Equals(Nil.Default))))
            .Returns(new AsyncUnaryCall<Box<int>>(Task.FromResult(Box.Create(123)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.ParameterlessReturnValueType();

        // Assert
        result.Should().Be(123);
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task ParameterlessReturnRefType()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.Is<Box<Nil>>(x => x.Value.Equals(Nil.Default))))
            .Returns(new AsyncUnaryCall<string>(Task.FromResult("FooBar"), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.ParameterlessReturnRefType();

        // Assert
        result.Should().Be("FooBar");
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task OneRefTypeParameterReturnValueType()
    {
        // Arrange
        var request = "RequestValue";
        var response = 123;
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<string, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), request))
            .Returns(new AsyncUnaryCall<Box<int>>(Task.FromResult(Box.Create(response)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.OneRefTypeParameterReturnValueType(request);

        // Assert
        result.Should().Be(123);
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task OneValueTypeParameterReturnValueType()
    {
        // Arrange
        var request = 123;
        var response = 456;
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<int>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), Box.Create(request)))
            .Returns(new AsyncUnaryCall<Box<int>>(Task.FromResult(Box.Create(response)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.OneValueTypeParameterReturnValueType(request);

        // Assert
        result.Should().Be(456);
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task OneRefTypeParameterReturnRefType()
    {
        // Arrange
        var request = "RequestValue";
        var response = "Ok";
        var sentRequest = default(string);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<string, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), request))
            .Returns(new AsyncUnaryCall<string>(Task.FromResult(response), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<string, string>, string, CallOptions, string>((method, host, options, request) => sentRequest = request)
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.OneRefTypeParameterReturnRefType(request);

        // Assert
        result.Should().Be("Ok");
        callInvokerMock.Verify();
        sentRequest.Should().Be("RequestValue");
    }

    [Fact]
    public async Task OneValueTypeParameterReturnRefType()
    {
        // Arrange
        var request = 123;
        var response = "OK";
        var sentRequest = default(Box<int>);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<int>, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), Box.Create(request)))
            .Returns(new AsyncUnaryCall<string>(Task.FromResult(response), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<int>, string>, string, CallOptions, Box<int>>((method, host, options, request) => sentRequest = request)
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.OneValueTypeParameterReturnRefType(request);

        // Assert
        result.Should().Be("OK");
        callInvokerMock.Verify();
        (sentRequest?.Value).Should().Be(123);
    }

    [Fact]
    public async Task TwoParametersReturnRefType()
    {
        // Arrange
        var requestArg1 = 123;
        var requestArg2 = "Foo";
        var response = "OK";
        var sentRequest = default(Box<DynamicArgumentTuple<int, string>>);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<DynamicArgumentTuple<int, string>>, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<DynamicArgumentTuple<int, string>>>()))
            .Returns(new AsyncUnaryCall<string>(Task.FromResult(response), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<DynamicArgumentTuple<int, string>>, string>, string, CallOptions, Box<DynamicArgumentTuple<int, string>>>((method, host, options, request) => sentRequest = request)
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.TwoParametersReturnRefType(requestArg1, requestArg2);

        // Assert
        result.Should().Be("OK");
        callInvokerMock.Verify();
        (sentRequest?.Value.Item1).Should().Be(123);
        (sentRequest?.Value.Item2).Should().Be("Foo");
    }

    [Fact]
    public async Task TwoParametersReturnValueType()
    {
        // Arrange
        var requestArg1 = 123;
        var requestArg2 = "Foo";
        var response = 987;
        var sentRequest = default(Box<DynamicArgumentTuple<int, string>>);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<DynamicArgumentTuple<int, string>>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<DynamicArgumentTuple<int, string>>>()))
            .Returns(new AsyncUnaryCall<Box<int>>(Task.FromResult(Box.Create(response)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<DynamicArgumentTuple<int, string>>, Box<int>>, string, CallOptions, Box<DynamicArgumentTuple<int, string>>>((method, host, options, request) => sentRequest = request)
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await client.TwoParametersReturnValueType(requestArg1, requestArg2);

        // Assert
        result.Should().Be(987);
        callInvokerMock.Verify();
        (sentRequest?.Value.Item1).Should().Be(123);
        (sentRequest?.Value.Item2).Should().Be("Foo");
    }

    [Fact]
    public async Task ParameterlessNonGenericReturnType()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.Is<Box<Nil>>(x => x.Value.Equals(Nil.Default))))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        await client.ParameterlessNonGenericReturnType();

        // Assert
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task OneRefTypeParameterNonGenericReturnType()
    {
        // Arrange
        var request = "RequestValue";
        var response = 123;
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<string, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), request))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        await client.OneRefTypeParameterNonGenericReturnType(request);

        // Assert
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task OneValueTypeParameterNonGenericReturnType()
    {
        // Arrange
        var request = 123;
        var response = 456;
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<int>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), Box.Create(request)))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        await client.OneValueTypeParameterNonGenericReturnType(request);

        // Assert
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task TwoParametersNonGenericReturnType()
    {
        // Arrange
        var requestArg1 = 123;
        var requestArg2 = "Foo";
        var response = 987;
        var sentRequest = default(Box<DynamicArgumentTuple<int, string>>);
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<DynamicArgumentTuple<int, string>>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<DynamicArgumentTuple<int, string>>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<DynamicArgumentTuple<int, string>>, Box<Nil>>, string, CallOptions, Box<DynamicArgumentTuple<int, string>>>((method, host, options, request) => sentRequest = request)
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        await client.TwoParametersNonGenericReturnType(requestArg1, requestArg2);

        // Assert
        callInvokerMock.Verify();
        (sentRequest?.Value.Item1).Should().Be(123);
        (sentRequest?.Value.Item2).Should().Be("Foo");
    }

    [Fact]
    public async Task ThrowsResponseHeaders()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.Is<Box<Nil>>(x => x.Value.Equals(Nil.Default))))
            .Returns(new AsyncUnaryCall<Box<int>>(
                Task.FromException<Box<int>>(new RpcException(new Status(StatusCode.Unknown, "Faulted"), "Faulted")),
                Task.FromException<Metadata>(new RpcException(new Status(StatusCode.Unknown, "FaultedOnResponseHeaders"), "FaultedOnResponseHeaders")),
                () => Status.DefaultSuccess,
                () => Metadata.Empty,
                () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await Assert.ThrowsAsync<RpcException>(async () => await client.ParameterlessReturnValueType().ResponseHeadersAsync);

        // Assert
        result.StatusCode.Should().Be(StatusCode.Unknown);
        result.Message.Should().Be("FaultedOnResponseHeaders");
        callInvokerMock.Verify();
    }

    [Fact]
    public async Task ThrowsResponse()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<int>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.Is<Box<Nil>>(x => x.Value.Equals(Nil.Default))))
            .Returns(new AsyncUnaryCall<Box<int>>(Task.FromException<Box<int>>(new RpcException(new Status(StatusCode.Unknown, "Faulted"), "Faulted")), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Verifiable();

        // Act
        var client = MagicOnionClient.Create<IUnaryTestService>(callInvokerMock.Object);
        var result = await Assert.ThrowsAsync<RpcException>(async () => await client.ParameterlessReturnValueType());

        // Assert
        result.StatusCode.Should().Be(StatusCode.Unknown);
        result.Message.Should().Be("Faulted");
        callInvokerMock.Verify();
    }

    public interface IUnaryTestService : IService<IUnaryTestService>
    {
        UnaryResult<Nil> ParameterlessReturnNil();
        UnaryResult<int> ParameterlessReturnValueType();
        UnaryResult<string> ParameterlessReturnRefType();
        UnaryResult<int> OneRefTypeParameterReturnValueType(string arg1);
        UnaryResult<int> OneValueTypeParameterReturnValueType(int arg1);
        UnaryResult<string> OneRefTypeParameterReturnRefType(string arg1);
        UnaryResult<string> OneValueTypeParameterReturnRefType(int arg1);
        UnaryResult<int> TwoParametersReturnValueType(int arg1, string arg2);
        UnaryResult<string> TwoParametersReturnRefType(int arg1, string arg2);

        UnaryResult ParameterlessNonGenericReturnType();
        UnaryResult OneRefTypeParameterNonGenericReturnType(string arg1);
        UnaryResult OneValueTypeParameterNonGenericReturnType(int arg1);
        UnaryResult TwoParametersNonGenericReturnType(int arg1, string arg2);
    }

    [Fact]
    public void MaxParameters()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act
        var client = MagicOnionClient.Create<IMaxParametersService>(callInvokerMock.Object);

        // Assert
        client.Should().NotBeNull();
    }

    public interface IMaxParametersService : IService<IMaxParametersService>
    {
        UnaryResult<int> ManyParameterReturnValueType(string arg1, bool arg2, long arg3, uint arg4, char arg5, byte arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15);
    }

    [Fact]
    public void TooManyParameters()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act / Assert
        Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<ITooManyParametersService>(callInvokerMock.Object));
    }

    public interface ITooManyParametersService : IService<ITooManyParametersService>
    {
        UnaryResult<int> AMethod(string arg1, bool arg2, long arg3, uint arg4, char arg5, byte arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15, int arg16 /* T16 */);
    }

    [Fact]
    public void ReturnTaskOfUnaryResult()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();

        // Act / Assert
        Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IReturnTaskOfUnaryResultService>(callInvokerMock.Object));
    }

    public interface IReturnTaskOfUnaryResultService : IService<IReturnTaskOfUnaryResultService>
    {
        Task<UnaryResult<int>> AMethod();
    }
}
