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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Services.Should().BeEmpty();
        serviceCollection.Hubs[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyHub"));
        serviceCollection.Hubs[0].Methods.Should().HaveCount(2);
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[1].MethodName.Should().Be("MethodC");
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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().BeEmpty();
        serviceCollection.Hubs.Should().BeEmpty();
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.CreateFromType<Nil>());
        serviceCollection.Hubs[0].Methods[0].Parameters.Should().BeEmpty();
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.CreateFromType<int>());
        serviceCollection.Hubs[0].Methods[0].Parameters.Should().HaveCount(1);
        serviceCollection.Hubs[0].Methods[0].Parameters[0].Name.Should().Be("arg1");
        serviceCollection.Hubs[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.CreateFromType<int>());
        serviceCollection.Hubs[0].Methods[0].Parameters[0].HasExplicitDefaultValue.Should().BeFalse();
        serviceCollection.Hubs[0].Methods[0].Parameters[0].DefaultValue.Should().Be("default(int)");
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<int, string>>());
        serviceCollection.Hubs[0].Methods[0].Parameters.Should().HaveCount(2);
        serviceCollection.Hubs[0].Methods[0].Parameters[0].Name.Should().Be("arg1");
        serviceCollection.Hubs[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.CreateFromType<int>());
        serviceCollection.Hubs[0].Methods[0].Parameters[0].HasExplicitDefaultValue.Should().BeFalse();
        serviceCollection.Hubs[0].Methods[0].Parameters[0].DefaultValue.Should().Be("default(int)");
        serviceCollection.Hubs[0].Methods[0].Parameters[1].Name.Should().Be("arg2");
        serviceCollection.Hubs[0].Methods[0].Parameters[1].Type.Should().Be(MagicOnionTypeInfo.CreateFromType<string>());
        serviceCollection.Hubs[0].Methods[0].Parameters[1].HasExplicitDefaultValue.Should().BeFalse();
        serviceCollection.Hubs[0].Methods[0].Parameters[1].DefaultValue.Should().Be("default(string)");
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<int, string>>());
        serviceCollection.Hubs[0].Methods[0].Parameters.Should().HaveCount(2);
        serviceCollection.Hubs[0].Methods[0].Parameters[0].Name.Should().Be("arg1");
        serviceCollection.Hubs[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.CreateFromType<int>());
        serviceCollection.Hubs[0].Methods[0].Parameters[0].HasExplicitDefaultValue.Should().BeFalse();
        serviceCollection.Hubs[0].Methods[0].Parameters[0].DefaultValue.Should().Be("default(int)");
        serviceCollection.Hubs[0].Methods[0].Parameters[1].Name.Should().Be("arg2");
        serviceCollection.Hubs[0].Methods[0].Parameters[1].Type.Should().Be(MagicOnionTypeInfo.CreateFromType<string>());
        serviceCollection.Hubs[0].Methods[0].Parameters[1].HasExplicitDefaultValue.Should().BeTrue();
        serviceCollection.Hubs[0].Methods[0].Parameters[1].DefaultValue.Should().Be("\"DEFAULT\"");
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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Hubs[0].Methods.Should().HaveCount(1);
        // Task MethodA();
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].HubId.Should().Be(1497325507);
        // void EventA();
        serviceCollection.Hubs[0].Receiver.Methods[0].MethodName.Should().Be("EventA");
        serviceCollection.Hubs[0].Receiver.Methods[0].HubId.Should().Be(842297178);
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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Hubs[0].Methods.Should().HaveCount(1);
        // Task MethodA();
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].HubId.Should().Be(12345);
        // void EventA();
        serviceCollection.Hubs[0].Receiver.Methods[0].MethodName.Should().Be("EventA");
        serviceCollection.Hubs[0].Receiver.Methods[0].HubId.Should().Be(67890);
    }

    [Fact]
    public void ReturnType_NotSupported_Void()
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
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be(MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedMethodReturnType.Id);
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
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be(MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedMethodReturnType.Id);
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_Threading_Tasks_Task);
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_String);
        serviceCollection.Hubs[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.CreateFromType<Task<string>>());
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_Threading_Tasks_ValueTask);
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
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_String);
        serviceCollection.Hubs[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.CreateFromType<ValueTask<string>>());
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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Hubs[0].Methods.Should().HaveCount(1);
        serviceCollection.Hubs[0].Receiver.Should().NotBeNull();
        serviceCollection.Hubs[0].Receiver.Methods.Should().HaveCount(3);
        // void EventA();
        serviceCollection.Hubs[0].Receiver.Methods[0].MethodName.Should().Be("EventA");
        serviceCollection.Hubs[0].Receiver.Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Hubs[0].Receiver.Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Receiver.Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Receiver.Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_Void);
        // void EventB(Nil nil);
        serviceCollection.Hubs[0].Receiver.Methods[1].MethodName.Should().Be("EventB");
        serviceCollection.Hubs[0].Receiver.Methods[1].Parameters.Should().HaveCount(1);
        serviceCollection.Hubs[0].Receiver.Methods[1].RequestType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Receiver.Methods[1].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Receiver.Methods[1].MethodReturnType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_Void);
        // void EventB(Nil nil);
        serviceCollection.Hubs[0].Receiver.Methods[2].MethodName.Should().Be("EventC");
        serviceCollection.Hubs[0].Receiver.Methods[2].Parameters.Should().HaveCount(2);
        serviceCollection.Hubs[0].Receiver.Methods[2].RequestType.Should().Be(MagicOnionTypeInfo.CreateFromType<DynamicArgumentTuple<string, int>>());
        serviceCollection.Hubs[0].Receiver.Methods[2].ResponseType.Should().Be(MagicOnionTypeInfo.KnownTypes.MessagePack_Nil);
        serviceCollection.Hubs[0].Receiver.Methods[2].MethodReturnType.Should().Be(MagicOnionTypeInfo.KnownTypes.System_Void);
    }

    [Fact]
    public void Receiver_NonVoidReturnType()
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
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be(MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedReceiverMethodReturnType.Id);
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
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be(MagicOnionDiagnosticDescriptors.StreamingHubInterfaceHasTwoOrMoreIStreamingHub.Id);
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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Hubs[0].Methods.Should().HaveCount(3);
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
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Hubs[0].Receiver.Methods.Should().HaveCount(3);
    }
}
