using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateMemoryPackTest
{
    [Fact]
    public async Task Generic()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        using MemoryPack;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<MyGenericObject<(string, int)>> HelloAsync(string name, int age);
        }

        [MemoryPackable]
        public class MyGenericObject<T> {}
        
        [MagicOnionClientGeneration(typeof(IGreeterService), Serializer = MagicOnionClientGenerationAttribute.GenerateSerializerType.MemoryPack)]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(
            source,
            verifierOptions: VerifierOptions.Default with { UseMemoryPack = true }
        );
    }
}
