using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests
{
    public class GenerateWithIfDirectiveTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateWithIfDirectiveTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Service_Interface()
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
        UnaryResult<int> A();
    }

    [GenerateIfDirective(""MYDEBUG || DEBUG"")]
    public interface IMyServiceForDebug : IService<IMyServiceForDebug>
    {
        UnaryResult<int> A();
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

            {
                var compilation = tempWorkspace.GetOutputCompilation();
                compilation.GetCompilationErrors().Should().BeEmpty();
                var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
                symbols.Should().Contain(x => x.Name == "MyServiceClient");
                symbols.Should().NotContain(x => x.Name == "MyServiceForDebugClient");
            }
            {
                var compilation = tempWorkspace.GetOutputCompilation(new [] { "MYDEBUG" });
                compilation.GetCompilationErrors().Should().BeEmpty();
                var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
                symbols.Should().Contain(x => x.Name == "MyServiceClient");
                symbols.Should().Contain(x => x.Name == "MyServiceForDebugClient");
            }
        }


        [Fact]
        public async Task GenerateDefineDebug_Service_Interface()
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
        UnaryResult<int> A();
    }

    [GenerateDefineDebug]
    public interface IMyServiceForDebug : IService<IMyServiceForDebug>
    {
        UnaryResult<int> A();
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

            {
                var compilation = tempWorkspace.GetOutputCompilation();
                compilation.GetCompilationErrors().Should().BeEmpty();
                var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
                symbols.Should().Contain(x => x.Name == "MyServiceClient");
                symbols.Should().NotContain(x => x.Name == "MyServiceForDebugClient");
            }
            {
                var compilation = tempWorkspace.GetOutputCompilation(new[] { "DEBUG" });
                compilation.GetCompilationErrors().Should().BeEmpty();
                var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
                symbols.Should().Contain(x => x.Name == "MyServiceClient");
                symbols.Should().Contain(x => x.Name == "MyServiceForDebugClient");
            }
        }

#if FALSE
        [Fact]
        public async Task Formatters()
        {
            using var tempWorkspace = TemporaryProjectWorkarea.Create();
            tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    [GenerateIfDirective(""MYDEBUG || DEBUG"")]
    public interface IMyServiceForDebug : IService<IMyServiceForDebug>
    {
        UnaryResult<MyReturnObject> A(MyGenericObject a, MyGenericObject b);
        UnaryResult<Nil> B(int a, int b);
    }

#if MYDEBUG || DEBUG
    [MessagePackObject]
    public class MyObject
    {
    }

    [MessagePackObject]
    public class MyReturnObject
    {
    }
#endif
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
            compilation.GetResolverKnownFormatterTypes().Should().BeEmpty();
        }
#endif
    }
}
