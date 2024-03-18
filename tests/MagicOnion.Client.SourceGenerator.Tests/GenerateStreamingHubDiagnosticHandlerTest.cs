using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateStreamingHubDiagnosticHandlerTest
{
    [Fact]
    public async Task Generate()
    {
        var source = """
                     using System;
                     using System.Threading.Tasks;
                     using MessagePack;
                     using MagicOnion;
                     using MagicOnion.Client;

                     namespace TempProject
                     {
                         public interface IMyHubReceiver
                         {
                             void A();
                             void B(int arg0, string arg1, bool arg2);
                         }
                         
                         public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
                         {
                            Task MethodA();
                            Task<int> MethodB();
                            ValueTask MethodC();
                            ValueTask<int> MethodD();
                            Task MethodE(int arg0, string arg1);
                            Task<string> MethodF(int arg0, string arg1, bool arg2);
                         }
                     
                         [MagicOnionClientGeneration(typeof(IMyHub), EnableStreamingHubDiagnosticHandler = true)]
                         partial class MagicOnionInitializer {}
                     }
                     """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}
