using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MessagePack;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public class MethodCollectorStreamingHubsTest
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

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();

    [Ignore]
    Task MethodB();

    Task MethodC();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Empty(serviceCollection.Services);
        Assert.Equal(MagicOnionTypeInfo.Create("MyNamespace", "IMyHub"), serviceCollection.Hubs[0].ServiceType);
        Assert.Equal(2, serviceCollection.Hubs[0].Methods.Count());
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal("MethodC", serviceCollection.Hubs[0].Methods[1].MethodName);
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
public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Empty(serviceCollection.Hubs);
        Assert.Empty(serviceCollection.Services);
        Assert.Empty(serviceCollection.Hubs);
    }

    [Fact]
    public void Parameter_Zero()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        // Task MethodA();
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Nil>(), serviceCollection.Hubs[0].Methods[0].RequestType);
        Assert.Empty(serviceCollection.Hubs[0].Methods[0].Parameters);
    }

    [Fact]
    public void Parameter_One()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA(int arg1);
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        // Task MethodA();
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Hubs[0].Methods[0].RequestType);
        Assert.Equal(1, serviceCollection.Hubs[0].Methods[0].Parameters.Count());
        Assert.Equal("arg1", serviceCollection.Hubs[0].Methods[0].Parameters[0].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Hubs[0].Methods[0].Parameters[0].Type);
        Assert.False(serviceCollection.Hubs[0].Methods[0].Parameters[0].HasExplicitDefaultValue);
        Assert.Equal("default(int)", serviceCollection.Hubs[0].Methods[0].Parameters[0].DefaultValue);
    }

    [Fact]
    public void Parameter_Many()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA(int arg1, string arg2);
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        // Task MethodA();
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<int, string>>(), serviceCollection.Hubs[0].Methods[0].RequestType);
        Assert.Equal(2, serviceCollection.Hubs[0].Methods[0].Parameters.Count());
        Assert.Equal("arg1", serviceCollection.Hubs[0].Methods[0].Parameters[0].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Hubs[0].Methods[0].Parameters[0].Type);
        Assert.False(serviceCollection.Hubs[0].Methods[0].Parameters[0].HasExplicitDefaultValue);
        Assert.Equal("default(int)", serviceCollection.Hubs[0].Methods[0].Parameters[0].DefaultValue);
        Assert.Equal("arg2", serviceCollection.Hubs[0].Methods[0].Parameters[1].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Hubs[0].Methods[0].Parameters[1].Type);
        Assert.False(serviceCollection.Hubs[0].Methods[0].Parameters[1].HasExplicitDefaultValue);
        Assert.Equal("default(string)", serviceCollection.Hubs[0].Methods[0].Parameters[1].DefaultValue);
    }


    [Fact]
    public void Parameter_HasDefaultValue()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA(int arg1, string arg2 = ""DEFAULT"");
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        // Task MethodA();
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<int, string>>(), serviceCollection.Hubs[0].Methods[0].RequestType);
        Assert.Equal(2, serviceCollection.Hubs[0].Methods[0].Parameters.Count());
        Assert.Equal("arg1", serviceCollection.Hubs[0].Methods[0].Parameters[0].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Hubs[0].Methods[0].Parameters[0].Type);
        Assert.False(serviceCollection.Hubs[0].Methods[0].Parameters[0].HasExplicitDefaultValue);
        Assert.Equal("default(int)", serviceCollection.Hubs[0].Methods[0].Parameters[0].DefaultValue);
        Assert.Equal("arg2", serviceCollection.Hubs[0].Methods[0].Parameters[1].Name);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Hubs[0].Methods[0].Parameters[1].Type);
        Assert.True(serviceCollection.Hubs[0].Methods[0].Parameters[1].HasExplicitDefaultValue);
        Assert.Equal("\"DEFAULT\"", serviceCollection.Hubs[0].Methods[0].Parameters[1].DefaultValue);
    }

    [Fact]
    public void HubId_Implicit()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Equal(1, serviceCollection.Hubs[0].Methods.Count());
        // Task MethodA();
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(1497325507, serviceCollection.Hubs[0].Methods[0].HubId);
        // void EventA();
        Assert.Equal("EventA", serviceCollection.Hubs[0].Receiver.Methods[0].MethodName);
        Assert.Equal(842297178, serviceCollection.Hubs[0].Receiver.Methods[0].HubId);
    }

    [Fact]
    public void HubId_Explicit()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

