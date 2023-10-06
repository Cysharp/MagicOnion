using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MagicOnion.Client.SourceGenerator.Tests;

// ref: https://www.meziantou.net/testing-roslyn-incremental-source-generators.htm

public class RunTest
{
    [Fact]
    public async Task RunTest1()
    {
        var (compilation, semanticModel) = CompilationHelper.Create("""
            using System;
            using MagicOnion;

            namespace TempProject;

            public interface IMyService : IService<IMyService>
            {
                UnaryResult<string> HelloAsync(string name, int age);
            }
            """);
        var sourceGenerator = new MagicOnionClientSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
        );

        // Run generator for the first time.
        driver = driver.RunGenerators(compilation);
        var resultFirstTime = driver.GetRunResult().Results;

        // Modify syntax trees
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("//comment"));

        // Run generator again.
        driver = driver.RunGenerators(compilation);

        // Generator's output should be unchanged.
        var result = driver.GetRunResult().Results.Single();
        var allOutputReason = result.TrackedOutputSteps.SelectMany(x => x.Value).SelectMany(x => x.Outputs);
        Assert.Collection(allOutputReason, x => Assert.Equal(IncrementalStepRunReason.Unchanged, x.Reason));

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
    }
}
