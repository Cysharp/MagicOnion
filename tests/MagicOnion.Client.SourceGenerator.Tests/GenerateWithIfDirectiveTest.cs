using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateWithIfDirectiveTest
{
    [Fact]
    public async Task Skip_Generation_StreamingHub_Interface()
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
                void OnMessage(int a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(int a);
            }
        
        #if DEBUG
            public interface IMyDebugHub : IStreamingHub<IMyDebugHub, IMyHubReceiver>
            {
                Task A(int a);
            }
        #endif

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Skip_Generation_Service_Interface()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<int> A();
            }
        
        #if DEBUG
            public interface IMyServiceForDebug : IService<IMyServiceForDebug>
            {
                UnaryResult<int> A();
            }
        #endif

            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
    
}
