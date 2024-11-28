using System.Buffers;
using System.Text;
using System.Text.Json;
using MessagePack;

namespace MagicOnion.Server.JsonTranscoding.Tests;

public class SystemTextJsonMessageSerializerTest
{
    [Fact]
    public void Serialize_Primitive_BuiltIn_String()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, "FooBar");

        // Assert
        Assert.Equal("\"FooBar\""u8.ToArray(), writer.WrittenMemory.ToArray());
    }

    [Fact]
    public void Serialize_Primitive_BuiltIn_Number()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, 123456789);

        // Assert
        Assert.Equal("123456789"u8.ToArray(), writer.WrittenMemory.ToArray());
    }

    [Fact]
    public void Serialize_Primitive_BuiltIn_Null()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, default(string));

        // Assert
        Assert.Equal("null"u8.ToArray(), writer.WrittenMemory.ToArray());
    }

    [Fact]
    public void Serialize_Primitive_Nil()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, Nil.Default);

        // Assert
        Assert.Equal("null"u8.ToArray(), writer.WrittenMemory.ToArray());
    }

    [Fact]
    public void Serialize_DynamicArgumentTuple_2()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["name", "age"]);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, new DynamicArgumentTuple<string, int>("Alice", 18));

        // Assert
        Assert.Equal("""["Alice",18]""", Encoding.UTF8.GetString(writer.WrittenMemory.Span));
    }


    [Fact]
    public void Serialize_DynamicArgumentTuple_15()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["arg1","arg2","arg3","arg4","arg5","arg6","arg7","arg8","arg9","arg10","arg11","arg12","arg13","arg14"]);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, new DynamicArgumentTuple<string, int, bool, long, char, int, string, bool, Guid, byte, string, int, long, float>(
            "Alice", 18, true, long.MaxValue, 'X', int.MaxValue, "Foo", false, Guid.Parse("4dfa3247-0686-4b07-9772-cfb7ef30c22c"), 255, "Bar", 12345, -12345, 3.14f));

        // Assert
        Assert.Equal("""["Alice",18,true,9223372036854775807,"X",2147483647,"Foo",false,"4dfa3247-0686-4b07-9772-cfb7ef30c22c",255,"Bar",12345,-12345,3.14]""", Encoding.UTF8.GetString(writer.WrittenMemory.Span));
    }

    [Fact]
    public void Serialize_KeyedDynamicArgumentTuple_2()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["name","age"], serializeAsKeyedObject: true);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, new DynamicArgumentTuple<string, int>("Alice", 18));

        // Assert
        Assert.Equal("""{"name":"Alice","age":18}""", Encoding.UTF8.GetString(writer.WrittenMemory.Span));
    }

    [Fact]
    public void Serialize_Complex()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, new TestResponse()
        {
            A = 1234,
            B = "Alice",
            C = true,
            Inner = new TestResponse.InnerResponse()
            {
                D = 98765432100,
                E = "Hello!",
            },
        });

        // Assert
        Assert.Equal("""{"A":1234,"B":"Alice","C":true,"Inner":{"D":98765432100,"E":"Hello!"}}""", Encoding.UTF8.GetString(writer.WrittenMemory.Span));
    }

    [Fact]
    public void Deserialize_Primitive_BuiltIn_String()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);

        // Act
        var value = messageSerializer.Deserialize<string>(new ReadOnlySequence<byte>("\"FooBar\""u8.ToArray()));

        // Assert
        Assert.Equal("FooBar", value);
    }

    [Fact]
    public void Deserialize_Primitive_BuiltIn_Number()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);

        // Act
        var value = messageSerializer.Deserialize<long>(new ReadOnlySequence<byte>("""123456789"""u8.ToArray()));

        // Assert
        Assert.Equal(123456789, value);
    }

    [Fact]
    public void Deserialize_Primitive_BuiltIn_Null()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);

        // Act
        var value = messageSerializer.Deserialize<object>(new ReadOnlySequence<byte>("""null"""u8.ToArray()));

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void Deserialize_Primitive_Nil()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);

        // Act
        var value = messageSerializer.Deserialize<Nil>(new ReadOnlySequence<byte>("""null"""u8.ToArray()));

        // Assert
        Assert.Equal(Nil.Default, value);
    }

    [Fact]
    public void Deserialize_DynamicArgumentTuple_2()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["name", "age"]);

        // Act
        var value = messageSerializer.Deserialize<DynamicArgumentTuple<string, int>>(new ReadOnlySequence<byte>("""["Alice",18]"""u8.ToArray()));

        // Assert
        Assert.Equal("Alice", value.Item1);
        Assert.Equal(18, value.Item2);
    }

    [Fact]
    public void Deserialize_DynamicArgumentTuple_15()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10", "arg11", "arg12", "arg13", "arg14"]);

        // Act
        var value = messageSerializer.Deserialize<DynamicArgumentTuple<string, int, bool, long, char, int, string, bool, Guid, byte, string, int, long, float>>(new ReadOnlySequence<byte>(
            """["Alice",18,true,9223372036854775807,"X",2147483647,"Foo",false,"4dfa3247-0686-4b07-9772-cfb7ef30c22c",255,"Bar",12345,-12345,3.14]"""u8.ToArray()));

        // Assert
        Assert.Equal("Alice", value.Item1);
        Assert.Equal(18, value.Item2);
        Assert.True(value.Item3);
        Assert.Equal(long.MaxValue, value.Item4);
        Assert.Equal('X', value.Item5);
        Assert.Equal(int.MaxValue, value.Item6);
        Assert.Equal("Foo", value.Item7);
        Assert.False(value.Item8);
        Assert.Equal(Guid.Parse("{4dfa3247-0686-4b07-9772-cfb7ef30c22c}"), value.Item9);
        Assert.Equal(255, value.Item10);
        Assert.Equal("Bar", value.Item11);
        Assert.Equal(12345, value.Item12);
        Assert.Equal(-12345, value.Item13);
        Assert.Equal(3.14, value.Item14, precision: 5);
    }

    [Fact]
    public void Deserialize_KeyedDynamicArgumentTuple_2()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["name", "age"]);

        // Act
        var value = messageSerializer.Deserialize<DynamicArgumentTuple<string, int>>(new ReadOnlySequence<byte>("""{"name": "Alice", "age": 18 }"""u8.ToArray()));

        // Assert
        Assert.Equal("Alice", value.Item1);
        Assert.Equal(18, value.Item2);
    }


    [Fact]
    public void Deserialize_KeyedDynamicArgumentTuple_15()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, ["arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10", "arg11", "arg12", "arg13", "arg14"]);

        // Act
        var value = messageSerializer.Deserialize<DynamicArgumentTuple<string, int, bool, long, char, int, string, bool, Guid, byte, string, int, long, float>>(new ReadOnlySequence<byte>(
            """
            {
                "arg1": "Alice",
                "arg2": 18,
                "arg3": true,
                "arg4": 9223372036854775807,
                "arg5": "X",
                "arg6": 2147483647,
                "arg7": "Foo",
                "arg8": false,
                "arg9": "4dfa3247-0686-4b07-9772-cfb7ef30c22c",
                "arg10": 255,
                "arg11": "Bar",
                "arg12": 12345,
                "arg13": -12345,
                "arg14": 3.14
            }
            """u8.ToArray()));

        // Assert
        Assert.Equal("Alice", value.Item1);
        Assert.Equal(18, value.Item2);
        Assert.True(value.Item3);
        Assert.Equal(long.MaxValue, value.Item4);
        Assert.Equal('X', value.Item5);
        Assert.Equal(int.MaxValue, value.Item6);
        Assert.Equal("Foo", value.Item7);
        Assert.False(value.Item8);
        Assert.Equal(Guid.Parse("{4dfa3247-0686-4b07-9772-cfb7ef30c22c}"), value.Item9);
        Assert.Equal(255, value.Item10);
        Assert.Equal("Bar", value.Item11);
        Assert.Equal(12345, value.Item12);
        Assert.Equal(-12345, value.Item13);
        Assert.Equal(3.14, value.Item14, precision: 5);
    }

    [Fact]
    public void Deserialize_Complex()
    {
        // Arrange
        var options = JsonSerializerOptions.Default;
        var messageSerializer = new SystemTextJsonMessageSerializer(options, []);
        var writer = new ArrayBufferWriter<byte>();

        // Act
        messageSerializer.Serialize(writer, new TestResponse()
        {
            A = 1234,
            B = "Alice",
            C = true,
            Inner = new TestResponse.InnerResponse()
            {
                D = 98765432100,
                E = "Hello!",
            },
        });

        // Assert
        Assert.Equal("""{"A":1234,"B":"Alice","C":true,"Inner":{"D":98765432100,"E":"Hello!"}}""", Encoding.UTF8.GetString(writer.WrittenMemory.Span));
    }


    [MessagePackObject]
    public class TestResponse
    {
        [Key(0)]
        public int A { get; set; }
        [Key(1)]
        public required string B { get; init; }
        [Key(2)]
        public bool C { get; set; }
        [Key(3)]
        public required InnerResponse Inner { get; init; }

        [MessagePackObject]
        public class InnerResponse
        {
            [Key(0)]
            public long D { get; set; }
            [Key(1)]
            public required string E { get; init; }
        }
    }
}
