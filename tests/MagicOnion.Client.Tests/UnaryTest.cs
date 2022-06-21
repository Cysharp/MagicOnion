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
    public async Task ParameterlessRequestsNil()
    {
        // Arrange
        var serializedResponse = new ReadOnlyMemory<byte>();
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<Box<Nil>, Box<Nil>>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<Box<Nil>>()))
            .Returns(new AsyncUnaryCall<Box<Nil>>(Task.FromResult(Box.Create(Nil.Default)), Task.FromResult(Metadata.Empty), () => Status.DefaultSuccess, () => Metadata.Empty, () => { }))
            .Callback<Method<Box<Nil>, Box<Nil>>, string, CallOptions, Box<Nil>>((method, host, callOptions, request) =>
            {
                var serializationContext = new FakeSerializationContext();
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
        serializedResponse.ToArray().Should().BeEquivalentTo(new [] { MessagePackCode.Nil });
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

    public interface IUnaryTestService : IService<IUnaryTestService>
    {
        UnaryResult<Nil> ParameterlessReturnNil();
        UnaryResult<int> ParameterlessReturnValueType();
        UnaryResult<string> ParameterlessReturnRefType();
        UnaryResult<int> OneRefTypeParameterReturnValueType(string arg0);
        UnaryResult<int> OneValueTypeParameterReturnValueType(int arg0);
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