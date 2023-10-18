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
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<List<MyResponse>>                        MethodList(List<int> args);
                UnaryResult<IList<MyResponse>>                       MethodIList();
                UnaryResult<IReadOnlyList<MyResponse>>               MethodIROList();
        
                UnaryResult<Dictionary<string, MyResponse>>          MethodDictionary();
                UnaryResult<IDictionary<string, MyResponse>>         MethodIDictionary();
                UnaryResult<IReadOnlyDictionary<string, MyResponse>> MethodIRODictionary();
        
                UnaryResult<IEnumerable<MyResponse>>                 MethodIEnumerable();
                UnaryResult<ICollection<MyResponse>>                 MethodICollection();
                UnaryResult<IReadOnlyCollection<MyResponse>>         MethodIROCollection();
        
                UnaryResult<ILookup<int, MyResponse>>                MethodILookup();
                UnaryResult<IGrouping<int, MyResponse>>              MethodIGrouping();
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