using MagicOnion.Server.Hubs; // for MethodIdAttribute

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    [MethodId(12345)]
    Task MethodA();
}

public interface IMyHubReceiver
{
    [MethodId(67890)]
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Equal(1, serviceCollection.Hubs[0].Methods.Count());
        // Task MethodA();
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(12345, serviceCollection.Hubs[0].Methods[0].HubId);
        // void EventA();
        Assert.Equal("EventA", serviceCollection.Hubs[0].Receiver.Methods[0].MethodName);
        Assert.Equal(67890, serviceCollection.Hubs[0].Receiver.Methods[0].HubId);
    }

    [Fact]
    public void ReturnType_NotSupported_NotTaskOfT()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    string MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedMethodReturnType.Id, diagnostics[0].Id);
    }

    [Fact]
    public void ReturnType_Task()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Threading_Tasks_Task, serviceCollection.Hubs[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ReturnType_TaskOfT()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task<string> MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_String, serviceCollection.Hubs[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<string>>(), serviceCollection.Hubs[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ReturnType_ValueTask()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    ValueTask MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Threading_Tasks_ValueTask, serviceCollection.Hubs[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ReturnType_ValueTaskOfT()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    ValueTask<string> MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_String, serviceCollection.Hubs[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<ValueTask<string>>(), serviceCollection.Hubs[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void ReturnType_Void()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    void MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal("MethodA", serviceCollection.Hubs[0].Methods[0].MethodName);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Void, serviceCollection.Hubs[0].Methods[0].MethodReturnType);
    }

    [Fact]
    public void Receiver()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    void EventA();
    void EventB(Nil nil);
    void EventC(string arg1, int arg2);
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Equal(1, serviceCollection.Hubs[0].Methods.Count());
        Assert.NotNull(serviceCollection.Hubs[0].Receiver);
        Assert.Equal(3, serviceCollection.Hubs[0].Receiver.Methods.Count());
        // void EventA();
        Assert.Equal("EventA", serviceCollection.Hubs[0].Receiver.Methods[0].MethodName);
        Assert.False(serviceCollection.Hubs[0].Receiver.Methods[0].IsClientResult);
        Assert.Empty(serviceCollection.Hubs[0].Receiver.Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Void, serviceCollection.Hubs[0].Receiver.Methods[0].MethodReturnType);
        // void EventB(Nil nil);
        Assert.Equal("EventB", serviceCollection.Hubs[0].Receiver.Methods[1].MethodName);
        Assert.False(serviceCollection.Hubs[0].Receiver.Methods[1].IsClientResult);
        Assert.Equal(1, serviceCollection.Hubs[0].Receiver.Methods[1].Parameters.Count());
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[1].RequestType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[1].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Void, serviceCollection.Hubs[0].Receiver.Methods[1].MethodReturnType);
        // void EventB(Nil nil);
        Assert.Equal("EventC", serviceCollection.Hubs[0].Receiver.Methods[2].MethodName);
        Assert.False(serviceCollection.Hubs[0].Receiver.Methods[2].IsClientResult);
        Assert.Equal(2, serviceCollection.Hubs[0].Receiver.Methods[2].Parameters.Count());
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<string, int>>(), serviceCollection.Hubs[0].Receiver.Methods[2].RequestType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[2].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Void, serviceCollection.Hubs[0].Receiver.Methods[2].MethodReturnType);
    }

    [Fact]
    public void Receiver_ClientResult()
    {
        // Arrange
        var source = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    Task ClientResultA();
    Task<int> ClientResultB(Nil nil);
    Task<string> ClientResultC(string arg1, int arg2);
    Task<string> ClientResultD(string arg1, int arg2, CancellationToken cancellationToken);
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Equal(1, serviceCollection.Hubs[0].Methods.Count());
        Assert.NotNull(serviceCollection.Hubs[0].Receiver);
        Assert.Equal(4, serviceCollection.Hubs[0].Receiver.Methods.Count());
        // Task ClientResultA();
        Assert.Equal("ClientResultA", serviceCollection.Hubs[0].Receiver.Methods[0].MethodName);
        Assert.True(serviceCollection.Hubs[0].Receiver.Methods[0].IsClientResult);
        Assert.Empty(serviceCollection.Hubs[0].Receiver.Methods[0].Parameters);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[0].RequestType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[0].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.System_Threading_Tasks_Task, serviceCollection.Hubs[0].Receiver.Methods[0].MethodReturnType);
        // Task<int> ClientResultB(Nil nil);
        Assert.Equal("ClientResultB", serviceCollection.Hubs[0].Receiver.Methods[1].MethodName);
        Assert.True(serviceCollection.Hubs[0].Receiver.Methods[1].IsClientResult);
        Assert.Equal(1, serviceCollection.Hubs[0].Receiver.Methods[1].Parameters.Count());
        Assert.Equal(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil, serviceCollection.Hubs[0].Receiver.Methods[1].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<int>(), serviceCollection.Hubs[0].Receiver.Methods[1].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<int>>(), serviceCollection.Hubs[0].Receiver.Methods[1].MethodReturnType);
        // Task<string> ClientResultC(string arg1, int arg2);
        Assert.Equal("ClientResultC", serviceCollection.Hubs[0].Receiver.Methods[2].MethodName);
        Assert.True(serviceCollection.Hubs[0].Receiver.Methods[2].IsClientResult);
        Assert.Equal(2, serviceCollection.Hubs[0].Receiver.Methods[2].Parameters.Count());
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<string, int>>(), serviceCollection.Hubs[0].Receiver.Methods[2].RequestType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Hubs[0].Receiver.Methods[2].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<string>>(), serviceCollection.Hubs[0].Receiver.Methods[2].MethodReturnType);
        // Task<string> ClientResultD(string arg1, int arg2, CancellationToken cancellationToken);
        Assert.Equal("ClientResultD", serviceCollection.Hubs[0].Receiver.Methods[3].MethodName);
        Assert.True(serviceCollection.Hubs[0].Receiver.Methods[3].IsClientResult);
        Assert.Equal(3, serviceCollection.Hubs[0].Receiver.Methods[3].Parameters.Count());
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<string, int>>(), serviceCollection.Hubs[0].Receiver.Methods[3].RequestType); // Skip CancellationToken
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<string>(), serviceCollection.Hubs[0].Receiver.Methods[3].ResponseType);
        Assert.Equal(MagicOnionTypeInfo.CreateFromType<Task<string>>(), serviceCollection.Hubs[0].Receiver.Methods[3].MethodReturnType);
    }

    [Fact]
    public void Receiver_ReturnTypeIsNotVoidOrTask()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    int EventA();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedReceiverMethodReturnType.Id, diagnostics[0].Id);
    }

    [Fact]
    public void Receiver_ReturnTypeIsTask()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHubReceiver
{
    Task<int> EventA();
    ValueTask<int> EventB();
    Task EventC();
    ValueTask EventD();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void StreamingHubInterfaces_TwoOrMore()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>, IStreamingHub<IMyHub2, IMyHubReceiver>
{
    Task MethodA();
}

public interface IMyHub2 : IStreamingHub<IMyHub2, IMyHubReceiver>
{
    Task MethodB();
}

public interface IMyHubReceiver
{
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.Equal(1, diagnostics.Count());
        Assert.Equal(MagicOnionDiagnosticDescriptors.StreamingHubInterfaceHasTwoOrMoreIStreamingHub.Id, diagnostics[0].Id);
    }

    [Fact]
    public void InterfaceInheritance()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>, IExtraMethods
{
    Task MethodA();
}

public interface IExtraMethods : IExtraMethods2
{
    Task MethodExA();
}

public interface IExtraMethods2
{
    Task MethodExB();
}

public interface IMyHubReceiver
{
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Equal(3, serviceCollection.Hubs[0].Methods.Count());
    }
    
    [Fact]
    public void InterfaceInheritance_Receiver()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
}

public interface IMyHubReceiver : IExtraReceiverMethods
{
    void MethodA();
}

public interface IExtraReceiverMethods : IExtraReceiverMethods2
{
    void MethodExA();
}

public interface IExtraReceiverMethods2
{
    void MethodExB();
}
";
        var (compilation, semModel) = CompilationHelper.Create(source);
        if (!ReferenceSymbols.TryCreate(compilation, out var referenceSymbols)) throw new InvalidOperationException("Cannot create the reference symbols.");
        var interfaceSymbols = MethodCollectorTestHelper.Traverse(compilation.Assembly.GlobalNamespace).ToImmutableArray();

        // Act
        var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, CancellationToken.None);

        // Assert
        Assert.NotNull(serviceCollection);
        Assert.Equal(1, serviceCollection.Hubs.Count());
        Assert.Equal(3, serviceCollection.Hubs[0].Receiver.Methods.Count());
    }
}
