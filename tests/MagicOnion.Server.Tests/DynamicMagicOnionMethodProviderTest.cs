using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Binder.Internal;
using System.Reflection.Metadata;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class DynamicMagicOnionMethodProviderTest
{
    [Fact]
    public void Service_Empty()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();

        // Act
        var methods = provider.GetGrpcMethods<Service_EmptyImpl>();

        // Assert
        Assert.Empty(methods);
    }

    class Service_EmptyImpl : ServiceBase<Service_EmptyImpl.IServiceDef>, Service_EmptyImpl.IServiceDef
    {
        public interface IServiceDef : IService<IServiceDef>
        {
        }
    }

    [Fact]
    public async Task Service_Unary_Invoker_ParameterZero_NoReturnValue()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();
        var method = (IMagicOnionUnaryMethod<Service_MethodsImpl, MessagePack.Nil, MessagePack.Nil, Box<MessagePack.Nil>, Box<MessagePack.Nil>>)
            methods.Single(x => x.MethodName == nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterZero_NoReturnValue));
        var instance = new Service_MethodsImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var attributeLookup = Array.Empty<(Type, Attribute)>().ToLookup(k => k.Item1, v => v.Item2);
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, attributeLookup, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, Nil.Default);

        // Assert
        Assert.Equal(MessagePack.Nil.Default, serviceContext.Result);
    }

    [Fact]
    public async Task Service_Unary_Invoker_ParameterZero_ReturnValueValueType()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();
        var method = (IMagicOnionUnaryMethod<Service_MethodsImpl, MessagePack.Nil, int, Box<MessagePack.Nil>, Box<int>>)
            methods.Single(x => x.MethodName == nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterZero_ReturnValueValueType));
        var instance = new Service_MethodsImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var attributeLookup = Array.Empty<(Type, Attribute)>().ToLookup(k => k.Item1, v => v.Item2);
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, attributeLookup, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, Nil.Default);

        // Assert
        Assert.Equal(12345, serviceContext.Result);
    }

    [Fact]
    public async Task Service_Unary_Invoker_ParameterZero_ReturnValueRefType()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();
        var method = (IMagicOnionUnaryMethod<Service_MethodsImpl, MessagePack.Nil, string, Box<MessagePack.Nil>, string>)
            methods.Single(x => x.MethodName == nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterZero_ReturnValueRefType));
        var instance = new Service_MethodsImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var attributeLookup = Array.Empty<(Type, Attribute)>().ToLookup(k => k.Item1, v => v.Item2);
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, attributeLookup, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, Nil.Default);

        // Assert
        Assert.Equal("Hello", serviceContext.Result);
    }

    [Fact]
    public async Task Service_Unary_Invoker_ParameterMany_NoReturnValue()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();
        var method = (IMagicOnionUnaryMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, MessagePack.Nil, Box<DynamicArgumentTuple<string, int, bool>>, Box<MessagePack.Nil>>)
            methods.Single(x => x.MethodName == nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterMany_NoReturnValue));
        var instance = new Service_MethodsImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var attributeLookup = Array.Empty<(Type, Attribute)>().ToLookup(k => k.Item1, v => v.Item2);
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, attributeLookup, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, new DynamicArgumentTuple<string, int, bool>("Hello", 12345, true));

        // Assert
        Assert.Equal(MessagePack.Nil.Default, serviceContext.Result);
    }

    [Fact]
    public async Task Service_Unary_Invoker_ParameterMany_ReturnValueValueType()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();
        var method = (IMagicOnionUnaryMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, int, Box<DynamicArgumentTuple<string, int, bool>>, Box<int>>)
            methods.Single(x => x.MethodName == nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterMany_ReturnValueValueType));
        var instance = new Service_MethodsImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var attributeLookup = Array.Empty<(Type, Attribute)>().ToLookup(k => k.Item1, v => v.Item2);
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, attributeLookup, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, new DynamicArgumentTuple<string, int, bool>("Hello", 12345, true));

        // Assert
        Assert.Equal(HashCode.Combine("Hello", 12345, true), serviceContext.Result);
    }

    [Fact]
    public async Task Service_Unary_Invoker_ParameterMany_ReturnValueRefType()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();
        var method = (IMagicOnionUnaryMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, string, Box<DynamicArgumentTuple<string, int, bool>>, string>)
            methods.Single(x => x.MethodName == nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterMany_ReturnValueRefType));
        var instance = new Service_MethodsImpl();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var attributeLookup = Array.Empty<(Type, Attribute)>().ToLookup(k => k.Item1, v => v.Item2);
        var serializer = Substitute.For<IMagicOnionSerializer>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var metrics = new MagicOnionMetrics(new TestMeterFactory());
        var serviceContext = new ServiceContext(instance, method, attributeLookup, serverCallContext, serializer, metrics, NullLogger.Instance, serviceProvider);

        // Act
        await method.InvokeAsync(instance, serviceContext, new DynamicArgumentTuple<string, int, bool>("Hello", 12345, true));

        // Assert
        Assert.Equal("Hello;12345;True", serviceContext.Result);
    }

    [Fact]
    public void Service_GetGrpcMethods()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();

        // Act
        var methods = provider.GetGrpcMethods<Service_MethodsImpl>();

        // Assert
        Assert.NotEmpty(methods);
        Assert.Equal(3 + 3 + 3 + 3 + 4 + 8 + 4, methods.Count);

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, MessagePack.Nil, MessagePack.Nil, Box<MessagePack.Nil>, Box<MessagePack.Nil>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterZero_NoReturnValue);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, MessagePack.Nil, int, Box<MessagePack.Nil>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterZero_ReturnValueValueType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, MessagePack.Nil, string, Box<MessagePack.Nil>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterZero_ReturnValueRefType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, int, MessagePack.Nil, Box<int>, Box<MessagePack.Nil>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterOneValueType_NoReturnValue);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, int, int, Box<int>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterOneValueType_ReturnValueValueType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, int, string, Box<int>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterOneValueType_ReturnValueRefType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, string, MessagePack.Nil, string, Box<MessagePack.Nil>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterOneRefType_NoReturnValue);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, string, int, string, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterOneRefType_ReturnValueValueType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, string, string, string, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterOneRefType_ReturnValueRefType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, MessagePack.Nil, Box<DynamicArgumentTuple<string, int, bool>>, Box<MessagePack.Nil>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterMany_NoReturnValue);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, int, Box<DynamicArgumentTuple<string, int, bool>>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterMany_ReturnValueValueType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(IMagicOnionUnaryMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, string, Box<DynamicArgumentTuple<string, int, bool>>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.Unary_ParameterMany_ReturnValueRefType);
            var expectedMethodType = MethodType.Unary;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionClientStreamingMethod<Service_MethodsImpl, int, int, Box<int>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ClientStreaming_RequestTypeValueType_ResponseTypeValueType);
            var expectedMethodType = MethodType.ClientStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionClientStreamingMethod<Service_MethodsImpl, string, int, string, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ClientStreaming_RequestTypeRefType_ResponseTypeValueType);
            var expectedMethodType = MethodType.ClientStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionClientStreamingMethod<Service_MethodsImpl, int, string, Box<int>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ClientStreaming_RequestTypeValueType_ResponseTypeRefType);
            var expectedMethodType = MethodType.ClientStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionClientStreamingMethod<Service_MethodsImpl, string, string, string, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ClientStreaming_RequestTypeRefType_ResponseTypeRefType);
            var expectedMethodType = MethodType.ClientStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, MessagePack.Nil, int, Box<MessagePack.Nil>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterZero_ResponseTypeValueType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, MessagePack.Nil, string, Box<MessagePack.Nil>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterZero_ResponseTypeRefType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, int, int, Box<int>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterOneValueType_ResponseTypeValueType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, int, string, Box<int>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterOneValueType_ResponseTypeRefType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, string, int, string, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterOneRefType_ResponseTypeValueType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, int, Box<DynamicArgumentTuple<string, int, bool>>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterMany_ResponseTypeValueType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionServerStreamingMethod<Service_MethodsImpl, DynamicArgumentTuple<string, int, bool>, string, Box<DynamicArgumentTuple<string, int, bool>>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.ServerStreaming_ParameterMany_ResponseTypeRefType);
            var expectedMethodType = MethodType.ServerStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionDuplexStreamingMethod<Service_MethodsImpl, int, int, Box<int>, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.DuplexStreaming_RequestTypeValueType_ResponseTypeValueType);
            var expectedMethodType = MethodType.DuplexStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionDuplexStreamingMethod<Service_MethodsImpl, string, int, string, Box<int>>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.DuplexStreaming_RequestTypeRefType_ResponseTypeValueType);
            var expectedMethodType = MethodType.DuplexStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionDuplexStreamingMethod<Service_MethodsImpl, int, string, Box<int>, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.DuplexStreaming_RequestTypeValueType_ResponseTypeRefType);
            var expectedMethodType = MethodType.DuplexStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }

        {
            var expectedType = typeof(MagicOnionDuplexStreamingMethod<Service_MethodsImpl, string, string, string, string>);
            var expectedServiceMethodName = nameof(Service_MethodsImpl.IServiceDef.DuplexStreaming_RequestTypeRefType_ResponseTypeRefType);
            var expectedMethodType = MethodType.DuplexStreaming;
            AssertMethod(expectedType, expectedServiceMethodName, expectedMethodType, methods);
        }


        static void AssertMethod(Type expectedType, string expectedServiceMethodName, MethodType expectedMethodType, IReadOnlyList<IMagicOnionGrpcMethod> methods)
        {
            var expectedServiceName = nameof(Service_MethodsImpl.IServiceDef);
            var expectedServiceImplementationType = typeof(Service_MethodsImpl);
            var m = methods.SingleOrDefault(x => x.MethodName == expectedServiceMethodName);
            Assert.NotNull(m);
            Assert.IsAssignableFrom(expectedType, m);
            Assert.Equal(expectedServiceImplementationType, m.ServiceImplementationType);
            Assert.Equal(expectedServiceMethodName, m.MethodName);
            Assert.Equal(expectedServiceName, m.ServiceName);
            Assert.Equal(expectedMethodType, m.MethodType);
        }
    }

    class Service_MethodsImpl : ServiceBase<Service_MethodsImpl.IServiceDef>, Service_MethodsImpl.IServiceDef
    {
        public interface IServiceDef : IService<IServiceDef>
        {
            UnaryResult Unary_ParameterZero_NoReturnValue() => default;
            UnaryResult<int> Unary_ParameterZero_ReturnValueValueType() => UnaryResult.FromResult(12345);
            UnaryResult<string> Unary_ParameterZero_ReturnValueRefType() => UnaryResult.FromResult("Hello");

            UnaryResult Unary_ParameterOneValueType_NoReturnValue(int arg0) => default;
            UnaryResult<int> Unary_ParameterOneValueType_ReturnValueValueType(int arg0) => UnaryResult.FromResult(arg0);
            UnaryResult<string> Unary_ParameterOneValueType_ReturnValueRefType(int arg0) => UnaryResult.FromResult($"{arg0}");

            UnaryResult Unary_ParameterOneRefType_NoReturnValue(string arg0) => default;
            UnaryResult<int> Unary_ParameterOneRefType_ReturnValueValueType(string arg0) => UnaryResult.FromResult(int.Parse(arg0));
            UnaryResult<string> Unary_ParameterOneRefType_ReturnValueRefType(string arg0) => UnaryResult.FromResult($"{arg0}");

            UnaryResult Unary_ParameterMany_NoReturnValue(string arg0, int arg1, bool arg2) => default;
            UnaryResult<int> Unary_ParameterMany_ReturnValueValueType(string arg0, int arg1, bool arg2) => UnaryResult.FromResult(HashCode.Combine(arg0, arg1, arg2));
            UnaryResult<string> Unary_ParameterMany_ReturnValueRefType(string arg0, int arg1, bool arg2) => UnaryResult.FromResult($"{arg0};{arg1};{arg2}");

            Task<ClientStreamingResult<int, int>> ClientStreaming_RequestTypeValueType_ResponseTypeValueType() => throw new NotImplementedException();
            Task<ClientStreamingResult<string, int>> ClientStreaming_RequestTypeRefType_ResponseTypeValueType() => throw new NotImplementedException();
            Task<ClientStreamingResult<int, string>> ClientStreaming_RequestTypeValueType_ResponseTypeRefType() => throw new NotImplementedException();
            Task<ClientStreamingResult<string, string>> ClientStreaming_RequestTypeRefType_ResponseTypeRefType() => throw new NotImplementedException();

            Task<ServerStreamingResult<int>> ServerStreaming_ParameterZero_ResponseTypeValueType() => throw new NotImplementedException();
            Task<ServerStreamingResult<string>> ServerStreaming_ParameterZero_ResponseTypeRefType() => throw new NotImplementedException();
            Task<ServerStreamingResult<int>> ServerStreaming_ParameterOneValueType_ResponseTypeValueType(int arg0) => throw new NotImplementedException();
            Task<ServerStreamingResult<string>> ServerStreaming_ParameterOneValueType_ResponseTypeRefType(int arg0) => throw new NotImplementedException();
            Task<ServerStreamingResult<int>> ServerStreaming_ParameterOneRefType_ResponseTypeValueType(string arg0) => throw new NotImplementedException();
            Task<ServerStreamingResult<string>> ServerStreaming_ParameterOneRefType_ResponseTypeRefType(string arg0) => throw new NotImplementedException();
            Task<ServerStreamingResult<int>> ServerStreaming_ParameterMany_ResponseTypeValueType(string arg0, int arg1, bool arg2) => throw new NotImplementedException();
            Task<ServerStreamingResult<string>> ServerStreaming_ParameterMany_ResponseTypeRefType(string arg0, int arg1, bool arg2) => throw new NotImplementedException();

            Task<DuplexStreamingResult<int, int>> DuplexStreaming_RequestTypeValueType_ResponseTypeValueType() => throw new NotImplementedException();
            Task<DuplexStreamingResult<string, int>> DuplexStreaming_RequestTypeRefType_ResponseTypeValueType() => throw new NotImplementedException();
            Task<DuplexStreamingResult<int, string>> DuplexStreaming_RequestTypeValueType_ResponseTypeRefType() => throw new NotImplementedException();
            Task<DuplexStreamingResult<string, string>> DuplexStreaming_RequestTypeRefType_ResponseTypeRefType() => throw new NotImplementedException();
        }
    }

    [Fact]
    public void Service_Invalid_ReturnType()
    {
        // Arrange
        var provider = new DynamicMagicOnionMethodProvider();

        // Act
        var ex = Record.Exception(() => provider.GetGrpcMethods<Service_Invalid_ReturnType_1Impl>());
        var ex2 = Record.Exception(() => provider.GetGrpcMethods<Service_Invalid_ReturnType_2Impl>());
        var ex3 = Record.Exception(() => provider.GetGrpcMethods<Service_Invalid_ReturnType_3Impl>());
        var ex4 = Record.Exception(() => provider.GetGrpcMethods<Service_Invalid_ReturnType_4Impl>());

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.NotNull(ex2);
        Assert.IsType<InvalidOperationException>(ex2);
        Assert.NotNull(ex3);
        Assert.IsType<InvalidOperationException>(ex3);
        Assert.NotNull(ex4);
        Assert.IsType<InvalidOperationException>(ex4);
    }

    class Service_Invalid_ReturnType_1Impl : ServiceBase<Service_Invalid_ReturnType_1Impl.IServiceDef>, Service_Invalid_ReturnType_1Impl.IServiceDef
    {
        public interface IServiceDef : IService<IServiceDef>
        {
            Task MethodAsync() => throw new NotImplementedException();
        }
    }

    class Service_Invalid_ReturnType_2Impl : ServiceBase<Service_Invalid_ReturnType_2Impl.IServiceDef>, Service_Invalid_ReturnType_2Impl.IServiceDef
    {
        public interface IServiceDef : IService<IServiceDef>
        {
            ValueTask MethodAsync() => throw new NotImplementedException();
        }
    }

    class Service_Invalid_ReturnType_3Impl : ServiceBase<Service_Invalid_ReturnType_3Impl.IServiceDef>, Service_Invalid_ReturnType_3Impl.IServiceDef
    {
        public interface IServiceDef : IService<IServiceDef>
        {
            Task<int> MethodAsync() => throw new NotImplementedException();
        }
    }

    class Service_Invalid_ReturnType_4Impl : ServiceBase<Service_Invalid_ReturnType_4Impl.IServiceDef>, Service_Invalid_ReturnType_4Impl.IServiceDef
    {
        public interface IServiceDef : IService<IServiceDef>
        {
            ValueTask<int> MethodAsync() => throw new NotImplementedException();
        }
    }
}
