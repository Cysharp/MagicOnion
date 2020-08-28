using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
        UnaryResult A(MyGenericObject<int> a);
        UnaryResult B(MyGenericObject<MyObject> a);
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
        UnaryResult A(MyGenericObject<int, MyObject> a);
        UnaryResult B(MyGenericObject<MyObject, int> a);
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
        UnaryResult A(MyGenericObject<MyGenericObject<MyObject>> a);
        UnaryResult B(MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>> a);
        UnaryResult B(MyGenericObject<MyGenericObject<MyGenericObject<int>>> a);
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
        UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<int>>>> B();
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
        public async Task ListFormatter_KnownType()
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
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.ListFormatter<string>",
                "global::MessagePack.Formatters.ListFormatter<int>"
            });
        }

        [Fact]
        public async Task ListFormatter_UserType()
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
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.ListFormatter<global::TempProject.MyResponse>",
            });
        }

        [Fact]
        public async Task ArrayFormatter_KnownType()
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
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().BeEmpty();
        }

        [Fact]
        public async Task ArrayFormatter_UserType()
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
            var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
            compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
            {
                "global::MessagePack.Formatters.ArrayFormatter<global::TempProject.MyResponse>"
            });
        }
    }
}
