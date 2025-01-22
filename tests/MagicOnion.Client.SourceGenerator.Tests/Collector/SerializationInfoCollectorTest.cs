using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public class SerializationInfoCollectorTest
{
    readonly ITestOutputHelper testOutputHelper;

    public SerializationInfoCollectorTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void NonGenerics()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateFromType<int>(),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject"),
            MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject"),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Enums);
        Assert.Empty(serializationInfoCollection.Generics);
        Assert.Equal(3, serializationInfoCollection.TypeHints.Count());
    }

    [Fact]
    public void Nullable()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")),
            MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject")),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Enums);
        Assert.Equal(2, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.YetAnotherGenericObject>()", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal(4, serializationInfoCollection.TypeHints.Count()); // Non-nullable + Nullable
    }

    [Fact]
    public void Generics_ManyTypeArguments()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<int>()),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(2, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String, global::System.Int64>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String, global::System.Int32>()", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal(5, serializationInfoCollection.TypeHints.Count()); // string, int, long, MyGenericObject<string, long>, MyGenericObject<string, int>
    }

    [Fact]
    public void Enum()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateFromType<int>()),
            MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnumConditional", MagicOnionTypeInfo.CreateFromType<int>()),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateFromType<int>())),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnumConditional", MagicOnionTypeInfo.CreateFromType<int>())),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateEnum("MyNamespace", "YetAnotherEnum", MagicOnionTypeInfo.CreateFromType<int>())),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(3, serializationInfoCollection.Generics.Count());
        Assert.Equal(3, serializationInfoCollection.Enums.Count());
        Assert.Equal("MyNamespace", serializationInfoCollection.Enums[0].Namespace);
        Assert.Equal("MyEnum", serializationInfoCollection.Enums[0].Name);
        Assert.Equal("MyEnumFormatter()", serializationInfoCollection.Enums[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("MyNamespace", serializationInfoCollection.Enums[1].Namespace);
        Assert.Equal("MyEnumConditional", serializationInfoCollection.Enums[1].Name);
        Assert.Equal("MyEnumConditionalFormatter()", serializationInfoCollection.Enums[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("MyNamespace", serializationInfoCollection.Enums[2].Namespace);
        Assert.Equal("YetAnotherEnum", serializationInfoCollection.Enums[2].Name);
        Assert.Equal("YetAnotherEnumFormatter()", serializationInfoCollection.Enums[2].GetFormatterNameWithConstructorArgs());
        Assert.Equal(6, serializationInfoCollection.TypeHints.Count()); // MyEnum, MyEnumConditional, YetAnotherEnum, MyGenericObject<MyEnum>, MyGenericObject<MyEnumConditional>, MyGenericObject<YetAnotherEnum>
    }
        
    [Fact]
    public void KnownTypes_SkipBuiltInGenericTypes()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateFromType<ArraySegment<byte>>(),
            MagicOnionTypeInfo.CreateFromType<ArraySegment<byte>?>(),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Generics);
        Assert.Equal(3, serializationInfoCollection.TypeHints.Count()); // byte, ArraySegment<byte>, Nullable<ArraySegment<byte>>
    }

    [Fact]
    public void KnownTypes_Array_SkipBuiltInTypes()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateFromType<byte[]>(),
            MagicOnionTypeInfo.CreateFromType<short[]>(),
            MagicOnionTypeInfo.CreateFromType<int[]>(),
            MagicOnionTypeInfo.CreateFromType<long[]>(),
            MagicOnionTypeInfo.CreateFromType<sbyte[]>(),
            MagicOnionTypeInfo.CreateFromType<ushort[]>(),
            MagicOnionTypeInfo.CreateFromType<uint[]>(),
            MagicOnionTypeInfo.CreateFromType<ulong[]>(),
            MagicOnionTypeInfo.CreateFromType<string[]>(),
            MagicOnionTypeInfo.CreateFromType<decimal[]>(),
            MagicOnionTypeInfo.CreateFromType<float[]>(),
            MagicOnionTypeInfo.CreateFromType<double[]>(),
            MagicOnionTypeInfo.CreateFromType<Guid[]>(),
            MagicOnionTypeInfo.CreateFromType<char[]>(),
            MagicOnionTypeInfo.CreateFromType<DateTime[]>(),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MessagePack", "Nil")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Vector2")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Vector3")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Vector4")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Quaternion")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Color")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Bounds")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Rect")),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Generics);
        Assert.Equal(46, serializationInfoCollection.TypeHints.Count());
    }  
    
    [Fact]
    public void KnownTypes_Array_NonBuiltIn()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject"), arrayRank: 2),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject"), arrayRank: 3),
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject"), arrayRank: 4),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(4, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.ArrayFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TwoDimensionalArrayFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ThreeDimensionalArrayFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.FourDimensionalArrayFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs());
        Assert.Equal(5, serializationInfoCollection.TypeHints.Count()); // MyObject, MyObject[], MyObject[,], MyObject[,,], MyObject[,,,]
    }

    [Fact]
    public void KnownTypes_Generics()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("System.Collections.Generic", "List",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "IList",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "IReadOnlyList",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "Dictionary",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "IDictionary",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "IReadOnlyDictionary",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "ICollection",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "IReadOnlyCollection",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "IEnumerable",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Collections.Generic", "KeyValuePair",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Linq", "ILookup",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
            MagicOnionTypeInfo.Create("System.Linq", "IGrouping",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(12, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.ListFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceListFormatter2<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceReadOnlyListFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.DictionaryFormatter<global::System.String, global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceDictionaryFormatter<global::System.String, global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[4].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceReadOnlyDictionaryFormatter<global::System.String, global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[5].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceCollectionFormatter2<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[6].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceReadOnlyCollectionFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[7].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceEnumerableFormatter<global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[8].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.KeyValuePairFormatter<global::System.String, global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[9].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceLookupFormatter<global::System.String, global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[10].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.InterfaceGroupingFormatter<global::System.String, global::MyNamespace.MyObject>()", serializationInfoCollection.Generics[11].GetFormatterNameWithConstructorArgs());
        Assert.Equal(14, serializationInfoCollection.TypeHints.Count());
    }
    
    [Fact]
    public void KnownTypes_Nullable()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateFromType<short?>(),
            MagicOnionTypeInfo.CreateFromType<int?>(),
            MagicOnionTypeInfo.CreateFromType<long?>(),
            MagicOnionTypeInfo.CreateFromType<byte?>(),
            MagicOnionTypeInfo.CreateFromType<ushort?>(),
            MagicOnionTypeInfo.CreateFromType<uint?>(),
            MagicOnionTypeInfo.CreateFromType<ulong?>(),
            MagicOnionTypeInfo.CreateFromType<sbyte?>(),
            MagicOnionTypeInfo.CreateFromType<bool?>(),
            MagicOnionTypeInfo.CreateFromType<char?>(),
            MagicOnionTypeInfo.CreateFromType<Guid?>(),
            MagicOnionTypeInfo.CreateFromType<DateTime?>(),
            MagicOnionTypeInfo.CreateFromType<TimeSpan?>(),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Enums);
        Assert.Empty(serializationInfoCollection.Generics);
        Assert.Equal(26, serializationInfoCollection.TypeHints.Count());
    }

    [Fact]
    public void KnownTypes_ValueTuple()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool, byte>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool, byte, short>>(),
            MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool, byte, short, Guid>>(),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(8, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String>()", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64>()", serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single>()", serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean>()", serializationInfoCollection.Generics[4].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte>()", serializationInfoCollection.Generics[5].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16>()", serializationInfoCollection.Generics[6].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16, global::System.Guid>()", serializationInfoCollection.Generics[7].GetFormatterNameWithConstructorArgs());
        Assert.Equal(8 + 8, serializationInfoCollection.TypeHints.Count());
    }

    [Fact]
    public void KnownTypes_Tuple()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.CreateFromType<Tuple<int>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool, byte>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool, byte, short>>(),
            MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool, byte, short, Guid>>(),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(8, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String>()", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64>()", serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single>()", serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean>()", serializationInfoCollection.Generics[4].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte>()", serializationInfoCollection.Generics[5].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16>()", serializationInfoCollection.Generics[6].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16, global::System.Guid>()", serializationInfoCollection.Generics[7].GetFormatterNameWithConstructorArgs());
    }

    [Fact]
    public void DynamicArgumentTuple()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()),
            MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<int>()),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Equal(2, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MagicOnion.Serialization.MessagePack.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MagicOnion.Serialization.MessagePack.DynamicArgumentTupleFormatter<global::System.String, global::System.Int32>(default(global::System.String), default(global::System.Int32))", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal(5, serializationInfoCollection.TypeHints.Count()); // string, long, int, DynamicArgumentTuple<string, long>, DynamicArgumentTuple<string, int>
    }

    [Fact]
    public void UserDefinedMessagePackSerializerFormattersNamespace()
    {
        // Arrange
        var userDefinedMessagePackFormattersNamespace = "MyFormatters";
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(userDefinedMessagePackFormattersNamespace, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")),
            MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Enums);
        Assert.Equal(3, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MagicOnion.Serialization.MessagePack.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MyFormatters.MyNamespace.MyGenericObjectFormatter<global::System.String>()", serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs());
    }

    [Fact]
    public void UserDefinedMessagePackSerializerFormattersNamespace_NotSpecified()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: true));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")),
            MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Empty(serializationInfoCollection.Enums);
        Assert.Equal(3, serializationInfoCollection.Generics.Count());
        Assert.Equal("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MagicOnion.Serialization.MessagePack.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());
        Assert.Equal("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String>()", serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs());
    }

    [Fact]
    public void DisableMapUserDefinedMessagePackSerializerFormatters()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MessagePackFormatterNameMapper(string.Empty, allowToMapUserDefinedFormatter: false));
        var types = new[]
        {
            MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")),
            MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()),
            MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()),
            MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateFromType<int>()),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        Assert.NotNull(serializationInfoCollection);
        Assert.Single(serializationInfoCollection.Enums);
        Assert.Equal(2, serializationInfoCollection.Generics.Count());

        // Enum
        Assert.Equal("MyEnumFormatter()", serializationInfoCollection.Enums[0].GetFormatterNameWithConstructorArgs());
        // Built-in
        Assert.Equal("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()", serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs());
        // MagicOnion
        Assert.Equal("global::MagicOnion.Serialization.MessagePack.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))", serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs());

        Assert.DoesNotContain(serializationInfoCollection.Generics, x => x.FormatterName == "global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String>");
    }

}

file static class SerializationFormatterRegisterInfoExtensions
{
    public static string GetFormatterNameWithConstructorArgs(this ISerializationFormatterRegisterInfo serializationFormatterRegisterInfo)
        => serializationFormatterRegisterInfo.FormatterName + serializationFormatterRegisterInfo.FormatterConstructorArgs;
}
