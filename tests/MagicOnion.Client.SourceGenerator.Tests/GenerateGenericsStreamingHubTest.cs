using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateGenericsStreamingHubTest
{
    [Fact]
    public async Task Parameters()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyGenericObject<int> a);
                Task B(MyGenericObject<MyObject> a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameters_MultipleTypeArgs()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyGenericObject<int, MyObject> a);
                Task B(MyGenericObject<MyObject, int> a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T1, T2>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T1, T2> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T1, T2>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T1, T2> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T1, T2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyGenericObject<MyGenericObject<MyObject>> a);
                Task B(MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>> a);
                Task C(MyGenericObject<MyGenericObject<MyGenericObject<int>>> a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<Nil> GetEnumAsync(MyGenericObject<MyGenericObject<MyEnum>> arg0);
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<Nil> GetValuesAsync(MyGenericObject<MyNestedGenericObject[]> arg0);
            }
        
            public class MyGenericObject<T>
            {
            }
        
            public class MyNestedGenericObject
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<Nil> GetStringValuesAsync(List<string> arg0);
                Task<Nil> GetIntValuesAsync(List<int> arg0);
            }
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<Nil> GetValuesAsync(List<MyResponse> arg0);
            }
            public class MyResponse
            {
            }
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<Nil> GetStringValuesAsync(string[] arg0);
                Task<Nil> GetIntValuesAsync(int[] arg0);
                Task<Nil> GetInt32ValuesAsync(Int32[] arg0);
                Task<Nil> GetSingleValuesAsync(float[] arg0);
                Task<Nil> GetBooleanValuesAsync(bool[] arg0);
            }
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<Nil> GetValuesAsync(MyResponse[] arg0);
            }
        
            public class MyResponse
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyGenericObject<int>> A();
                Task<MyGenericObject<MyObject>> B();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyGenericObject<MyGenericObject<MyObject>>> A();
                Task<MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>>> B();
                Task<MyGenericObject<MyGenericObject<MyGenericObject<int>>>> C();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyGenericObject<int, MyObject>> A();
                Task<MyGenericObject<MyObject, int>> B();
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T1, T2>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T1, T2> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T1, T2>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T1, T2> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T1, T2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyGenericObject<MyEnum>> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyGenericObject<MyGenericObject<MyEnum>>> GetEnumAsync();
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyGenericObject<MyNestedGenericObject[]>> GetValuesAsync();
            }
        
            public class MyGenericObject<T>
            {
            }
        
            public class MyNestedGenericObject
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<List<string>> GetStringValuesAsync();
                Task<List<int>> GetIntValuesAsync();
            }
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<List<MyResponse>> GetValuesAsync();
            }
            public class MyResponse
            {
            }
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<string[]> GetStringValuesAsync();
                Task<int[]> GetIntValuesAsync();
                Task<Int32[]> GetInt32ValuesAsync();
                Task<float[]> GetSingleValuesAsync();
                Task<bool[]> GetBooleanValuesAsync();
            }
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
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyResponse[]> GetValuesAsync();
            }
        
            public class MyResponse
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyGenericObject<int> a);
                void B(MyGenericObject<MyObject> b);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Nested()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyGenericObject<MyGenericObject<MyObject>> a);
                void B(MyGenericObject<MyGenericObject<MyGenericObject<MyObject>>> b);
                void C(MyGenericObject<MyGenericObject<MyGenericObject<int>>> c);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_MultipleTypeArgs()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyGenericObject<int, MyObject> a);
                void B(MyGenericObject<MyObject, int> b);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        
            [MessagePackObject]
            public class MyGenericObject<T1, T2>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T1, T2> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T1, T2>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T1, T2> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T1, T2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Enum()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyGenericObject<MyEnum> a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Nested_Enum()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyGenericObject<MyGenericObject<MyEnum>> a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            public enum MyEnum
            {
                A, B, C
            }
        
            [MessagePackObject]
            public class MyGenericObject<T>
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Nested_Array()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyGenericObject<MyNestedGenericObject[]> a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            public class MyGenericObject<T>
            {
            }
        
            public class MyNestedGenericObject
            {
            }
        }
        
        // Pseudo generated MessagePackFormatter using mpc (MessagePack.Generator)
        namespace MessagePack.Formatters.TempProject
        {
            public class MyGenericObjectFormatter<T> : MessagePack.Formatters.IMessagePackFormatter<global::TempProject.MyGenericObject<T>>
            {
                public void Serialize(ref MessagePackWriter writer, global::TempProject.MyGenericObject<T> value, MessagePackSerializerOptions options) => throw new NotImplementedException();
                public global::TempProject.MyGenericObject<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_ListFormatter_KnownType()
    {
        var source = """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(List<string> a);
                void B(List<int> b);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_ListFormatter_UserType()
    {
        var source = """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(List<MyResponse> a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
            public class MyResponse
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_ArrayFormatter_KnownType()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(string[] a);
                void B(int[] a);
                void C(Int32[] a);
                void D(float[] a);
                void E(bool[] a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_ArrayFormatter_UserType()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void A(MyResponse[] a);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        
            public class MyResponse
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}
