using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests;

public class GenerateServiceTest
{
    readonly ITestOutputHelper testOutputHelper;

    public GenerateServiceTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Return_UnaryResultNonGeneric()
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
        UnaryResult A();
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
    }
        
    [Fact]
    public async Task Return_UnaryResultOfValueType()
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
        UnaryResult<long> A();
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
    }
                
    [Fact]
    public async Task Return_UnaryResultOfRefType()
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
        UnaryResult<string> A();
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
    }

    [Fact]
    public async Task Invalid_Return_TaskOfUnaryResultOfT()
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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>await compiler.GenerateFileAsync(
            tempWorkspace.CsProjectPath,
            Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
            true,
            "TempProject.Generated",
            "",
            "MessagePack.Formatters",
            SerializerType.MessagePack
        ));

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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters",
            SerializerType.MessagePack
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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters",
            SerializerType.MessagePack
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

        var compiler = new MagicOnionCompiler(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await compiler.GenerateFileAsync(
                tempWorkspace.CsProjectPath,
                Path.Combine(tempWorkspace.OutputDirectory, "Generated.cs"),
                true,
                "TempProject.Generated",
                "",
                "MessagePack.Formatters",
                SerializerType.MessagePack
            );
        });
    }
}
