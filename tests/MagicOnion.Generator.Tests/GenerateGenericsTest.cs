using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests;

public class GenerateGenericsTest
{
    readonly ITestOutputHelper testOutputHelper;

    public GenerateGenericsTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Parameters()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> A(MyGenericObject<int> a);
        UnaryResult<Nil> B(MyGenericObject<MyObject> a);
    }

    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::System.Int32>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject>",
        });
    }

    [Fact]
    public async Task Parameters_MultipleTypeArgs()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> A(MyGenericObject<int, MyObject> a);
        UnaryResult<Nil> B(MyGenericObject<MyObject, int> a);
    }

    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyGenericObject<T1, T2>
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::System.Int32, global::TempProject.MyObject>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject, global::System.Int32>",
        });
    }

    [Fact]
    public async Task Parameters_Nested()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> A(MyGenericObject<MyGenericObject<MyObject>> a);
        UnaryResult<Nil> B(MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>> a);
        UnaryResult<Nil> B(MyGenericObject<MyGenericObject<MyGenericObject<int>>> a);
    }

    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyObject>>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::System.Int32>>>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::System.Int32>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject>",
        });
    }


    [Fact]
    public async Task Parameters_Nested_Enum()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> GetEnumAsync(MyGenericObject<MyGenericObject<MyEnum>> arg0);
    }

    public enum MyEnum
    {
        A, B, C
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));

        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyEnum>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyEnum>>",
        });
    }

    [Fact]
    public async Task Parameters_Nested_Array()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> GetValuesAsync(MyGenericObject<MyNestedGenericObject[]> arg0);
    }

    public class MyGenericObject<T>
    {
    }

    public class MyNestedGenericObject
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyNestedGenericObject[]>",
            "global::MessagePack.Formatters.ArrayFormatter<global::TempProject.MyNestedGenericObject>"
        });
    }

    [Fact]
    public async Task Parameters_ListFormatter_KnownType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> GetStringValuesAsync(List<string> arg0);
        UnaryResult<Nil> GetIntValuesAsync(List<int> arg0);
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ListFormatter<global::System.String>",
            "global::MessagePack.Formatters.ListFormatter<global::System.Int32>"
        });
    }

    [Fact]
    public async Task Parameters_ListFormatter_UserType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> GetValuesAsync(List<MyResponse> arg0);
    }
    public class MyResponse
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ListFormatter<global::TempProject.MyResponse>",
        });
    }

    [Fact]
    public async Task Parameters_ArrayFormatter_KnownType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> GetStringValuesAsync(string[] arg0);
        UnaryResult<Nil> GetIntValuesAsync(int[] arg0);
        UnaryResult<Nil> GetInt32ValuesAsync(Int32[] arg0);
        UnaryResult<Nil> GetSingleValuesAsync(float[] arg0);
        UnaryResult<Nil> GetBooleanValuesAsync(bool[] arg0);
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().BeEmpty();
    }

    [Fact]
    public async Task Parameters_ArrayFormatter_UserType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<Nil> GetValuesAsync(MyResponse[] arg0);
    }

    public class MyResponse
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ArrayFormatter<global::TempProject.MyResponse>"
        });
    }

    [Fact]
    public async Task Return()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyGenericObject<int>> A();
        UnaryResult<MyGenericObject<MyObject>> B();
    }

    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}

// Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
namespace MessagePack.Formatters.TempProject
{
    public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
    {
        public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
        public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::System.Int32>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject>",
        });
    }

    [Fact]
    public async Task Return_Nested()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyGenericObject<MyGenericObject<MyObject>>> A();
        UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>>> B();
        UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<int>>>> C();
    }

    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}

// Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
namespace MessagePack.Formatters.TempProject
{
    public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
    {
        public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
        public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyObject>>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::System.Int32>>>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::System.Int32>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject>",
        });
    }

    [Fact]
    public async Task Return_MultipleTypeArgs()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyGenericObject<int, MyObject>> A();
        UnaryResult<MyGenericObject<MyObject, int>> B();
    }

    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyGenericObject<T1, T2>
    {
    }
}

// Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
namespace MessagePack.Formatters.TempProject
{
    public class MyGenericObjectFormatter<T1, T2> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T1, T2>>
    {
        public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T1, T2> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
        public global::TempProject.MyGenericObject<T1, T2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::System.Int32, global::TempProject.MyObject>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject, global::System.Int32>",
        });
    }

    [Fact]
    public async Task Return_Enum()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyGenericObject<MyEnum>> GetEnumAsync();
    }

    public enum MyEnum
    {
        A, B, C
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}

// Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
namespace MessagePack.Formatters.TempProject
{
    public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
    {
        public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
        public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));

        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyEnum>",
        });
    }

    [Fact]
    public async Task Return_Nested_Enum()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyGenericObject<MyGenericObject<MyEnum>>> GetEnumAsync();
    }

    public enum MyEnum
    {
        A, B, C
    }

    [MessagePackObject]
    public class MyGenericObject<T>
    {
    }
}

// Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
namespace MessagePack.Formatters.TempProject
{
    public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
    {
        public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
        public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));

        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyEnum>",
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyEnum>>",
        });
    }

    [Fact]
    public async Task Return_Nested_Array()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyGenericObject<MyNestedGenericObject[]>> GetValuesAsync();
    }

    public class MyGenericObject<T>
    {
    }

    public class MyNestedGenericObject
    {
    }
}

// Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
namespace MessagePack.Formatters.TempProject
{
    public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
    {
        public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
        public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyNestedGenericObject[]>",
            "global::MessagePack.Formatters.ArrayFormatter<global::TempProject.MyNestedGenericObject>"
        });
    }

    [Fact]
    public async Task Return_ListFormatter_KnownType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<List<string>> GetStringValuesAsync();
        UnaryResult<List<int>> GetIntValuesAsync();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ListFormatter<global::System.String>",
            "global::MessagePack.Formatters.ListFormatter<global::System.Int32>"
        });
    }

    [Fact]
    public async Task Return_ListFormatter_UserType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<List<MyResponse>> GetValuesAsync();
    }
    public class MyResponse
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ListFormatter<global::TempProject.MyResponse>",
        });
    }

    [Fact]
    public async Task Return_ArrayFormatter_KnownType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<string[]> GetStringValuesAsync();
        UnaryResult<int[]> GetIntValuesAsync();
        UnaryResult<Int32[]> GetInt32ValuesAsync();
        UnaryResult<float[]> GetSingleValuesAsync();
        UnaryResult<bool[]> GetBooleanValuesAsync();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().BeEmpty();
    }

    [Fact]
    public async Task Return_ArrayFormatter_UserType()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyResponse[]> GetValuesAsync();
    }

    public class MyResponse
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ArrayFormatter<global::TempProject.MyResponse>"
        });
    }

        
    [Fact]
    public async Task KnownFormatters()
    {
        using var tempWorkspace = TemporaryProjectWorkarea.Create();
        tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        UnaryResult<List<MyResponse>>                        MethodList(List<int> args);
        UnaryResult<IList<MyResponse>>                       MethodIList();
        UnaryResult<IReadOnlyList<MyResponse>>               MethodIROList();

        UnaryResult<Dictionary<string, MyResponse>>          MethodDictionary();
        UnaryResult<IDictionary<string, MyResponse>>         MethodIDictionary();
        UnaryResult<IReadOnlyDictionary<string, MyResponse>> MethodIRODictionary();

        UnaryResult<IEnumerable<MyResponse>>                 MethodIEnumerable();
        UnaryResult<ICollection<MyResponse>>                 MethodICollection();
        UnaryResult<IReadOnlyCollection<MyResponse>>         MethodIROCollection();

        UnaryResult<ILookup<int, MyResponse>>                MethodILookup();
        UnaryResult<IGrouping<int, MyResponse>>              MethodIGrouping();
    }
    public class MyResponse
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            tempWorkspace.OutputDirectory,
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        );

        var compilation = tempWorkspace.GetOutputCompilation();
        compilation.GetCompilationErrors().Should().BeEmpty();
        var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
        compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
        {
            "global::MessagePack.Formatters.ListFormatter<global::System.Int32>",
            "global::MessagePack.Formatters.ListFormatter<global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceListFormatter2<global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceReadOnlyListFormatter<global::TempProject.MyResponse>",

            "global::MessagePack.Formatters.DictionaryFormatter<global::System.String, global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceDictionaryFormatter<global::System.String, global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceReadOnlyDictionaryFormatter<global::System.String, global::TempProject.MyResponse>",

            "global::MessagePack.Formatters.InterfaceEnumerableFormatter<global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceCollectionFormatter2<global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceReadOnlyCollectionFormatter<global::TempProject.MyResponse>",

            "global::MessagePack.Formatters.InterfaceLookupFormatter<global::System.Int32, global::TempProject.MyResponse>",
            "global::MessagePack.Formatters.InterfaceGroupingFormatter<global::System.Int32, global::TempProject.MyResponse>",

        });
    }
}
