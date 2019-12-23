using MagicOnion.GeneratorCore;
using MicroBatchFramework;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Generator
{
    public class Program : BatchBase
    {
        static async Task Main(string[] args)
        {
            await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<Program>(args).ConfigureAwait(false);
        }

        public async Task RunAsync(
            [Option("i", "Input path of analyze csproj or directory.")]string input,
            [Option("o", "Output path(file) or directory base(in separated mode).")]string output,
            [Option("u", "Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer.")]bool unuseUnityAttr = false,
            [Option("n", "Set namespace root name.")]string @namespace = "MagicOnion",
            [Option("c", "Conditional compiler symbols, split with ','.")]string conditionalSymbol = null)
        {
            await new MagicOnionCompiler(x => Console.WriteLine(x), this.Context.CancellationToken)
                .GenerateFileAsync(
                    input,
                    output,
                    unuseUnityAttr,
                    @namespace,
                    conditionalSymbol);
        }
    }
}
