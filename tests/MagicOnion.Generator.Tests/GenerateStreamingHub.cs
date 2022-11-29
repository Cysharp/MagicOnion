using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests;

public class GenerateStreamingHubTest
{
    readonly ITestOutputHelper testOutputHelper;

    public GenerateStreamingHubTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
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
    public async Task Invalid_Return_Void()
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
        void A();
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);

        var ex = await Record.ExceptionAsync(async () => await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters"
        ));

        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
        ex.Message.Should().Contain("IMyHub.A' has unsupported return type");
    }


    [Fact]
    public async Task Invalid_HubReceiver_ReturnsNotVoid()
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
        Task B();
    }
    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
    }
}
            ");

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);

        var ex = await Record.ExceptionAsync(async () => await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters"
        ));

        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidOperationException>();
        ex.Message.Should().Contain("IMyHubReceiver.B' has unsupported return type");
    }

}
