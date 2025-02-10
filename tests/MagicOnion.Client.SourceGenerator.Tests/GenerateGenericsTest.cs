using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateGenericsTest
{
    [Fact]
    public async Task Parameters()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        using System.Collections.Generic;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<Nil> A(MyGenericObject<int> a);
                UnaryResult<Nil> B(MyGenericObject<MyObject> a);
                UnaryResult<MyGenericObject<IReadOnlyList<MyObject>>> C();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_Nested()
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
                UnaryResult<Nil> A(MyGenericObject<MyGenericObject<MyObject>> a);
                UnaryResult<Nil> B(MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>> a);
                UnaryResult<Nil> C(MyGenericObject<MyGenericObject<MyGenericObject<int>>> a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
        [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_Nested_Enum()
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
                UnaryResult<Nil> GetEnumAsync(MyGenericObject<MyGenericObject<MyEnum>> arg0);
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_Nested_Array()
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
                UnaryResult<Nil> GetValuesAsync(MyGenericObject<MyNestedGenericObject[]> arg0);
            }
        
            public class MyGenericObject<T>
            {
            }
        
            public class MyNestedGenericObject
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_Nested_DoNotGenerateResolver()
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
                UnaryResult<Nil> A(MyGenericObject<MyGenericObject<MyObject>> a);
                UnaryResult<Nil> B(MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>> a);
                UnaryResult<Nil> C(MyGenericObject<MyGenericObject<MyGenericObject<int>>> a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_Nested_DoNotGenerateResolver_Enum()
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
                UnaryResult<Nil> GetEnumAsync(MyGenericObject<MyGenericObject<MyEnum>> arg0);
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_Nested_DoNotGenerateResolver_Array()
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
                UnaryResult<Nil> GetValuesAsync(MyGenericObject<MyNestedGenericObject[]> arg0);
            }
        
            public class MyGenericObject<T>
            {
            }
        
            public class MyNestedGenericObject
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_ListFormatter_KnownType()
    {
        var source = """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<Nil> GetStringValuesAsync(List<string> arg0);
                UnaryResult<Nil> GetIntValuesAsync(List<int> arg0);
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_ListFormatter_UserType()
    {
        var source = """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<Nil> GetValuesAsync(List<MyResponse> arg0);
            }
            public class MyResponse
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_ArrayFormatter_KnownType()
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
                UnaryResult<Nil> GetStringValuesAsync(string[] arg0);
                UnaryResult<Nil> GetIntValuesAsync(int[] arg0);
                UnaryResult<Nil> GetInt32ValuesAsync(Int32[] arg0);
                UnaryResult<Nil> GetSingleValuesAsync(float[] arg0);
                UnaryResult<Nil> GetBooleanValuesAsync(bool[] arg0);
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_ArrayFormatter_UserType()
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
                UnaryResult<Nil> GetValuesAsync(MyResponse[] arg0);
            }
        
            public class MyResponse
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return()
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
                UnaryResult<MyGenericObject<int>> A();
                UnaryResult<MyGenericObject<MyObject>> B();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Nested()
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
                UnaryResult<MyGenericObject<MyGenericObject<MyObject>>> A();
                UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>>> B();
                UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<int>>>> C();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_MultipleTypeArgs()
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
                UnaryResult<MyGenericObject<int, MyObject>> A();
                UnaryResult<MyGenericObject<MyObject, int>> B();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T1, T2>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T1, T2> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T1, T2>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T1, T2> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T1, T2> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Enum()
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
                UnaryResult<MyGenericObject<MyEnum>> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Nested_Enum()
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
                UnaryResult<MyGenericObject<MyGenericObject<MyEnum>>> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Nested_Array()
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
                UnaryResult<MyGenericObject<MyNestedGenericObject[]>> GetValuesAsync();
            }

            public class MyGenericObject<T>
            {
            }

            public class MyNestedGenericObject
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            [MagicOnionClientGenerationOption("MessagePack.GenerateResolverForCustomFormatter", true)]
            partial class MagicOnionInitializer {}
        }

        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Nested_DoNotGenerateResolver()
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
                UnaryResult<MyGenericObject<MyGenericObject<MyObject>>> A();
                UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>>> B();
                UnaryResult<MyGenericObject<MyGenericObject<MyGenericObject<int>>>> C();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_MultipleTypeArgs_DoNotGenerateResolver()
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
                UnaryResult<MyGenericObject<int, MyObject>> A();
                UnaryResult<MyGenericObject<MyObject, int>> B();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T1, T2>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T1, T2> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T1, T2>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T1, T2> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T1, T2> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Enum_DoNotGenerateResolver()
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
                UnaryResult<MyGenericObject<MyEnum>> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Nested_Enum_DoNotGenerateResolver()
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
                UnaryResult<MyGenericObject<MyGenericObject<MyEnum>>> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Nested_Array_DoNotGenerateResolver()
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
                UnaryResult<MyGenericObject<MyNestedGenericObject[]>> GetValuesAsync();
            }

            public class MyGenericObject<T>
            {
            }

            public class MyNestedGenericObject
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }

        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : global::MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_ListFormatter_KnownType()
    {
        var source = """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<List<string>> GetStringValuesAsync();
                UnaryResult<List<int>> GetIntValuesAsync();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_ListFormatter_UserType()
    {
        var source = """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<List<MyResponse>> GetValuesAsync();
            }
            public class MyResponse
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_ArrayFormatter_KnownType()
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
                UnaryResult<string[]> GetStringValuesAsync();
                UnaryResult<int[]> GetIntValuesAsync();
                UnaryResult<Int32[]> GetInt32ValuesAsync();
                UnaryResult<float[]> GetSingleValuesAsync();
                UnaryResult<bool[]> GetBooleanValuesAsync();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_ArrayFormatter_UserType()
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
                UnaryResult<MyResponse[]> GetValuesAsync();
            }
        
            public class MyResponse
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task KnownFormatters()
    {
        var source = """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                // https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/v3.0.300/src/MessagePack/Resolvers/DynamicGenericResolver.cs#L52
                UnaryResult<List<MyResponse>>                        MethodList(List<int> args);
                UnaryResult<LinkedList<MyResponse>>                  MethodLinkedList(LinkedList<int> args);
                UnaryResult<Queue<MyResponse>>                       MethodQueue();
                //UnaryResult<PriorityQueue<int,MyResponse>>           MethodPriorityQueue();
                UnaryResult<Stack<MyResponse>>                       MethodStack();
                UnaryResult<HashSet<MyResponse>>                     MethodHashSet();
                UnaryResult<ReadOnlyCollection<MyResponse>>          MethodROCollection();
        
                UnaryResult<IList<MyResponse>>                       MethodIList();
                UnaryResult<ICollection<MyResponse>>                 MethodICollection();
                UnaryResult<IEnumerable<MyResponse>>                 MethodIEnumerable();
        
                UnaryResult<Dictionary<string, MyResponse>>          MethodDictionary();
                UnaryResult<IDictionary<string, MyResponse>>         MethodIDictionary();
                UnaryResult<SortedDictionary<string, MyResponse>>    MethodSortedDictionary();
                UnaryResult<SortedList<int, MyResponse>>             MethodSortedList();
        
                UnaryResult<ILookup<int, MyResponse>>                MethodILookup();
                UnaryResult<IGrouping<int, MyResponse>>              MethodIGrouping();
        
                UnaryResult<ISet<MyResponse>>                        MethodISet();
                UnaryResult<IReadOnlySet<MyResponse>>                MethodIROSet();
        
                UnaryResult<IReadOnlyCollection<MyResponse>>         MethodIROCollection();
                UnaryResult<IReadOnlyList<MyResponse>>               MethodIROList();
                UnaryResult<IReadOnlyDictionary<string, MyResponse>> MethodIRODictionary();
                UnaryResult<ReadOnlyDictionary<string, MyResponse>>  MethodRODictionary();
        
            }
            public class MyResponse
            {
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}
