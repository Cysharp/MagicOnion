using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class MagicOnionGrpcMethodTest
{
    [Fact]
    public async Task Unary_Invoker_NoRequest_NoResponse()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var method = new MagicOnionUnaryMethod<ServiceImpl, MessagePack.Nil, Box<MessagePack.Nil>>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Unary), (instance, context, _) =>
        {
            called = true;
            invokerArgInstance = instance;
            return default;
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, Nil.Default);

        // Assert
        Assert.Equal(MessagePack.Nil.Default, serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
    }

    [Fact]
    public async Task Unary_Invoker_NoRequest_ResponseValueType()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var method = new MagicOnionUnaryMethod<ServiceImpl, MessagePack.Nil, int, Box<MessagePack.Nil>, Box<int>>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Unary_Parameterless_Int), (instance, context, _) =>
        {
            called = true;
            invokerArgInstance = instance;
            return UnaryResult.FromResult(12345);
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, Nil.Default);

        // Assert
        Assert.Equal(12345, serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
    }

    [Fact]
    public async Task Unary_Invoker_NoRequest_ResponseRefType()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var method = new MagicOnionUnaryMethod<ServiceImpl, MessagePack.Nil, string, Box<MessagePack.Nil>, string>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Unary_Parameterless_String), (instance, context, _) =>
        {
            called = true;
            invokerArgInstance = instance;
            return UnaryResult.FromResult("Hello");
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, Nil.Default);

        // Assert
        Assert.Equal("Hello", serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
    }

    [Fact]
    public async Task Unary_Invoker_RequestValueType_NoResponse()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var invokerArgRequest = default(object);
        var method = new MagicOnionUnaryMethod<ServiceImpl, int, Box<int>>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Unary_Int), (instance, context, request) =>
        {
            called = true;
            invokerArgInstance = instance;
            invokerArgRequest = request;
            return default;
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, 12345);

        // Assert
        Assert.Equal(12345, invokerArgRequest);
        Assert.Equal(MessagePack.Nil.Default, serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
    }

    [Fact]
    public async Task Unary_Invoker_RequestValueType_ResponseValueType()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var invokerArgRequest = default(object);
        var method = new MagicOnionUnaryMethod<ServiceImpl, int, int, Box<int>, Box<int>>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Unary_Int_Int), (instance, context, request) =>
        {
            called = true;
            invokerArgInstance = instance;
            invokerArgRequest = request;
            return UnaryResult.FromResult(request * 2);
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, 12345);

        // Assert
        Assert.Equal(12345, invokerArgRequest);
        Assert.Equal(12345 * 2, serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
    }

    [Fact]
    public async Task Unary_Invoker_RequestValueType_ResponseRefType()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var invokerArgRequest = default(object);
        var method = new MagicOnionUnaryMethod<ServiceImpl, int, string, Box<int>, string>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Unary_Int_String), (instance, context, request) =>
        {
            called = true;
            invokerArgInstance = instance;
            invokerArgRequest = request;
            return UnaryResult.FromResult(request.ToString());
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, 12345);

        // Assert
        Assert.Equal(12345, invokerArgRequest);
        Assert.Equal("12345", serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
    }

    [Fact]
    public async Task DuplexStreaming_Invoker_RequestValueType_ResponseValueType()
    {
        // Arrange
        var called = false;
        var invokerArgInstance = default(object);
        var requestCurrentFirst = default(object);
        var method = new MagicOnionDuplexStreamingMethod<ServiceImpl, int, int, Box<int>, Box<int>>(nameof(ServiceImpl.IMyService), nameof(ServiceImpl.IMyService.Duplex), async (instance, context) =>
        {
            called = true;
            invokerArgInstance = instance;

            var streamingContext = new DuplexStreamingContext<int, int>((StreamingServiceContext<int, int>)context);
            await streamingContext.WriteAsync(12345);
            var request = await streamingContext.MoveNext(TestContext.Current.CancellationToken);
            requestCurrentFirst = streamingContext.Current;
        });
        var instance = new ServiceImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var requestStream = Substitute.For<IAsyncStreamReader<int>>();
        requestStream.MoveNext(TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.FromResult(true));
        requestStream.Current.Returns(54321);

        var responseStream = Substitute.For<IServerStreamWriter<int>>();
        var serviceContext = new StreamingServiceContext<int, int>(instance, method, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider, requestStream, responseStream);

        // Act
        await method.InvokeAsync(instance, serviceContext);

        // Assert
        Assert.Null(serviceContext.Result);
        Assert.True(called);
        Assert.Equal(instance, invokerArgInstance);
        Assert.Equal(54321, requestCurrentFirst);
#pragma warning disable xUnit1051 // Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken
        _ = responseStream.Received(1).WriteAsync(12345);
#pragma warning restore xUnit1051 // Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken
    }


    class ServiceImpl : ServiceBase<ServiceImpl.IMyService>, ServiceImpl.IMyService
    {
        public interface IMyService : IService<IMyService>
        {
            UnaryResult Unary();
            UnaryResult Unary_Int(int arg0);
            UnaryResult Unary_String(string arg0);
            UnaryResult<int> Unary_Parameterless_Int();
            UnaryResult<string> Unary_Parameterless_String();
            UnaryResult<int> Unary_Int_Int(int arg0);
            UnaryResult<string> Unary_Int_String(int arg0);
            UnaryResult<string> Unary_String_String(string arg0);
            Task<DuplexStreamingResult<int, int>> Duplex();
        }

        public UnaryResult Unary() => default;
        public UnaryResult Unary_Int(int arg0) => default;
        public UnaryResult Unary_String(string arg0) => default;
        public UnaryResult<int> Unary_Parameterless_Int() => default;
        public UnaryResult<string> Unary_Parameterless_String() => default;
        public UnaryResult<int> Unary_Int_Int(int arg0) => UnaryResult.FromResult(0);
        public UnaryResult<string> Unary_Int_String(int arg0) => UnaryResult.FromResult("");
        public UnaryResult<string> Unary_String_String(string arg0) => UnaryResult.FromResult("");
        public Task<DuplexStreamingResult<int, int>> Duplex() => Task.FromResult(default(DuplexStreamingResult<int, int>));
    }
}
