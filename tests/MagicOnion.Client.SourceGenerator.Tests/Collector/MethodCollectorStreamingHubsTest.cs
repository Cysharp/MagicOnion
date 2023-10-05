#if FALSE
using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public class MethodCollectorStreamingHubsTest
{
    [Fact]
    public void FileScopedNamespace()
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Services.Should().BeEmpty();
        serviceCollection.Hubs[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyHub"));
        serviceCollection.Hubs[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Hubs[0].Methods.Should().HaveCount(1);
        // Task MethodA();
        serviceCollection.Hubs[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Hubs[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.CreateFromType<Nil>());
        serviceCollection.Hubs[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.CreateFromType<Nil>());
        serviceCollection.Hubs[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Hubs[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.CreateFromType<Task>());
    }

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().HaveCount(1);
        serviceCollection.Services.Should().BeEmpty();
        serviceCollection.Hubs[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyHub"));
        serviceCollection.Hubs[0].HasIfDirectiveCondition.Should().BeFalse();
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        var ex = Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        var ex = Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        var ex = Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
    }
    
    [Fact]
    public void IfDirectives()
    {
        // Arrange
        var source = @"
using System;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace MyNamespace;

[GenerateIfDirective(""DEBUG || CONST_1 || CONST_2"")]
public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    [GenerateDefineDebug]
    Task MethodA();
    [GenerateIfDirective(""CONST_3"")]
    Task MethodB();
    Task MethodC();
}

public interface IMyHubReceiver
{
    void EventA();
    void EventB(Nil nil);
    void EventC(string arg1, int arg2);
}
";
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Hubs[0].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Hubs[0].IfDirectiveCondition.Should().Be("DEBUG || CONST_1 || CONST_2");
        serviceCollection.Hubs[0].Methods[0].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Hubs[0].Methods[0].IfDirectiveCondition.Should().Be("DEBUG");
        serviceCollection.Hubs[0].Methods[1].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Hubs[0].Methods[1].IfDirectiveCondition.Should().Be("CONST_3");
        serviceCollection.Hubs[0].Methods[2].HasIfDirectiveCondition.Should().BeFalse();
    }

    [Fact]
    public void IfDirectives_Receiver()
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
    Task MethodB();
    Task MethodC();
}

[GenerateIfDirective(""DEBUG || CONST_1 || CONST_2"")]
public interface IMyHubReceiver
{
    [GenerateDefineDebug]
    void EventA();
    [GenerateIfDirective(""CONST_3"")]
    void EventB(Nil nil);
    void EventC(string arg1, int arg2);
}
";
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyHub.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Hubs[0].Receiver.HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Hubs[0].Receiver.IfDirectiveCondition.Should().Be("DEBUG || CONST_1 || CONST_2");
        serviceCollection.Hubs[0].Receiver.Methods[0].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Hubs[0].Receiver.Methods[0].IfDirectiveCondition.Should().Be("DEBUG");
        serviceCollection.Hubs[0].Receiver.Methods[1].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Hubs[0].Receiver.Methods[1].IfDirectiveCondition.Should().Be("CONST_3");
        serviceCollection.Hubs[0].Receiver.Methods[2].HasIfDirectiveCondition.Should().BeFalse();
    }     
}
#endif
