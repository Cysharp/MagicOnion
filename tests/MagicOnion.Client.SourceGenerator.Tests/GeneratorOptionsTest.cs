using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GeneratorOptionsTest
{
    [Fact]
    public async Task Default()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }
        
        [MagicOnionClientGeneration(typeof(IGreeterService))]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task DisableAutoRegistration()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }
        
        [MagicOnionClientGeneration(typeof(IGreeterService), DisableAutoRegistration = true)]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task MessagePackFormatterNamespace()
    {
        var source = """
        using System;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1
        {
            public interface IGreeterService : IService<IGreeterService>
            {
                UnaryResult<MyGenericObject<string>> HelloAsync(string name, int age);
            }

            public class MyGenericObject<T> {}
        }

        namespace __UserDefined__.MessagePack.Formatters.MyApplication1
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::MyApplication1.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MyApplication1.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::MyApplication1.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        
        [MagicOnionClientGeneration(typeof(MyApplication1.IGreeterService), MessagePackFormatterNamespace = "__UserDefined__.MessagePack.Formatters")]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Serializer_MemoryPack()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }

        [MagicOnionClientGeneration(typeof(IGreeterService), Serializer = MagicOnionClientGenerationAttribute.GenerateSerializerType.MemoryPack)]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(
            source,
            verifierOptions: VerifierOptions.Default with { UseMemoryPack = true }
        );
    }
}
