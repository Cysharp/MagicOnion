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
        [Option("u", "Do not use UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer. (Same as --disable-auto-register)")]bool noUseUnityAttr = false,
        [Option("d", "Do not automatically call MagicOnionInitializer during start-up. (Automatic registration requires .NET 5+ or Unity)")]bool disableAutoRegister = false,
        [Option("n", "The namespace of clients to generate.")]string @namespace = "MagicOnion",
        [Option("m", "The namespace of pre-generated MessagePackFormatters.")]string messagepackFormatterNamespace = "MessagePack.Formatters",
        [Option("c", "The conditional compiler symbols used during code analysis. The value is split by ','.")]string conditionalSymbol = null,
        [Option("v", "Enable verbose logging")]bool verbose = false,
        [Option("s", "The serializer used for message serialization")] SerializerType serializer = SerializerType.MessagePack
    )
    {
        await new MagicOnionCompiler(new MagicOnionGeneratorConsoleLogger(verbose), ctx.CancellationToken)
            .GenerateFileAsync(
                input,
                output,
                disableAutoRegister,
                @namespace,
                conditionalSymbol,
                messagepackFormatterNamespace,
                serializer);
    }
}
