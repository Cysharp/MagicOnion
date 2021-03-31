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
    public class GenerateStreamingHubTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateStreamingHubTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task HubReceiver_1()
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
        void OnMessage(MyObject a);
    }
    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task A(MyObject a);
    }

    [MessagePackObject]
    public class MyObject
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
            compilation.GetCompilationErrors().Should().BeEmpty();
        }

        [Fact]
        public async Task Return_Task()
        {
            using var tempWorkspace = TemporaryProjectWorkarea.Create();
            tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyHubReceiver { }
    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task A(MyObject a);
    }

    [MessagePackObject]
    public class MyObject
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
            compilation.GetCompilationErrors().Should().BeEmpty();
        }

        [Fact]
        public async Task Return_TaskOfT()
        {
            using var tempWorkspace = TemporaryProjectWorkarea.Create();
            tempWorkspace.AddFileToProject("IMyService.cs", @"
using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace TempProject
{
    public interface IMyHubReceiver { }
    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task<MyObject> A(MyObject a);
    }

    [MessagePackObject]
    public class MyObject
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
            compilation.GetCompilationErrors().Should().BeEmpty();
        }
    }
}
