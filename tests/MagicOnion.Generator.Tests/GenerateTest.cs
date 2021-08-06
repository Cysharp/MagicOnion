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
    public class GenerateTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private const string MyServiceSourceCode = @"
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
            ";

        public GenerateTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task CsProjContainsAnalyzerReferenceUpdate()
        {
            var options = TemporaryProjectWorkareaOptions.Default with
            {
                AdditionalCsProjectContent = @"
                   <ItemGroup>
                    <PackageReference Update=""MessagePackAnalyzer"" Version=""2.2.85"">
                      <PrivateAssets>all</PrivateAssets>
                      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
                    </PackageReference>
                  </ItemGroup>
                ",
            };
            using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
            tempWorkspace.AddFileToProject("IMyService.cs", MyServiceSourceCode);

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
        public async Task CsProjContainsAnalyzerReferenceExclude()
        {
            var options = TemporaryProjectWorkareaOptions.Default with
            {
                AdditionalCsProjectContent = @"
                   <ItemGroup>
                    <PackageReference Exclude=""MessagePackAnalyzer"" />
                  </ItemGroup>
                ",
            };
            using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
            tempWorkspace.AddFileToProject("IMyService.cs", MyServiceSourceCode);

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
        public async Task CsProjContainsAnalyzerReferenceRemove()
        {
            var options = TemporaryProjectWorkareaOptions.Default with
            {
                AdditionalCsProjectContent = @"
                   <ItemGroup>
                    <PackageReference Remove=""MessagePackAnalyzer"" />
                  </ItemGroup>
                ",
            };
            using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
            tempWorkspace.AddFileToProject("IMyService.cs", MyServiceSourceCode);

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
        public async Task CsProjTargetsNet5()
        {
            var options = TemporaryProjectWorkareaOptions.Default with
            {
                TargetFramework = "net5.0",
            };
            using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
            tempWorkspace.AddFileToProject("IMyService.cs", MyServiceSourceCode);

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