using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateNullableTest
{
    [Fact]
    public async Task NullableReferenceType()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string?> HelloAsync(string? name, int age);
        }
        
        [MagicOnionClientGeneration(typeof(IGreeterService))]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task NullableValueType()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<bool?> HelloAsync(string name, int? age);
        }
        
        [MagicOnionClientGeneration(typeof(IGreeterService))]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}
