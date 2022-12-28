using MagicOnion.Generator.Utils;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests;

public class GenerateTest
{
    readonly ITestOutputHelper testOutputHelper;

    const string MyServiceSourceCode = @"
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
        this.testOutputHelper = testOutputHelper;
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

        var outputCompilation = tempWorkspace.GetOutputCompilation();
        outputCompilation.GetCompilationErrors().Should().BeEmpty();
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

        var outputCompilation = tempWorkspace.GetOutputCompilation();
        outputCompilation.GetCompilationErrors().Should().BeEmpty();
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

        var outputCompilation = tempWorkspace.GetOutputCompilation();
        outputCompilation.GetCompilationErrors().Should().BeEmpty();
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

        var outputCompilation = tempWorkspace.GetOutputCompilation();
        outputCompilation.GetCompilationErrors().Should().BeEmpty();
    }

    [Fact]
    public async Task ImplicitUsings_PropertyGroup_Enable()
    {
        var options = TemporaryProjectWorkareaOptions.Default with
        {
            ImplicitUsings = true,
        };
        using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
        tempWorkspace.AddFileToProject("IMyService.cs", """
            using MagicOnion;
            using MessagePack;
            
            namespace MyNamespace;
            
            public interface IMyService : IService<IMyService>
            {
                // ImplicitUsings: Microsoft.NET.Sdk
                // global::System
                // global::System.Collections.Generic
                // global::System.IO
                // global::System.Linq
                // global::System.Net.Http
                // global::System.Threading
                // global::System.Threading.Tasks
                UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
            }
        """);

        // Try create a compilation from the temporary project.
        var sourceProjectCompilation = await PseudoCompilation.CreateFromProjectAsync(
            new[] { tempWorkspace.CsProjectPath },
            Array.Empty<string>(),
            new MagicOnionGeneratorTestOutputLogger(testOutputHelper),
            CancellationToken.None
        );

        // Generate codes and get a compilation of the project with generated code.
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
        var outputCompilation = tempWorkspace.GetOutputCompilation();

        // Assert
        sourceProjectCompilation.GetDiagnostics().Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);
        outputCompilation.GetCompilationErrors().Should().BeEmpty();
    }
    
    [Fact]
    public async Task ImplicitUsings_PropertyGroup_Disable()
    {
        var options = TemporaryProjectWorkareaOptions.Default with
        {
            ImplicitUsings = false,
        };
        using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
        tempWorkspace.AddFileToProject("IMyService.cs", """
            using MagicOnion;
            using MessagePack;
            
            namespace MyNamespace;
            
            public interface IMyService : IService<IMyService>
            {
                // ImplicitUsings: Microsoft.NET.Sdk
                // global::System
                // global::System.Collections.Generic
                // global::System.IO
                // global::System.Linq
                // global::System.Net.Http
                // global::System.Threading
                // global::System.Threading.Tasks
                UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
            }
        """);

        // Try create a compilation from the temporary project.
        var sourceProjectCompilation = await PseudoCompilation.CreateFromProjectAsync(
            new[] { tempWorkspace.CsProjectPath },
            Array.Empty<string>(),
            new MagicOnionGeneratorTestOutputLogger(testOutputHelper),
            CancellationToken.None
        );

        // Generate codes and get a compilation of the project with generated code.
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
        var outputCompilation = tempWorkspace.GetOutputCompilation();

        // Assert
        sourceProjectCompilation.GetDiagnostics().Should().Contain(x => x.Severity == DiagnosticSeverity.Error); // Failed to compile the project.
        outputCompilation.GetCompilationErrors().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GlobalUsingsInProject()
    {
        var options = TemporaryProjectWorkareaOptions.Default with
        {
            ImplicitUsings = false,
            Usings = new []
            {
                (Namespace: "System", Static: false, Remove: false),
                (Namespace: "System.Collections.Generic", Static: false, Remove: false),
                (Namespace: "System.IO", Static: false, Remove: false),
                (Namespace: "System.Linq", Static: false, Remove: false),
                (Namespace: "System.Net.Http", Static: false, Remove: false),
                (Namespace: "System.Threading", Static: false, Remove: false),
                (Namespace: "System.Threading.Tasks", Static: false, Remove: false),

                (Namespace: "MagicOnion", Static: false, Remove: false),
                (Namespace: "MessagePack", Static: false, Remove: false),
            },
        };
        using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
        tempWorkspace.AddFileToProject("IMyService.cs", """
            namespace MyNamespace;
            
            public interface IMyService : IService<IMyService>
            {
                // ImplicitUsings: Microsoft.NET.Sdk
                // global::System
                // global::System.Collections.Generic
                // global::System.IO
                // global::System.Linq
                // global::System.Net.Http
                // global::System.Threading
                // global::System.Threading.Tasks
                UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
            }
        """);

        // Try create a compilation from the temporary project.
        var sourceProjectCompilation = await PseudoCompilation.CreateFromProjectAsync(
            new[] { tempWorkspace.CsProjectPath },
            Array.Empty<string>(),
            new MagicOnionGeneratorTestOutputLogger(testOutputHelper),
            CancellationToken.None
        );
        
        // Generate codes and get a compilation of the project with generated code.
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
        var outputCompilation = tempWorkspace.GetOutputCompilation();

        // Assert
        sourceProjectCompilation.GetDiagnostics().Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);
        outputCompilation.GetCompilationErrors().Should().BeEmpty();
    }
    
    [Fact]
    public async Task GlobalUsingsInProject_Remove()
    {
        var options = TemporaryProjectWorkareaOptions.Default with
        {
            ImplicitUsings = false,
            Usings = new []
            {
                (Namespace: "System", Static: false, Remove: false),
                (Namespace: "System.Collections.Generic", Static: false, Remove: false),
                (Namespace: "System.IO", Static: false, Remove: false),
                (Namespace: "System.Linq", Static: false, Remove: false),
                (Namespace: "System.Net.Http", Static: false, Remove: false),
                (Namespace: "System.Threading", Static: false, Remove: false),
                (Namespace: "System.Threading.Tasks", Static: false, Remove: false),

                (Namespace: "MagicOnion", Static: false, Remove: false),
                (Namespace: "MessagePack", Static: false, Remove: false),

                // Remove
                (Namespace: "MagicOnion", Static: false, Remove: true),
            },
        };
        using var tempWorkspace = TemporaryProjectWorkarea.Create(options);
        tempWorkspace.AddFileToProject("IMyService.cs", """
            namespace MyNamespace;
            
            public interface IMyService : IService<IMyService>
            {
                // ImplicitUsings: Microsoft.NET.Sdk
                // global::System
                // global::System.Collections.Generic
                // global::System.IO
                // global::System.Linq
                // global::System.Net.Http
                // global::System.Threading
                // global::System.Threading.Tasks
                UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
            }
        """);

        // Try create a compilation from the temporary project.
        var sourceProjectCompilation = await PseudoCompilation.CreateFromProjectAsync(
            new[] { tempWorkspace.CsProjectPath },
            Array.Empty<string>(),
            new MagicOnionGeneratorTestOutputLogger(testOutputHelper),
            CancellationToken.None
        );

        // Assert
        sourceProjectCompilation.GetDiagnostics().Should().Contain(x => x.Severity == DiagnosticSeverity.Error); // the compilation should have some errors.
    }
}
