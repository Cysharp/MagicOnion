using System.Reflection;
using MagicOnion.Server.Hubs;
using MagicOnionEngineTest;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests;

public class MagicOnionEngineTest
{
    [Fact]
    public void CollectFromTypes_Empty()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{};
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().BeEmpty();
        def.StreamingHubHandlers.Should().BeEmpty();
    }

    [Fact]
    public void CollectFromTypes_NonService()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(NonService) };
        var options = new MagicOnionOptions();

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options, new NullMagicOnionLogger()));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void CollectFromTypes_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyService) };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().HaveCount(2);
        def.StreamingHubHandlers.Should().BeEmpty();
    }

    [Fact]
    public void CollectFromTypes_StreamingHub()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyHub) };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().HaveCount(1); // Connect
        def.StreamingHubHandlers.Should().HaveCount(2);
    }

    [Fact]
    public void CollectFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().HaveCount(3); // MyHub.Connect, MyService.MethodA, MyService.MethodB
        def.StreamingHubHandlers.Should().HaveCount(2); // MyHub.MethodA, MyHub.MethodB
    }
    
    [Fact]
    public void CollectFromAssembly_Ignore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyIgnoredService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().NotContain(x => x.ServiceName.Contains("Ignored"));
        def.StreamingHubHandlers.Should().NotContain(x => x.HubName.Contains("Ignored"));
    }
  
    [Fact]
    public void CollectFromAssembly_NonPublic()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyNonPublicService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().NotContain(x => x.ServiceName.Contains("NonPublic"));
        def.StreamingHubHandlers.Should().NotContain(x => x.HubName.Contains("NonPublic"));
    }
 
    [Fact]
    public void CollectFromAssembly_Abstract()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyAbstractService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().NotContain(x => x.ServiceName.Contains("Abstract"));
        def.StreamingHubHandlers.Should().NotContain(x => x.HubName.Contains("Abstract"));
    }

    [Fact]
    public void CollectFromAssembly_Generic_Constructed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyGenericsService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().Contain(x => x.ServiceType == typeof(MyConstructedGenericsService));
        def.StreamingHubHandlers.Should().Contain(x => x.HubType == typeof(MyConstructedGenericsHub));
    }
    
    [Fact]
    public void CollectFromAssembly_Generic_Definitions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyGenericsService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options, new NullMagicOnionLogger());

        // Assert
        def.MethodHandlers.Should().NotContain(x => x.ServiceType.Name.Contains("GenericsDefinition"));
        def.StreamingHubHandlers.Should().NotContain(x => x.HubType.Name.Contains("GenericsDefinition"));
        def.MethodHandlers.Should().NotContain(x => x.ServiceType.IsGenericTypeDefinition);
        def.StreamingHubHandlers.Should().NotContain(x => x.HubType.IsGenericTypeDefinition);
    }
    
    [Fact]
    public void CollectHandlers_Duplicated_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyService), typeof(MyService) };
        var options = new MagicOnionOptions();

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options, new NullMagicOnionLogger()));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void CollectHandlers_Duplicated_Hub()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IGroupRepositoryFactory, ConcurrentDictionaryGroupRepositoryFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyHub), typeof(MyHub) };
        var options = new MagicOnionOptions();

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options, new NullMagicOnionLogger()));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }
    
    [Fact]
    public void VerifyServiceType_Service()
    {
        // Arrange
        var type = typeof(MyService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        ex.Should().BeNull();
    }

    [Fact]
    public void VerifyServiceType_Hub()
    {
        // Arrange
        var type = typeof(MyHub);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        ex.Should().BeNull();
    }

    [Fact]
    public void VerifyServiceType_Abstract()
    {
        // Arrange
        var type = typeof(MyAbstractService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void VerifyServiceType_Interface()
    {
        // Arrange
        var type = typeof(IMyService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void VerifyServiceType_NonService()
    {
        // Arrange
        var type = typeof(NonService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void VerifyServiceType_GenericDefinition()
    {
        // Arrange
        var type = typeof(GenericService<>);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
    }

    class NonService : IServiceMarker {}

    class GenericService<T> : ServiceBase<IMyService>, IMyService
    {
        public UnaryResult<Nil> MethodA() => default;
        public UnaryResult<Nil> MethodB() => default;
    }
}
