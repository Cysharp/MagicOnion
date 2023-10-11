using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GeneratorOptionsTest
{
    [Fact]
    public async Task Default()
    {
        var source = """
        using MagicOnion;

        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task DisableAutoRegister()
    {
        var source = """
        using MagicOnion;

        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source, options: GeneratorOptions.Default with { DisableAutoRegister = true });
    }

    [Fact]
    public async Task Namespace()
    {
        var source = """
        using MagicOnion;

        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source, options: GeneratorOptions.Default with { Namespace = "__Generated__" });
    }
    
    [Fact]
    public async Task MessagePackFormatterNamespace()
    {
        var source = """
        using System;
        using MagicOnion;

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
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source, options: GeneratorOptions.Default with { MessagePackFormatterNamespace = "__UserDefined__.MessagePack.Formatters" });
    }

    [Fact]
    public async Task Serializer_MemoryPack()
    {
        var source = """
        using MagicOnion;

        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(
            source,
            options: GeneratorOptions.Default with { Serializer = GeneratorOptions.SerializerType.MemoryPack },
            verifierOptions: VerifierOptions.Default with { UseMemoryPack = true }
        );
    }
}
