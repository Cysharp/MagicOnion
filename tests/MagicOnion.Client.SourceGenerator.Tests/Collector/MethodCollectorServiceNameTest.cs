using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public class MethodCollectorServiceNameTest
{
    [Fact]
    public void Service_WithServiceNameAttribute_UsesCustomName()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace AreaA
{
    [ServiceName(""AreaA.IMyService"")]
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> MethodA();
    }
}

namespace AreaB
{
    [ServiceName(""AreaB.IMyService"")]
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> MethodA();
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(compilation.GetDiagnostics(TestContext.Current.CancellationToken), x => x.Severity == DiagnosticSeverity.Error);
        Assert.NotNull(serviceCollection);
        Assert.Equal(2, serviceCollection.Services.Count);

        var serviceA = serviceCollection.Services.First(s => s.ServiceType.Namespace == "AreaA");
        var serviceB = serviceCollection.Services.First(s => s.ServiceType.Namespace == "AreaB");

        Assert.Equal("AreaA.IMyService", serviceA.Methods[0].ServiceName);
        Assert.Equal("AreaA.IMyService/MethodA", serviceA.Methods[0].Path);

        Assert.Equal("AreaB.IMyService", serviceB.Methods[0].ServiceName);
        Assert.Equal("AreaB.IMyService/MethodA", serviceB.Methods[0].Path);

        Assert.NotEqual(serviceA.Methods[0].ServiceName, serviceB.Methods[0].ServiceName);
    }

    [Fact]
    public void Service_WithoutServiceNameAttribute_UsesShortName()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> MethodA();
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(compilation.GetDiagnostics(TestContext.Current.CancellationToken), x => x.Severity == DiagnosticSeverity.Error);
        Assert.Single(serviceCollection.Services);
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("IMyService/MethodA", serviceCollection.Services[0].Methods[0].Path);
    }

    [Fact]
    public void StreamingHub_WithServiceNameAttribute_UsesCustomName()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace AreaA
{
    [ServiceName(""AreaA.IChatHub"")]
    public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
    {
        ValueTask SendAsync(string message);
    }
    public interface IChatHubReceiver
    {
        void OnReceive(string message);
    }
}

namespace AreaB
{
    [ServiceName(""AreaB.IChatHub"")]
    public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
    {
        ValueTask SendAsync(string message);
    }
    public interface IChatHubReceiver
    {
        void OnReceive(string message);
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(compilation.GetDiagnostics(TestContext.Current.CancellationToken), x => x.Severity == DiagnosticSeverity.Error);
        Assert.NotNull(serviceCollection);
        Assert.Equal(2, serviceCollection.Hubs.Count);

        var hubA = serviceCollection.Hubs.First(h => h.ServiceType.Namespace == "AreaA");
        var hubB = serviceCollection.Hubs.First(h => h.ServiceType.Namespace == "AreaB");

        Assert.Equal("AreaA.IChatHub", hubA.ServiceName);
        Assert.Equal("AreaB.IChatHub", hubB.ServiceName);
        Assert.NotEqual(hubA.ServiceName, hubB.ServiceName);
    }

    [Fact]
    public void StreamingHub_WithoutServiceNameAttribute_UsesShortName()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace
{
    public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
    {
        ValueTask SendAsync(string message);
    }
    public interface IChatHubReceiver
    {
        void OnReceive(string message);
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(compilation.GetDiagnostics(TestContext.Current.CancellationToken), x => x.Severity == DiagnosticSeverity.Error);
        Assert.Single(serviceCollection.Hubs);
        Assert.Equal("IChatHub", serviceCollection.Hubs[0].ServiceName);
    }
}
