using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Tests;

public class MethodHandlerMetadataFactoryTest
{
    [Fact]
    public void NonService()
    {
        // Arrange
        var serviceType = typeof(MyNonService);
        var methodInfo = serviceType.GetMethod(nameof(MyNonService.Unary_Parameterless))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AttributeLookup_None()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup.Attribute_None))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().BeEmpty();
    }

    [Fact]
    public void AttributeLookup_One()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup.Attribute_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(1);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyFirstAttribute));
        metadata.AttributeLookup[typeof(MyFirstAttribute)].Should().HaveCount(1);
    }

    [Fact]
    public void AttributeLookup_Many()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup.Attribute_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(2);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyFirstAttribute), typeof(MySecondAttribute));
        metadata.AttributeLookup[typeof(MyFirstAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MySecondAttribute)].Should().HaveCount(1);
    }

    [Fact]
    public void AttributeLookup_Many_Multiple()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup.Attribute_Many_Multiple))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(2);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyFirstAttribute), typeof(MySecondAttribute));
        metadata.AttributeLookup[typeof(MyFirstAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MySecondAttribute)].Should().HaveCount(3);
        metadata.AttributeLookup[typeof(MySecondAttribute)].Should().Equal(new MySecondAttribute(0), new MySecondAttribute(1), new MySecondAttribute(2));
    }

    [Fact]
    public void AttributeLookup_Class_None()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup_WithClassAttribute);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup_WithClassAttribute.Attribute_None))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(1);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyThirdAttribute));
        metadata.AttributeLookup[typeof(MyThirdAttribute)].Should().HaveCount(1);
    }

    [Fact]
    public void AttributeLookup_Class_One()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup_WithClassAttribute);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup_WithClassAttribute.Attribute_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(2);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyThirdAttribute), typeof(MyFirstAttribute));
        metadata.AttributeLookup[typeof(MyThirdAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MyFirstAttribute)].Should().HaveCount(1);
    }

    [Fact]
    public void AttributeLookup_Class_Many()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup_WithClassAttribute);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup_WithClassAttribute.Attribute_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(3);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyThirdAttribute), typeof(MyFirstAttribute), typeof(MySecondAttribute));
        metadata.AttributeLookup[typeof(MyThirdAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MyFirstAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MySecondAttribute)].Should().HaveCount(1);
    }

    [Fact]
    public void AttributeLookup_Class_Many_Multiple()
    {
        // Arrange
        var serviceType = typeof(MyService_AttributeLookup_WithClassAttribute);
        var methodInfo = serviceType.GetMethod(nameof(MyService_AttributeLookup_WithClassAttribute.Attribute_Many_Multiple))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.AttributeLookup.Should().HaveCount(3);
        metadata.AttributeLookup.Select(x => x.Key).Should().Equal(typeof(MyThirdAttribute), typeof(MyFirstAttribute), typeof(MySecondAttribute));
        metadata.AttributeLookup[typeof(MyThirdAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MyFirstAttribute)].Should().HaveCount(1);
        metadata.AttributeLookup[typeof(MySecondAttribute)].Should().HaveCount(3);
        metadata.AttributeLookup[typeof(MySecondAttribute)].Should().Equal(new MySecondAttribute(0), new MySecondAttribute(1), new MySecondAttribute(2));
    }

    [Fact]
    public void Unary_Parameterless()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.Unary_Parameterless))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.Unary);
        metadata.RequestType.Should().Be<MessagePack.Nil>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void Unary_Parameter_One()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.Unary_Parameter_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.Unary);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void Unary_Parameter_Many()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.Unary_Parameter_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.Unary);
        metadata.RequestType.Should().Be<DynamicArgumentTuple<int, string>>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void ServerStreaming_Parameterless()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ServerStreaming_Parameterless))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ServerStreaming);
        metadata.RequestType.Should().Be<MessagePack.Nil>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void ServerStreaming_Parameter_One()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ServerStreaming_Parameter_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ServerStreaming);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void ServerStreaming_Parameter_Many()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ServerStreaming_Parameter_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ServerStreaming);
        metadata.RequestType.Should().Be<DynamicArgumentTuple<int, string>>();
        metadata.ResponseType.Should().Be<int>();
    }


    [Fact]
    public void ServerStreaming_Sync_Parameterless()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ServerStreaming_Sync_Parameterless))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ServerStreaming);
        metadata.RequestType.Should().Be<MessagePack.Nil>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void ServerStreaming_Sync_Parameter_One()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ServerStreaming_Sync_Parameter_One))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ServerStreaming);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void ServerStreaming_Sync_Parameter_Many()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ServerStreaming_Sync_Parameter_Many))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ServerStreaming);
        metadata.RequestType.Should().Be<DynamicArgumentTuple<int, string>>();
        metadata.ResponseType.Should().Be<int>();
    }

    [Fact]
    public void ClientStreaming()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ClientStreaming))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ClientStreaming);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<string>();
    }

    [Fact]
    public void ClientStreaming_Invalid()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ClientStreaming_Invalid))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void ClientStreaming_Sync()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ClientStreaming_Sync))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.ClientStreaming);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<string>();
    }

    [Fact]
    public void ClientStreaming_Sync_Invalid()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.ClientStreaming_Sync_Invalid))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void DuplexStreaming()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.DuplexStreaming))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.DuplexStreaming);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<string>();
    }

    [Fact]
    public void DuplexStreaming_Invalid()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.DuplexStreaming_Invalid))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void DuplexStreaming_Sync()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.DuplexStreaming_Sync))!;

        // Act
        var metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo);

        // Assert
        metadata.ServiceImplementationType.Should().Be<MyService>();
        metadata.ServiceImplementationMethod.Should().BeSameAs(methodInfo);
        metadata.ServiceInterface.Should().Be<IMyService>();
        metadata.MethodType.Should().Be(MethodType.DuplexStreaming);
        metadata.RequestType.Should().Be<int>();
        metadata.ResponseType.Should().Be<string>();
    }

    [Fact]
    public void DuplexStreaming_Sync_Invalid()
    {
        // Arrange
        var serviceType = typeof(MyService);
        var methodInfo = serviceType.GetMethod(nameof(MyService.DuplexStreaming_Sync_Invalid))!;

        // Act
        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(serviceType, methodInfo));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    interface IMyService : IService<IMyService>
    {
        UnaryResult<int> Unary_Parameterless();
        UnaryResult<int> Unary_Parameter_One(int arg0);
        UnaryResult<int> Unary_Parameter_Many(int arg0, string arg1);

        Task<ServerStreamingResult<int>> ServerStreaming_Parameterless();
        Task<ServerStreamingResult<int>> ServerStreaming_Parameter_One(int arg0);
        Task<ServerStreamingResult<int>> ServerStreaming_Parameter_Many(int arg0, string arg1);
        ServerStreamingResult<int> ServerStreaming_Sync_Parameterless();
        ServerStreamingResult<int> ServerStreaming_Sync_Parameter_One(int arg0);
        ServerStreamingResult<int> ServerStreaming_Sync_Parameter_Many(int arg0, string arg1);

        Task<ClientStreamingResult<int, string>> ClientStreaming();
        Task<ClientStreamingResult<int, string>> ClientStreaming_Invalid(int arg0);
        ClientStreamingResult<int, string> ClientStreaming_Sync();
        ClientStreamingResult<int, string> ClientStreaming_Sync_Invalid(int arg0);

        Task<DuplexStreamingResult<int, string>> DuplexStreaming();
        Task<DuplexStreamingResult<int, string>> DuplexStreaming_Invalid(int arg0);
        DuplexStreamingResult<int, string> DuplexStreaming_Sync();
        DuplexStreamingResult<int, string> DuplexStreaming_Sync_Invalid(int arg0);
    }

    class MyService : IMyService
    {
        public UnaryResult<int> Unary_Parameterless() => default;
        public UnaryResult<int> Unary_Parameter_One(int arg0) => default;
        public UnaryResult<int> Unary_Parameter_Many(int arg0, string arg1) => default;

        public Task<ServerStreamingResult<int>> ServerStreaming_Parameterless() => default;
        public Task<ServerStreamingResult<int>> ServerStreaming_Parameter_One(int arg0) => default;
        public Task<ServerStreamingResult<int>> ServerStreaming_Parameter_Many(int arg0, string arg1) => default;
        public ServerStreamingResult<int> ServerStreaming_Sync_Parameterless() => default;
        public ServerStreamingResult<int> ServerStreaming_Sync_Parameter_One(int arg0) => default;
        public ServerStreamingResult<int> ServerStreaming_Sync_Parameter_Many(int arg0, string arg1) => default;

        public Task<ClientStreamingResult<int, string>> ClientStreaming() => default;
        public Task<ClientStreamingResult<int, string>> ClientStreaming_Invalid(int arg0) => default;
        public ClientStreamingResult<int, string> ClientStreaming_Sync() => default;
        public ClientStreamingResult<int, string> ClientStreaming_Sync_Invalid(int arg0) => default;

        public Task<DuplexStreamingResult<int, string>> DuplexStreaming() => default;
        public Task<DuplexStreamingResult<int, string>> DuplexStreaming_Invalid(int arg0) => default;
        public DuplexStreamingResult<int, string> DuplexStreaming_Sync() => default;
        public DuplexStreamingResult<int, string> DuplexStreaming_Sync_Invalid(int arg0) => default;

        public IMyService WithOptions(CallOptions option) => throw new NotImplementedException();
        public IMyService WithHeaders(Metadata headers) => throw new NotImplementedException();
        public IMyService WithDeadline(DateTime deadline) => throw new NotImplementedException();
        public IMyService WithCancellationToken(CancellationToken cancellationToken) => throw new NotImplementedException();
        public IMyService WithHost(string host) => throw new NotImplementedException();
    }

    class MyNonService
    {
        public UnaryResult<int> Unary_Parameterless() => default;
    }

    interface IMyService_AttributeLookup : IService<IMyService_AttributeLookup>
    {
        UnaryResult<int> Attribute_None();
        UnaryResult<int> Attribute_One();
        UnaryResult<int> Attribute_Many();
    }

    class MyService_AttributeLookup : IMyService_AttributeLookup
    {
        public UnaryResult<int> Attribute_None() => default;
        [MyFirst]
        public UnaryResult<int> Attribute_One() => default;
        [MyFirst, MySecond]
        public UnaryResult<int> Attribute_Many() => default;
        [MyFirst, MySecond(0), MySecond(1), MySecond(2)]
        public UnaryResult<int> Attribute_Many_Multiple() => default;

        public IMyService_AttributeLookup WithOptions(CallOptions option) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithHeaders(Metadata headers) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithDeadline(DateTime deadline) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithCancellationToken(CancellationToken cancellationToken) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithHost(string host) => throw new NotImplementedException();
    }

    [MyThird]
    class MyService_AttributeLookup_WithClassAttribute : IMyService_AttributeLookup
    {
        public UnaryResult<int> Attribute_None() => default;
        [MyFirst]
        public UnaryResult<int> Attribute_One() => default;
        [MyFirst, MySecond]
        public UnaryResult<int> Attribute_Many() => default;
        [MyFirst, MySecond(0), MySecond(1), MySecond(2)]
        public UnaryResult<int> Attribute_Many_Multiple() => default;

        public IMyService_AttributeLookup WithOptions(CallOptions option) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithHeaders(Metadata headers) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithDeadline(DateTime deadline) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithCancellationToken(CancellationToken cancellationToken) => throw new NotImplementedException();
        public IMyService_AttributeLookup WithHost(string host) => throw new NotImplementedException();
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

