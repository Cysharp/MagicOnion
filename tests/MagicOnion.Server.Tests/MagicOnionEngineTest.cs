#if FALSE
using System.Reflection;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs;
using MagicOnionEngineTest;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests;

public class MagicOnionEngineTest
{
    [Fact]
    public void CollectFromTypes_Empty()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{};
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options);

        // Assert
        Assert.Empty(def.MethodHandlers);
        Assert.Empty(def.StreamingHubHandlers);
    }

    [Fact]
    public void CollectFromTypes_NonService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(NonService) };
        var options = new MagicOnionOptions();

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void CollectFromTypes_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyService) };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options);

        // Assert
        Assert.Equal(2, def.MethodHandlers.Count());
        Assert.Empty(def.StreamingHubHandlers);
    }

    [Fact]
    public void CollectFromTypes_StreamingHub()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyHub) };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options);

        // Assert
        Assert.Single(def.MethodHandlers); // Connect
        Assert.Equal(2, def.StreamingHubHandlers.Count());
    }

    [Fact]
    public void CollectFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options);

        // Assert
        Assert.Equal(6, def.MethodHandlers.Count()); // IMyHub.Connect, IMyService.MethodA, IMyService.MethodB, IMyGenericsHub.Connect, IMyGenericsService.MethodA, IMyGenericsService.MethodB
        Assert.Equal(4, def.StreamingHubHandlers.Count()); // IMyHub.MethodA, IMyHub.MethodB, IMyGenericsHub.MethodA, IMyGenericsHub.MethodB
    }
    
    [Fact]
    public void CollectFromAssembly_Ignore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyIgnoredService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options);

        // Assert
        Assert.DoesNotContain(def.MethodHandlers, x => x.ServiceName.Contains("Ignored"));
        Assert.DoesNotContain(def.StreamingHubHandlers, x => x.HubName.Contains("Ignored"));
    }
  
    [Fact]
    public void CollectFromAssembly_NonPublic()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyNonPublicService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options);

        // Assert
        Assert.DoesNotContain(def.MethodHandlers, x => x.ServiceName.Contains("NonPublic"));
        Assert.DoesNotContain(def.StreamingHubHandlers, x => x.HubName.Contains("NonPublic"));
    }
 
    [Fact]
    public void CollectFromAssembly_Abstract()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyAbstractService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options);

        // Assert
        Assert.DoesNotContain(def.MethodHandlers, x => x.ServiceName.Contains("Abstract"));
        Assert.DoesNotContain(def.StreamingHubHandlers, x => x.HubName.Contains("Abstract"));
    }

    [Fact]
    public void CollectFromAssembly_Generic_Constructed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyGenericsService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options);

        // Assert
        Assert.Contains(def.MethodHandlers, x => x.ServiceType == typeof(MyConstructedGenericsService));
        Assert.Contains(def.StreamingHubHandlers, x => x.HubType == typeof(MyConstructedGenericsHub));
    }
    
    [Fact]
    public void CollectFromAssembly_Generic_Definitions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var assemblies = new Assembly[]{ typeof(IMyGenericsService).Assembly };
        var options = new MagicOnionOptions();

        // Act
        var def = MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, assemblies, options);

        // Assert
        Assert.DoesNotContain(def.MethodHandlers, x => x.ServiceType.Name.Contains("GenericsDefinition"));
        Assert.DoesNotContain(def.StreamingHubHandlers, x => x.HubType.Name.Contains("GenericsDefinition"));
        Assert.DoesNotContain(def.MethodHandlers, x => x.ServiceType.IsGenericTypeDefinition);
        Assert.DoesNotContain(def.StreamingHubHandlers, x => x.HubType.IsGenericTypeDefinition);
    }
    
    [Fact]
    public void CollectHandlers_Duplicated_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyService), typeof(MyService) };
        var options = new MagicOnionOptions();

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void CollectHandlers_Duplicated_Hub()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationManager());
        services.AddMagicOnionCore();
        var serviceProvider = services.BuildServiceProvider();
        var types = new Type[]{ typeof(MyHub), typeof(MyHub) };
        var options = new MagicOnionOptions();

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.BuildServerServiceDefinition(serviceProvider, types, options));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }
    
    [Fact]
    public void VerifyServiceType_Service()
    {
        // Arrange
        var type = typeof(MyService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void VerifyServiceType_Hub()
    {
        // Arrange
        var type = typeof(MyHub);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void VerifyServiceType_Abstract()
    {
        // Arrange
        var type = typeof(MyAbstractService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void VerifyServiceType_Interface()
    {
        // Arrange
        var type = typeof(IMyService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void VerifyServiceType_NonService()
    {
        // Arrange
        var type = typeof(NonService);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void VerifyServiceType_GenericDefinition()
    {
        // Arrange
        var type = typeof(GenericService<>);

        // Act
        var ex = Record.Exception(() => MagicOnionEngine.VerifyServiceType(type));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    class NonService : IServiceMarker {}

    class GenericService<T> : ServiceBase<IMyService>, IMyService
    {
        public UnaryResult<Nil> MethodA() => default;
        public UnaryResult<Nil> MethodB() => default;
    }

    [Fact]
    public void ShouldIgnoreAssembly()
    {
        Assert.True(MagicOnionEngine.ShouldIgnoreAssembly("mscorlib"));
        Assert.True(MagicOnionEngine.ShouldIgnoreAssembly("System.Private.CoreLib"));
        Assert.True(MagicOnionEngine.ShouldIgnoreAssembly("Grpc.Net.Client"));
        Assert.True(MagicOnionEngine.ShouldIgnoreAssembly("MagicOnion.Client"));
        Assert.True(MagicOnionEngine.ShouldIgnoreAssembly("MagicOnion.Client._DynamicClient_"));
        Assert.True(MagicOnionEngine.ShouldIgnoreAssembly("MagicOnion.Server"));

        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly(""));
        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly("A"));
        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly("AspNetCoreSample"));
        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly("GrpcSample"));
        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly("MyGrpc.Net"));
        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly("MyApp.System.Net"));
        Assert.False(MagicOnionEngine.ShouldIgnoreAssembly("MagicOnionSample"));
    }
}
#endif
