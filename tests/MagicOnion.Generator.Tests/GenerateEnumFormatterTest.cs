using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests
{
    public class GenerateEnumFormatterTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateEnumFormatterTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task GenerateEnumFormatter_Return()
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
        UnaryResult<MyEnum> GetEnumAsync();
    }

    public enum MyEnum
    {
        A, B, C
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
            symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));
        }


        [Fact]
        public async Task GenerateEnumFormatter_Return_Nullable()
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
        UnaryResult<MyEnum?> GetEnumAsync();
    }

    public enum MyEnum
    {
        A, B, C
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
            symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));
        }

        [Fact]
        public async Task GenerateEnumFormatter_Parameter()
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
        UnaryResult<Nil> GetEnumAsync(MyEnum a);
    }

    public enum MyEnum
    {
        A, B, C
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
            symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));
        }

        [Fact]
        public async Task GenerateEnumFormatter_Parameter_Nullable()
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
        UnaryResult<Nil> GetEnumAsync(MyEnum? a);
    }

    public enum MyEnum
    {
        A, B, C
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
            symbols.Should().Contain(x => x.Name.EndsWith("MyEnumFormatter"));
        }
    }
}
