using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MessagePack;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests
{
    public class GenerateGenericsTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateGenericsTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int>",
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int, global::TempProject.MyObject>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject, int>",
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyObject>>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<int>>>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int>",
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.ListFormatter<string>",
                "global::MessagePack.Formatters.ListFormatter<int>"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetCompilationErrors().Should().BeEmpty();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int>",
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetCompilationErrors().Should().BeEmpty();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyObject>>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyObject>>>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<int>>>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int>",
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetCompilationErrors().Should().BeEmpty();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int, global::TempProject.MyObject>",
                "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject, int>",
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetCompilationErrors().Should().BeEmpty();
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.ListFormatter<string>",
                "global::MessagePack.Formatters.ListFormatter<int>"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
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

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                tempWorkspace.OutputDirectory,
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters"
            );

            var compilation = tempWorkspace.GetOutputCompilation();
            compilation.GetCompilationErrors().Should().BeEmpty();
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.ArrayFormatter<global::TempProject.MyResponse>"
            });
        }
    }
}
