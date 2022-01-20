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
        public async Task StreamingHub_Interface()
        {
            using var tempWorkspace = TemporaryProjectWorkarea.Create();
            tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyHubReceiver
    {
        void OnMessage(int a);
    }
    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task A(int a);
    }

    [GenerateIfDirective(""MYDEBUG || DEBUG"")]
    public interface IMyDebugHub : IStreamingHub<IMyDebugHub, IMyHubReceiver>
    {
        Task A(int a);
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
                symbols.Should().Contain(x => x.Name == "MyHubClient");
                symbols.Should().NotContain(x => x.Name == "MyDebugHubClient");
            }
            {
                var compilation = tempWorkspace.GetOutputCompilation(new[] { "MYDEBUG" });
                compilation.GetCompilationErrors().Should().BeEmpty();
                var symbols = compilation.GetNamedTypeSymbolsFromGenerated();
                symbols.Should().Contain(x => x.Name == "MyHubClient");
                symbols.Should().Contain(x => x.Name == "MyDebugHubClient");
            }
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
        UnaryResult<MyReturnObject> A(MyDebugObject a, MyDebugObject b);
        UnaryResult<Nil> B(MyObject a, MyDebugObject b);
    }

    [GenerateIfDirective(""CUSTOM_DEBUG"")]
    public interface IMyServiceForCustomDebug : IService<IMyServiceForCustomDebug>
    {
        UnaryResult<MyReturnObject> A(MyDebugObject a, MyDebugObjectForCustomDebug b);
    }

    [MessagePackObject]
    public class MyDebugObject
    {
    }

    [MessagePackObject]
    public class MyDebugObjectForCustomDebug
    {
    }

    [MessagePackObject]
    public class MyDebugReturnObject
    {
    }

    public interface IMyService : IService<IMyService>
    {
        UnaryResult<MyReturnObject> A(MyObject a, MyObject b);
    }

    [MessagePackObject]
    public class MyObject
    {
    }
    [MessagePackObject]
    public class MyReturnObject
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

            {
                var compilation = tempWorkspace.GetOutputCompilation();
                compilation.GetCompilationErrors().Should().BeEmpty();
                compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
                {
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyObject, global::TempProject.MyObject>",
                });
                compilation.GetResolverKnownFormatterTypes().Should().NotContain(new[]
                {
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyDebugObject, global::TempProject.MyDebugObject>",
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyDebugObject, global::TempProject.MyDebugObjectForCustomDebug>",
                });
            }

            {
                var compilation = tempWorkspace.GetOutputCompilation(new[] { "MYDEBUG" });
                compilation.GetCompilationErrors().Should().BeEmpty();
                compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
                {
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyObject, global::TempProject.MyObject>",
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyDebugObject, global::TempProject.MyDebugObject>",
                });
                compilation.GetResolverKnownFormatterTypes().Should().NotContain(new[]
                {
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyDebugObject, global::TempProject.MyDebugObjectForCustomDebug>",
                });
            }
            {
                var compilation = tempWorkspace.GetOutputCompilation(new[] { "CUSTOM_DEBUG" });
                compilation.GetCompilationErrors().Should().BeEmpty();
                compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
                {
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyDebugObject, global::TempProject.MyDebugObjectForCustomDebug>",
                });
                compilation.GetResolverKnownFormatterTypes().Should().NotContain(new[]
                {
                    "global::MagicOnion.DynamicArgumentTupleFormatter<global::TempProject.MyDebugObject, global::TempProject.MyDebugObject>",
                });
            }
        }

        [Fact]
        public async Task Generics()
        {
            using var tempWorkspace = TemporaryProjectWorkarea.Create();
            tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    [GenerateIfDirective(""MYDEBUG || DEBUG"")]
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

            {
                var compilation = tempWorkspace.GetOutputCompilation();
                compilation.GetResolverKnownFormatterTypes().Should().NotContain(new[]
                {
                    "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int>",
                    "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject>",
                });
            }
            {
                var compilation = tempWorkspace.GetOutputCompilation(new [] { "MYDEBUG" });
                compilation.GetResolverKnownFormatterTypes().Should().Contain(new[]
                {
                    "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<int>",
                    "global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyObject>",
                });
            }
        }
    }
}
