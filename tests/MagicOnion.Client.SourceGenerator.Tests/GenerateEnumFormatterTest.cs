using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateEnumFormatterTest
{
    [Fact]
    public async Task GenerateEnumFormatter_Return()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<MyEnum> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task GenerateEnumFormatter_Return_Nullable()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<MyEnum?> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task GenerateEnumFormatter_Parameter()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<Nil> GetEnumAsync(MyEnum a);
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task GenerateEnumFormatter_Parameter_Nullable()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<Nil> GetEnumAsync(MyEnum? a);
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task GenerateEnumFormatter_Nested()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<Nil> GetEnumAsync(MyClass.MyEnum? a);
            }
        
            public class MyClass
            {
                public enum MyEnum
                {
                    A, B, C
                }
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}
