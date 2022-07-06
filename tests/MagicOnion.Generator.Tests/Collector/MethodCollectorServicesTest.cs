using System;
using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Generator.Tests.Collector;

public class MethodCollectorServicesTest
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

public interface IMyService : IService<IMyService>
{
    UnaryResult<Nil> NilAsync();
}
";
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // UnaryResult<Nil> NilAsync();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("NilAsync");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.Create("MessagePack", "Nil")));
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
public interface IMyService : IService<IMyService>
{
    [GenerateDefineDebug]
    UnaryResult<Nil> MethodA();

    [GenerateIfDirective(""CONST_3"")]
    UnaryResult<Nil> MethodB();

    UnaryResult<Nil> MethodC();
}
";
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Services[0].IfDirectiveCondition.Should().Be("DEBUG || CONST_1 || CONST_2");
        serviceCollection.Services[0].Methods[0].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Services[0].Methods[0].IfDirectiveCondition.Should().Be("DEBUG");
        serviceCollection.Services[0].Methods[1].HasIfDirectiveCondition.Should().BeTrue();
        serviceCollection.Services[0].Methods[1].IfDirectiveCondition.Should().Be("CONST_3");
        serviceCollection.Services[0].Methods[2].HasIfDirectiveCondition.Should().BeFalse();
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.CreateArray("System", "Tuple", MagicOnionTypeInfo.Create("System", "Boolean"), MagicOnionTypeInfo.Create("System", "Int64")));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.CreateArray("System", "Tuple", MagicOnionTypeInfo.Create("System", "Int32"), MagicOnionTypeInfo.Create("System", "String")));
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.CreateArray("System", "Tuple", MagicOnionTypeInfo.Create("System", "Int32"), MagicOnionTypeInfo.Create("System", "String"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("System", "Tuple", MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("System", "Boolean")), MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("System", "Int64"))));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("System", "Int32")));
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("System", "Int32"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // UnaryResult<Nil> MethodA();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.Create("MessagePack", "Nil")));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // UnaryResult<string> MethodA();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.Create("System", "String")));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // UnaryResult<Nil> MethodA(string arg1);
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Name.Should().Be("arg1");
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.Create("MessagePack", "Nil")));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // UnaryResult<Nil> MethodA(string arg1, int arg2);
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.Create("System", "String"), MagicOnionTypeInfo.Create("System", "Int32")));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters[1].Type.Should().Be( MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "UnaryResult", MagicOnionTypeInfo.Create("MessagePack", "Nil")));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Services[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters[0].HasExplicitDefaultValue.Should().BeTrue();
        serviceCollection.Services[0].Methods[0].Parameters[0].DefaultValue.Should().Be("\"Hello\"");
        serviceCollection.Services[0].Methods[0].Parameters[1].Type.Should().Be( MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].Parameters[1].DefaultValue.Should().Be("1234");
        serviceCollection.Services[0].Methods[0].Parameters[1].HasExplicitDefaultValue.Should().BeTrue();
        serviceCollection.Services[0].Methods[0].Parameters[2].Type.Should().Be( MagicOnionTypeInfo.Create("System", "Int64"));
        serviceCollection.Services[0].Methods[0].Parameters[2].DefaultValue.Should().Be("0");
        serviceCollection.Services[0].Methods[0].Parameters[2].HasExplicitDefaultValue.Should().BeTrue();
        serviceCollection.Services[0].Methods[0].Parameters[3].Type.Should().Be( MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters[3].DefaultValue.Should().Be("null");
        serviceCollection.Services[0].Methods[0].Parameters[3].HasExplicitDefaultValue.Should().BeTrue();
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(4);
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // Task<ServerStreamingResult<int>> ServerStreamingNoArg();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("ServerStreaming");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("MessagePack", "Nil"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("System.Threading.Tasks", "Task", MagicOnionTypeInfo.Create("MagicOnion", "ServerStreamingResult", MagicOnionTypeInfo.Create("System", "Int32"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // Task<ServerStreamingResult<int>> ServerStreamingNoArg();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("ServerStreaming");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Name.Should().Be("arg1");
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("System.Threading.Tasks", "Task", MagicOnionTypeInfo.Create("MagicOnion", "ServerStreamingResult", MagicOnionTypeInfo.Create("System", "Int32"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // Task<ServerStreamingResult<int>> ServerStreamingNoArg();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("ServerStreaming");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.Create("System", "String"), MagicOnionTypeInfo.Create("System", "Int32")));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Type.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters[0].Name.Should().Be("arg1");
        serviceCollection.Services[0].Methods[0].Parameters[1].Type.Should().Be(MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].Parameters[1].Name.Should().Be("arg2");
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("System.Threading.Tasks", "Task", MagicOnionTypeInfo.Create("MagicOnion", "ServerStreamingResult", MagicOnionTypeInfo.Create("System", "Int32"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // Task<DuplexStreamingResult<int, string>> MethodA();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("System.Threading.Tasks", "Task",
            MagicOnionTypeInfo.Create("MagicOnion", "DuplexStreamingResult", 
                MagicOnionTypeInfo.Create("System", "Int32"), MagicOnionTypeInfo.Create("System", "String"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act
        var collector = new MethodCollector();
        var serviceCollection = collector.Collect(compilation);

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Hubs.Should().BeEmpty();
        serviceCollection.Services.Should().HaveCount(1);
        serviceCollection.Services[0].ServiceType.Should().Be(MagicOnionTypeInfo.Create("MyNamespace", "IMyService"));
        serviceCollection.Services[0].HasIfDirectiveCondition.Should().BeFalse();
        serviceCollection.Services[0].Methods.Should().HaveCount(1);
        // Task<DuplexStreamingResult<int, string>> MethodA();
        serviceCollection.Services[0].Methods[0].ServiceName.Should().Be("IMyService");
        serviceCollection.Services[0].Methods[0].MethodName.Should().Be("MethodA");
        serviceCollection.Services[0].Methods[0].RequestType.Should().Be(MagicOnionTypeInfo.Create("System", "Int32"));
        serviceCollection.Services[0].Methods[0].ResponseType.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        serviceCollection.Services[0].Methods[0].Parameters.Should().BeEmpty();
        serviceCollection.Services[0].Methods[0].MethodReturnType.Should().Be(MagicOnionTypeInfo.Create("System.Threading.Tasks", "Task",
            MagicOnionTypeInfo.Create("MagicOnion", "ClientStreamingResult", 
                MagicOnionTypeInfo.Create("System", "Int32"), MagicOnionTypeInfo.Create("System", "String"))));
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
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", source);
        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

        // Act & Assert
        var collector = new MethodCollector();
        Assert.Throws<InvalidOperationException>(() => collector.Collect(compilation));
    }

    
//    [Fact]
//    public void ImplicitUsings()
//    {
//        // Arrange
//        var source = @"
//using MagicOnion;
//using MessagePack;

//namespace MyNamespace;

//public interface IMyService : IService<IMyService>
//{
//    UnaryResult<Nil> NilAsync();
//    UnaryResult<string> StringAsync();
//    UnaryResult<Nil> OneParameter(string arg1);
//    UnaryResult<Nil> TwoParameter(string arg1, int arg2);
//    Task<ServerStreamingResult<int>> ServerStreaming(string arg1, int arg2);
//}
//";
//        using var tempWorkspace = TemporaryProjectWorkarea.Create();
//        tempWorkspace.AddFileToProject("IMyService.cs", source);
//        var compilation = tempWorkspace.GetOutputCompilation().Compilation;

//        // Act
//        var collector = new MethodCollector2();
//        collector.Collect(compilation);

//        // Assert
//    }
}
