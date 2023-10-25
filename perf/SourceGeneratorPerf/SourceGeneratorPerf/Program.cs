using System.Text;
using JetBrains.Profiler.Api;
using MagicOnion.Client.SourceGenerator;
using MagicOnion.Client.SourceGenerator.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

MemoryProfiler.CollectAllocations(true);


var sb = new StringBuilder();
sb.AppendLine("""
    #nullable enable
    using System;
    using System.Threading.Tasks;
    using MagicOnion;
    using MagicOnion.Client;
    
    namespace TempProject;
    """);

for (var i = 0; i < 100; i++)
{
    sb.AppendLine($$"""
    public interface IMyStreamingHub{{i}} : IStreamingHub<IMyStreamingHub{{i}}, IMyStreamingHubReceiver{{i}}>
    {
    """);
    for (var j = 0; j < 10; j++)
    {
        sb.AppendLine($$"""
        Task<string> HelloAsync{{j}}(string name, int age);
    """);
    }
    
    sb.AppendLine($$"""
    }
    public interface IMyStreamingHubReceiver{{i}}
    {
    """);
    for (var j = 0; j < 10; j++)
    {
        sb.AppendLine($$"""
        void Callback{{j}}(string name, int age);
    """);
    }

    sb.AppendLine($$"""
    }
    """);

    sb.AppendLine($$"""
    public interface IMyService{{i}} : IService<IMyService{{i}}>
    {
    """);
    for (var j = 0; j < 10; j++)
    {
        sb.AppendLine($$"""
        UnaryResult<string> HelloAsync{{j}}(string name, int age);
    """);
    }
    sb.AppendLine($$"""
    }
    """);
}
sb.AppendLine("""
    [MagicOnionClientGeneration(typeof(MagicOnionInitializer))]
    partial class MagicOnionInitializer {}
    """);

var (compilation, semanticModel) = CompilationHelper.Create(sb.ToString());
var sourceGenerator = new MagicOnionClientSourceGenerator();

GeneratorDriver driver = CSharpGeneratorDriver.Create(
    generators: new[] { sourceGenerator.AsSourceGenerator() },
    driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
);

// Run generator and update compilation
MemoryProfiler.GetSnapshot("Before RunGenerators#1");
driver.RunGenerators(compilation, CancellationToken.None);
MemoryProfiler.GetSnapshot("After RunGenerators#1/Before RunGenerators#2");
driver = driver.RunGenerators(compilation, CancellationToken.None);
MemoryProfiler.GetSnapshot("After RunGenerators#2/Before RunGenerators#3");
driver = driver.RunGenerators(compilation, CancellationToken.None);
MemoryProfiler.GetSnapshot("After RunGenerators#3/Before RunGeneratorsAndUpdateCompilation");
driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);
MemoryProfiler.GetSnapshot("After RunGeneratorsAndUpdateCompilation");
