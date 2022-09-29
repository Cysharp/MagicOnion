using System.Reflection;
using MagicOnion.Server.Hubs;
using MagicOnionEngineTest;
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
}