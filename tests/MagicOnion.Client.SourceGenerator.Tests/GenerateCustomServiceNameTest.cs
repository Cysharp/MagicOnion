using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateCustomServiceNameTest
{
    [Fact]
    public async Task Service_Default()
    {
        var source = """
                     using MagicOnion;
                     using MagicOnion.Client;

                     namespace MyApplication1;

                     public interface IMyService : IService<IMyService>
                     {
                         UnaryResult MethodA();
                     }

                     [MagicOnionClientGeneration(typeof(IMyService))]
                     partial class MagicOnionInitializer {}
                     """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Service_CustomServiceName()
    {
        var source = """
                     using MagicOnion;
                     using MagicOnion.Client;

                     namespace MyApplication1;

                     [ServiceName("ICustomMyService")]
                     public interface IMyService : IService<IMyService>
                     {
                         UnaryResult MethodA();
                     }

                     [MagicOnionClientGeneration(typeof(IMyService))]
                     partial class MagicOnionInitializer {}
                     """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task StreamingHub_Default()
    {
        var source = """
                     using MagicOnion;
                     using MagicOnion.Client;

                     namespace MyApplication1;

                     public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
                     {
                         void MethodA();
                     }
                     public interface IMyHubReceiver {}

                     [MagicOnionClientGeneration(typeof(IMyHub))]
                     partial class MagicOnionInitializer {}
                     """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task StreamingHub_CustomServiceName()
    {
        var source = """
                     using MagicOnion;
                     using MagicOnion.Client;

                     namespace MyApplication1;

                     [ServiceName("ICustomMyHub")]
                     public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
                     {
                         void MethodA();
                     }
                     public interface IMyHubReceiver {}

                     [MagicOnionClientGeneration(typeof(IMyHub))]
                     partial class MagicOnionInitializer {}
                     """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}
