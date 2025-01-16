using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MessagePack;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public class MethodCollectorServicesTest
{
    [Fact]
    public void Ignore_Method()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    UnaryResult<Nil> MethodA();

    [Ignore]
    UnaryResult<Nil> MethodB();

    UnaryResult<Nil> MethodC();
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(2, serviceCollection.Services[0].Methods.Count());
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal("MethodC", serviceCollection.Services[0].Methods[1].MethodName);
    }

    [Fact]
    public void Ignore_Interface()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

[Ignore]
public interface IMyService : IService<IMyService>
{
    UnaryResult<Nil> MethodA();

    UnaryResult<Nil> MethodB();

    UnaryResult<Nil> MethodC();
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Empty(serviceCollection.Services);
    }

    [Fact]
    public void Unary_NonGenericResult()
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
        UnaryResult MethodA();
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
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void Unary_Array()
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
        UnaryResult<Tuple<int, string>[]> MethodA(Tuple<bool, long>[] arg1);
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
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Tuple<bool, long>[]>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Tuple<int, string>[]>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult<Tuple<int, string>[]>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void Unary_Nullable()
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
        UnaryResult<int?> MethodA(Tuple<bool?, long?> arg1);
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
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Tuple<bool?, long?>>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int?>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult<int?>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void Unary_Parameter_Zero_ReturnNil()
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
        Assert.NotNull(serviceCollection);
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // UnaryResult<Nil> MethodA();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Empty(serviceCollection.Services[0].Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult<Nil>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }


    [Fact]
    public void Unary_Parameter_Zero_ReturnValue()
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
        UnaryResult<string> MethodA();
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // UnaryResult<string> MethodA();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Empty(serviceCollection.Services[0].Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult<string>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void Unary_Parameter_One_ReturnNil()
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
        UnaryResult<Nil> MethodA(string arg1);
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // UnaryResult<Nil> MethodA(string arg1);
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].Parameters[0].Type);
        Assert.Equal("arg1", serviceCollection.Services[0].Methods[0].Parameters[0].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult<Nil>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void Unary_Parameter_Many_ReturnNil()
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
        UnaryResult<Nil> MethodA(string arg1, int arg2);
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // UnaryResult<Nil> MethodA(string arg1, int arg2);
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<string, int>>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].Parameters[0].Type);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].Parameters[1].Type);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<UnaryResult<Nil>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }


    [Fact]
    public void Unary_HasDefaultValue()
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
        UnaryResult<Nil> MethodA(string arg1 = ""Hello"", int arg2 = 1234, long arg3 = default, string arg4 = default);
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
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].Parameters[0].Type);
        Assert.True(serviceCollection.Services[0].Methods[0].Parameters[0].HasExplicitDefaultValue);
        Assert.Equal("\"Hello\"", serviceCollection.Services[0].Methods[0].Parameters[0].DefaultValue);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].Parameters[1].Type);
        Assert.Equal("1234", serviceCollection.Services[0].Methods[0].Parameters[1].DefaultValue);
        Assert.True(serviceCollection.Services[0].Methods[0].Parameters[1].HasExplicitDefaultValue);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<long>(), serviceCollection.Services[0].Methods[0].Parameters[2].Type);
        Assert.Equal("0", serviceCollection.Services[0].Methods[0].Parameters[2].DefaultValue);
        Assert.True(serviceCollection.Services[0].Methods[0].Parameters[2].HasExplicitDefaultValue);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].Parameters[3].Type);
        Assert.Equal("null", serviceCollection.Services[0].Methods[0].Parameters[3].DefaultValue);
        Assert.True(serviceCollection.Services[0].Methods[0].Parameters[3].HasExplicitDefaultValue);
    }

    [Fact]
    public void Unary_Methods()
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
        UnaryResult<Nil> NilAsync();
        UnaryResult<string> StringAsync();
        UnaryResult<Nil> OneParameter(string arg1);
        UnaryResult<Nil> TwoParameter(string arg1, int arg2);
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(4, serviceCollection.Services[0].Methods.Count());
    }

    [Fact]
    public void Unary_InvalidReturnType_ServerStreamingResult()
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
        UnaryResult<ServerStreamingResult<int>> MethodA();
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.UnaryUnsupportedMethodReturnType.Id, diagnostics[0].Id);
    }

    [Fact]
    public void Unary_InvalidReturnType_ClientStreamingResult()
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
        UnaryResult<ClientStreamingResult<int, string>> MethodA();
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.UnaryUnsupportedMethodReturnType.Id, diagnostics[0].Id);
    }

    [Fact]
    public void Unary_InvalidReturnType_DuplexStreamingResult()
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
        UnaryResult<DuplexStreamingResult<int, string>> MethodA();
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.UnaryUnsupportedMethodReturnType.Id, diagnostics[0].Id);
    }

    [Fact]
    public void UnsupportedType()
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
        void MethodA();
    }
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, diagnostics[0].Id);
    }

    [Fact]
    public void ServerStreaming_Parameter_Zero()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<ServerStreamingResult<int>> ServerStreaming();
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // Task<ServerStreamingResult<int>> ServerStreamingNoArg();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("ServerStreaming", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Empty(serviceCollection.Services[0].Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<ServerStreamingResult<int>>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ServerStreaming_Parameter_One()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<ServerStreamingResult<int>> ServerStreaming(string arg1);
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // Task<ServerStreamingResult<int>> ServerStreamingNoArg();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("ServerStreaming", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].Parameters[0].Type);
        Assert.Equal("arg1", serviceCollection.Services[0].Methods[0].Parameters[0].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<ServerStreamingResult<int>>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ServerStreaming_Parameter_Many()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<ServerStreamingResult<int>> ServerStreaming(string arg1, int arg2);
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // Task<ServerStreamingResult<int>> ServerStreamingNoArg();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("ServerStreaming", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<string, int>>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].Parameters[0].Type);
        Assert.Equal("arg1", serviceCollection.Services[0].Methods[0].Parameters[0].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].Parameters[1].Type);
        Assert.Equal("arg2", serviceCollection.Services[0].Methods[0].Parameters[1].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<ServerStreamingResult<int>>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ServerStreaming_ShouldNotBeTask()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    ServerStreamingResult<int> ServerStreaming(string arg1, int arg2);
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, diagnostics[0].Id);
    }


    [Fact]
    public void DuplexStreaming()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<DuplexStreamingResult<int, string>> MethodA();
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // Task<DuplexStreamingResult<int, string>> MethodA();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Empty(serviceCollection.Services[0].Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<DuplexStreamingResult<int, string>>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void DuplexStreaming_ParameterNotSupported()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<DuplexStreamingResult<int, string>> MethodA(string arg1);
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.StreamingMethodMustHaveNoParameters.Id, diagnostics[0].Id);
    }

    [Fact]
    public void ClientStreaming()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<ClientStreamingResult<int, string>> MethodA();
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
        Assert.Empty(serviceCollection.Hubs);
        Assert.Equal(1, serviceCollection.Services.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"), serviceCollection.Services[0].ServiceType);
        Assert.Equal(1, serviceCollection.Services[0].Methods.Count());
        // Task<DuplexStreamingResult<int, string>> MethodA();
        Assert.Equal("IMyService", serviceCollection.Services[0].Methods[0].ServiceName);
        Assert.Equal("MethodA", serviceCollection.Services[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Services[0].Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Services[0].Methods[0].ResponseType);
        Assert.Empty(serviceCollection.Services[0].Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<ClientStreamingResult<int, string>>>(), serviceCollection.Services[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ClientStreaming_ParameterNotSupported()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyService : IService<IMyService>
{
    Task<ClientStreamingResult<int, string>> MethodA(string arg1);
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.StreamingMethodMustHaveNoParameters.Id, diagnostics[0].Id);
    }
}
