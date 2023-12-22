using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MagicOnion.Client.SourceGenerator.Tests;

// ref: https://www.meziantou.net/testing-roslyn-incremental-source-generators.htm

public class RunTest
{
    [Fact]
    public void RunAndGenerate()
    {
        var (compilation, semanticModel) = CompilationHelper.Create(
            """
                #pragma warning disable CS8019 // Unnecessary using directive.

                using System;
                using MagicOnion;
                using MagicOnion.Client;
                
                namespace TempProject;

                public interface IMyService : IService<IMyService>
                {
                    UnaryResult<string> HelloAsync(string name, int age);
                }

                [MagicOnion.Client.MagicOnionClientGeneration(typeof(IMyService))]
                partial class MagicOnionInitializer {}
                """);
        var sourceGenerator = new MagicOnionClientSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
        );

        // Run generator and update compilation
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        Assert.DoesNotContain(diagnostics, x => x.Severity > DiagnosticSeverity.Info);
        Assert.Empty(newCompilation.GetDiagnostics());

        var results = driver.GetRunResult().Results;
        var generatedTrees = driver.GetRunResult().GeneratedTrees;
    }

    [Fact]
    public void RunAndGenerate_External()
    {
        var (compilation, semanticModel) = CompilationHelper.Create(
            """
                using System;
                using MagicOnion;
                using MagicOnion.Client;
                
                namespace TempProject;

                [MagicOnionClientGeneration(typeof(SampleServiceDefinitions.Services.IGreeterService))]
                partial class MagicOnionInitializer {}
                """);
        var sourceGenerator = new MagicOnionClientSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
        );

        // Run generator and update compilation
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        Assert.DoesNotContain(diagnostics, x => x.Severity > DiagnosticSeverity.Info);
        Assert.DoesNotContain(newCompilation.GetDiagnostics(), x => x.Severity > DiagnosticSeverity.Info);
    }

    [Fact]
    public void RunAndGenerate_Service()
    {
        var (compilation, semanticModel) = CompilationHelper.Create(
            """
                #nullable enable
                using System;
                using MagicOnion;
                using MagicOnion.Client;
                
                namespace TempProject;

                public interface IMyService : IService<IMyService>
                {
                    UnaryResult<string> HelloAsync(string name, int age);
                    UnaryResult<string?> HelloNullableAsync(string? name, int? age);
                }
                
                [MagicOnionClientGeneration(typeof(IMyService))]
                partial class MagicOnionInitializer {}
                """);
        var sourceGenerator = new MagicOnionClientSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
        );

        // Run generator and update compilation
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        Assert.DoesNotContain(diagnostics, x => x.Severity > DiagnosticSeverity.Info);
        Assert.DoesNotContain(newCompilation.GetDiagnostics(), x => x.Severity > DiagnosticSeverity.Info);
    }

    [Fact]
    public void RunAndGenerate_StreamingHub()
    {
        var (compilation, semanticModel) = CompilationHelper.Create(
            """
                #nullable enable
                using System;
                using System.Threading.Tasks;
                using MagicOnion;
                using MagicOnion.Client;
                
                namespace TempProject;

                public interface IMyStreamingHub : IStreamingHub<IMyStreamingHub, IMyStreamingHubReceiver>
                {
                    Task<string> HelloAsync(string name, int age);
                    Task<string?> HelloNullableAsync(string? name, int? age);
                }

                public interface IMyStreamingHubReceiver
                {
                }
                
                [MagicOnionClientGeneration(typeof(IMyStreamingHub))]
                partial class MagicOnionInitializer {}
                """);
        var sourceGenerator = new MagicOnionClientSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
        );

        // Run generator and update compilation
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        Assert.DoesNotContain(diagnostics, x => x.Severity > DiagnosticSeverity.Info);
        Assert.DoesNotContain(newCompilation.GetDiagnostics(), x => x.Severity > DiagnosticSeverity.Info);
    }

    //[Fact]
    //public async Task UnchangedTest_1()
    //{
    //    var (compilation, semanticModel) = CompilationHelper.Create(
    //        """
    //            using System;
    //            using MagicOnion;
    //
    //            namespace TempProject;
    //
    //            public interface IMyService : IService<IMyService>
    //            {
    //                UnaryResult<string> HelloAsync(string name, int age);
    //            }
    //            struct A {}
    //            """);
    //    var sourceGenerator = new MagicOnionClientSourceGenerator();
    //
    //    GeneratorDriver driver = CSharpGeneratorDriver.Create(
    //        generators: new[] { sourceGenerator.AsSourceGenerator() },
    //        driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
    //    );
    //
    //    // Run generator for the first time.
    //    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var compilation2, out _);
    //    var resultFirstTime = driver.GetRunResult().Results;
    //
    //    // Modify syntax tree (editing source code)
    //    compilation2 = compilation2.AddSyntaxTrees(CSharpSyntaxTree.ParseText("//dummy"));
    //
    //    // Run generator again.
    //    driver = driver.RunGeneratorsAndUpdateCompilation(compilation2, out var compilation3, out _);
    //
    //    // Generator's output should be unchanged.
    //    var result = driver.GetRunResult().Results.Single();
    //    var allOutputReason = result.TrackedOutputSteps.SelectMany(x => x.Value).SelectMany(x => x.Outputs);
    //    Assert.Collection(allOutputReason, x => Assert.Equal(IncrementalStepRunReason.Unchanged, x.Reason));
    //}
    
    [Fact]
    public void RunAndGenerate_ParseAttributeName()
    {
        var (compilation, semanticModel) = CompilationHelper.Create(
            """
            using MagicOnion.Client;
            
            using moc = global::MagicOnion.Client;
            
            [MagicOnionClientGeneration]
            partial class IdentifierOnly {}
            
            [MagicOnion.Client.MagicOnionClientGeneration]
            partial class FullyQualified {}
            
            [global::MagicOnion.Client.MagicOnionClientGeneration]
            partial class FullyQualifiedWithGlobal {}
            
            [moc::MagicOnionClientGeneration]
            partial class AliasQualified {}
            
            
            [MagicOnionClientGenerationAttribute]
            partial class FullName_IdentifierOnly {}
            
            [MagicOnion.Client.MagicOnionClientGenerationAttribute]
            partial class FullName_FullyQualified {}
            
            [global::MagicOnion.Client.MagicOnionClientGenerationAttribute]
            partial class FullName_FullyQualifiedWithGlobal {}
            
            [moc::MagicOnionClientGenerationAttribute]
            partial class FullName_AliasQualified {}
            """);
        var sourceGenerator = new MagicOnionClientSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
        );

        // Run generator and update compilation
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        Assert.DoesNotContain(diagnostics, x => x.Severity > DiagnosticSeverity.Info);
        Assert.DoesNotContain(newCompilation.GetDiagnostics(), x => x.Severity > DiagnosticSeverity.Info);
    }
}
