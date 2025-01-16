using MagicOnion.Internal;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Internal;
using MessagePack;

namespace MagicOnion.Server.Tests;

public class StreamingHubMethodHandlerMetadataFactoryTest
{
    [Fact]
    public void MethodId_Default()
    {
        // Arrange
        var type = typeof(MyHub_MethodId);
        var methodInfo = type.GetMethod(nameof(IMyHub_MethodId.Method_Default))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(FNV1A32.GetHashCode(nameof(IMyHub_MethodId.Method_Default)), metadata.MethodId);
    }

    [Fact]
    public void MethodId_Attribute()
    {
        // Arrange
        var type = typeof(MyHub_MethodId);
        var methodInfo = type.GetMethod(nameof(IMyHub_MethodId.Method_WithAttribute))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(12345, metadata.MethodId);
    }

    [Fact]
    public void MethodId_Invalid_AttributesOnImplementation()
    {
        // Arrange
        var type = typeof(MyHub_MethodId);
        var methodInfo = type.GetMethod(nameof(IMyHub_MethodId.Invalid_Method_WithAttributeOnImpl))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo));

        // Assert
        Assert.NotNull(ex);
    }

    [Fact]
    public void Invalid_Returns_Int()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Invalid_Method_Returns_Int))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Invalid_Returns_Generic()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Invalid_Method_Generic))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Parameterless_WithoutReturnValue()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_Task))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_Task)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Empty(metadata.Parameters);
        Assert.Equal(typeof(Nil), metadata.RequestType);
        Assert.Null(metadata.ResponseType);
    }

    [Fact]
    public void Parameterless_ReturnValue()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_TaskOfValue))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_TaskOfValue)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Empty(metadata.Parameters);
        Assert.Equal(typeof(Nil), metadata.RequestType);
        Assert.Equal(typeof(int), metadata.ResponseType);
    }

    [Fact]
    public void OneParameter()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_OneParameter))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_OneParameter)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Equal(1, metadata.Parameters.Count());
        Assert.Equal(typeof(int), metadata.RequestType);
        Assert.Null(metadata.ResponseType);
    }

    [Fact]
    public void TwoParameters()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_TwoParameters))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_TwoParameters)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Equal(2, metadata.Parameters.Count());
        Assert.Equal(typeof(DynamicArgumentTuple<int, string>), metadata.RequestType);
        Assert.Null(metadata.ResponseType);
    }

    [Fact]
    public void AttributeLookup_None()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookup.Attribute_None))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Empty(metadata.AttributeLookup);
    }

    [Fact]
    public void AttributeLookup_One()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookup.Attribute_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(1, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyFirstAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyFirstAttribute)].Count());
    }

    [Fact]
    public void AttributeLookup_Many()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookup.Attribute_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(2, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyFirstAttribute), typeof(MySecondAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyFirstAttribute)].Count());
        Assert.Equal(1, metadata.AttributeLookup[typeof(MySecondAttribute)].Count());
    }

    [Fact]
    public void AttributeLookup_Many_Multiple()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookup.Attribute_Many_Multiple))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(2, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyFirstAttribute), typeof(MySecondAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyFirstAttribute)].Count());
        Assert.Equal(3, metadata.AttributeLookup[typeof(MySecondAttribute)].Count());
        Assert.Equal([new MySecondAttribute(0), new MySecondAttribute(1), new MySecondAttribute(2)], metadata.AttributeLookup[typeof(MySecondAttribute)]);
    }

    [Fact]
    public void AttributeLookup_Class_None()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookupWithClassAttriubte);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookupWithClassAttriubte.Attribute_None))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(1, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyThirdAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyThirdAttribute)].Count());
    }

    [Fact]
    public void AttributeLookup_Class_One()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookupWithClassAttriubte);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookupWithClassAttriubte.Attribute_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(2, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyThirdAttribute), typeof(MyFirstAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyThirdAttribute)].Count());
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyFirstAttribute)].Count());
    }

    [Fact]
    public void AttributeLookup_Class_Many()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookupWithClassAttriubte);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookupWithClassAttriubte.Attribute_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(3, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyThirdAttribute), typeof(MyFirstAttribute), typeof(MySecondAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyThirdAttribute)].Count());
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyFirstAttribute)].Count());
        Assert.Equal(1, metadata.AttributeLookup[typeof(MySecondAttribute)].Count());
    }

    [Fact]
    public void AttributeLookup_Class_Many_Multiple()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookupWithClassAttriubte);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookupWithClassAttriubte.Attribute_Many_Multiple))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(3, metadata.AttributeLookup.Count());
        Assert.Equal([typeof(MyThirdAttribute), typeof(MyFirstAttribute), typeof(MySecondAttribute)], metadata.AttributeLookup.Select(x => x.Key));
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyThirdAttribute)].Count());
        Assert.Equal(1, metadata.AttributeLookup[typeof(MyFirstAttribute)].Count());
        Assert.Equal(3, metadata.AttributeLookup[typeof(MySecondAttribute)].Count());
        Assert.Equal([new MySecondAttribute(0), new MySecondAttribute(1), new MySecondAttribute(2)], metadata.AttributeLookup[typeof(MySecondAttribute)]);
    }

    [Fact]
    public void Attribute_Class_Order()
    {
        // Arrange
        var serviceType = typeof(MyHub_AttributeLookupWithClassAttriubte);
        var methodInfo = serviceType.GetMethod(nameof(MyHub_AttributeLookupWithClassAttriubte.Attribute_Many_Multiple))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        Assert.Equal(5, metadata.Metadata.Count());
        Assert.Equal([ /* Class */ nameof(MyThirdAttribute),  /* Method */ nameof(MyFirstAttribute), nameof(MySecondAttribute), nameof(MySecondAttribute), nameof(MySecondAttribute)], metadata.Metadata.Select(x => x.GetType().Name));
        Assert.Equal(0, Assert.IsType<MySecondAttribute>(metadata.Metadata[2]).Value);
        Assert.Equal(1, Assert.IsType<MySecondAttribute>(metadata.Metadata[3]).Value);
        Assert.Equal(2, Assert.IsType<MySecondAttribute>(metadata.Metadata[4]).Value);
    }

    [Fact]
    public void ValueTask()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_ValueTask))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_ValueTask)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Empty(metadata.Parameters);
        Assert.Equal(typeof(Nil), metadata.RequestType);
        Assert.Null(metadata.ResponseType);
    }

    [Fact]
    public void ValueTaskOfT()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_ValueTaskOfValue))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_ValueTaskOfValue)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Empty(metadata.Parameters);
        Assert.Equal(typeof(Nil), metadata.RequestType);
        Assert.Equal(typeof(int), metadata.ResponseType);
    }

    [Fact]
    public void Void()
    {
        // Arrange
        var type = typeof(MyHub);
        var methodInfo = type.GetMethod(nameof(MyHub.Method_Void))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, methodInfo);

        // Assert
        Assert.Equal(typeof(MyHub), metadata.StreamingHubImplementationType);
        Assert.Equal(typeof(IMyHub), metadata.StreamingHubInterfaceType);
        Assert.Same(typeof(IMyHub).GetMethod(nameof(IMyHub.Method_Void)), metadata.InterfaceMethod);
        Assert.Same(methodInfo, metadata.ImplementationMethod);
        Assert.Empty(metadata.Parameters);
        Assert.Equal(typeof(Nil), metadata.RequestType);
        Assert.Null(metadata.ResponseType);
    }

    interface IMyHubReceiver
    {}

    interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task Method_Task();
        Task<int> Method_TaskOfValue();
        ValueTask Method_ValueTask();
        ValueTask<int> Method_ValueTaskOfValue();
        void Method_Void();

        Task Method_Parameterless();
        Task Method_OneParameter(int arg0);
        Task Method_TwoParameters(int arg0, string arg1);

        int Invalid_Method_Returns_Int(int arg0, string arg1);
        T Invalid_Method_Generic<T>(T arg0);
    }
    class MyHub : IMyHub
    {
        public Task Method_Task() => throw new NotImplementedException();
        public Task<int> Method_TaskOfValue() => throw new NotImplementedException();
        public ValueTask Method_ValueTask() => throw new NotImplementedException();
        public ValueTask<int> Method_ValueTaskOfValue() => throw new NotImplementedException();
        public Task Method_Parameterless() => throw new NotImplementedException();
        public Task Method_OneParameter(int arg0) => throw new NotImplementedException();
        public Task Method_TwoParameters(int arg0, string arg1) => throw new NotImplementedException();
        public void Method_Void() => throw new NotImplementedException();
        public void Invalid_Method_Returns_Void(int arg0, string arg1) => throw new NotImplementedException();
        public int Invalid_Method_Returns_Int(int arg0, string arg1) => throw new NotImplementedException();
        public T Invalid_Method_Generic<T>(T arg0) => throw new NotImplementedException();

        public IMyHub FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();
    }

    interface IMyHub_MethodId : IStreamingHub<IMyHub_MethodId, IMyHubReceiver>
    {
        Task Method_Default();
        [MethodId(12345)]
        Task Method_WithAttribute();

        Task Invalid_Method_WithAttributeOnImpl();
    }

    class MyHub_MethodId : IMyHub_MethodId
    {
        public IMyHub_MethodId FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();

        public Task Method_Default() => throw new NotImplementedException();
        public Task Method_WithAttribute() => throw new NotImplementedException();
        [MethodId(4567)]
        public Task Invalid_Method_WithAttributeOnImpl() => throw new NotImplementedException();
    }

    interface IMyHub_AttributeLookup : IStreamingHub<IMyHub_AttributeLookup, IMyHubReceiver>
    {
        Task<int> Attribute_None();
        Task<int> Attribute_One();
        Task<int> Attribute_Many();
        Task<int> Attribute_Many_Multiple();
    }

    class MyHub_AttributeLookup : IMyHub_AttributeLookup
    {
        public Task<int> Attribute_None() => default;
        [MyFirst]
        public Task<int> Attribute_One() => default;
        [MyFirst, MySecond]
        public Task<int> Attribute_Many() => default;
        [MyFirst, MySecond(0), MySecond(1), MySecond(2)]
        public Task<int> Attribute_Many_Multiple() => default;


        public IMyHub_AttributeLookup FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();
    }

    [MyThird]
    class MyHub_AttributeLookupWithClassAttriubte : IMyHub_AttributeLookup
    {
        public Task<int> Attribute_None() => default;
        [MyFirst]
        public Task<int> Attribute_One() => default;
        [MyFirst, MySecond]
        public Task<int> Attribute_Many() => default;
        [MyFirst, MySecond(0), MySecond(1), MySecond(2)]
        public Task<int> Attribute_Many_Multiple() => default;


        public IMyHub_AttributeLookup FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();
    }


    class MyFirstAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class MySecondAttribute : Attribute, IEquatable<MySecondAttribute>
    {
        public int Value { get; }
        public MySecondAttribute() => Value = 0;
        public MySecondAttribute(int value) => Value = value;

        public bool Equals(MySecondAttribute other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MySecondAttribute)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value);
        }
    }
    class MyThirdAttribute : Attribute { }

}
