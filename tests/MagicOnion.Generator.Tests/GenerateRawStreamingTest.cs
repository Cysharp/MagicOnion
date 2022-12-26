using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests;

public class GenerateRawStreamingTest
{
    readonly ITestOutputHelper testOutputHelper;

    public GenerateRawStreamingTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task StreamingResult()
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
}
