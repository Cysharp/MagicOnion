using System.Threading.Tasks;
using ConsoleAppFramework;
using MagicOnion.Generator.Internal;

namespace MagicOnion.Generator;

public class Program
{
    static Task Main(string[] args)
        => ConsoleApp.Create(args)
            .AddRootCommand(RunAsync)
            .RunAsync();

    [Command("", description: "MagicOnion Code Generator generates client codes for Ahead-of-Time compilation.")]
    static async Task RunAsync(
        ConsoleAppContext ctx,
        [Option("i", "The path to the project (.csproj) to generate the client.")]string input,
        [Option("o", "The generated file path (single file) or the directory path to generate the files (multiple files).")]string output,
        [Option("u", "Do not use UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer.")]bool noUseUnityAttr = false,
        [Option("n", "The namespace of generated client.")]string @namespace = "MagicOnion",
        [Option("m", "The namespace of generated MessagePackFormatters.")]string messagepackNamespace = "MessagePack.Formatters",
        [Option("c", "The conditional compiler symbols used during analysis. The value is split by ','.")]string conditionalSymbol = null,
        [Option("v", "Enable verbose logging")]bool verbose = false
    )
    {
        await new MagicOnionCompiler(new MagicOnionGeneratorConsoleLogger(verbose), ctx.CancellationToken)
            .GenerateFileAsync(
                input,
                output,
                noUseUnityAttr,
                @namespace,
                conditionalSymbol,
                messagepackNamespace);
    }
}