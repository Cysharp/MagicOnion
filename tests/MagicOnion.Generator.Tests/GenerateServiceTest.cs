using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests
{
    public class GenerateServiceTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateServiceTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Return_UnaryResultOfT()
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
        }

        [Fact]
        public async Task Return_TaskOfUnaryResultOfT()
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
        Task<UnaryResult<int>> A();
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
        }


        [Fact]
        public async Task Return_StreamingResult()
        {
            using var tempWorkspace = TemporaryProjectWorkarea.Create();
            tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using MessagePack;
using MagicOnion;
using System.Threading.Tasks;

namespace TempProject
{
    public interface IMyService : IService<IMyService>
    {
        Task<ClientStreamingResult<string, string>> ClientStreamingAsync();
        Task<ServerStreamingResult<string>> ServerStreamingAsync();
        Task<DuplexStreamingResult<string, string>> DuplexStreamingAsync();
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
        }

        [Fact]
        public async Task Invalid_Return_NonGenerics()
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
        int A();
    }
}
            ");

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await compiler.GenerateFileAsync(
                    tempWorkspace.CsProjectPath,
                    Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                    true,
                    "TempProject.Generated",
                    "",
                    "MessagePack.Formatters"
                );
            });
        }

        [Fact]
        public async Task Invalid_Return_NonSupportedUnaryResultOfT()
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
        UnaryResult<ServerStreamingResult<int>> A();
    }
}
            ");

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await compiler.GenerateFileAsync(
                    tempWorkspace.CsProjectPath,
                    Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                    true,
                    "TempProject.Generated",
                    "",
                    "MessagePack.Formatters"
                );
            });
        }

        [Fact]
        public async Task Invalid_Return_RawStreaming_NonTask()
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
        ClientStreamingResult<string, string> ClientStreamingAsync();
        ServerStreamingResult<string> ServerStreamingAsync();
        DuplexStreamingResult<string, string> DuplexStreamingAsync();
    }
}
            ");

            var compiler = new MagicOnionCompiler(_testOutputHelper.WriteLine, CancellationToken.None);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await compiler.GenerateFileAsync(
                    tempWorkspace.CsProjectPath,
                    Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                    true,
                    "TempProject.Generated",
                    "",
                    "MessagePack.Formatters"
                );
            });
        }
    }
}
