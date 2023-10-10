using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Xunit.Abstractions;

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
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<int>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject"), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject"), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject"), new string[] { "CONST_2 || DEBUG"}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject"), new string[] { "CONST_3" }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Enums.Should().BeEmpty();
        serializationInfoCollection.Generics.Should().BeEmpty();
        serializationInfoCollection.TypeHints.Should().HaveCount(3);
    }

    [Fact]
    public void Nullable()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject")), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Enums.Should().BeEmpty();
        serializationInfoCollection.Generics.Should().HaveCount(2);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.YetAnotherGenericObject>()");
        serializationInfoCollection.TypeHints.Should().HaveCount(4); // Non-nullable + Nullable
    }

    [Fact]
    public void Generics_MergeIfDirectives()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<int>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject", MagicOnionTypeInfo.CreateFromType<string>()), new string[] { "CONST_2 || DEBUG"}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "YetAnotherGenericObject", MagicOnionTypeInfo.CreateFromType<string>()), new string[] { "CONST_3" }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(2);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String>()");
        // NOTE: If there is a type without `if` condition, then merged if condition is always empty. (The type is always required by consumers)
        serializationInfoCollection.Generics[0].IfDirectiveConditions.Should().BeEmpty();
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.MyNamespace.YetAnotherGenericObjectFormatter<global::System.String>()");
        serializationInfoCollection.Generics[1].IfDirectiveConditions.Should().HaveCount(2);
        serializationInfoCollection.Generics[1].IfDirectiveConditions.Should().BeEquivalentTo("CONST_2 || DEBUG", "CONST_3");
        serializationInfoCollection.TypeHints.Should().HaveCount(4); // int, string, MyGenericObject<string>, YetAnotherGenericObject<string>
    }

    [Fact]
    public void Generics_ManyTypeArguments()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<int>()), new string[] { "CONST_1" }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(2);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String, global::System.Int64>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String, global::System.Int32>()");
        serializationInfoCollection.TypeHints.Should().HaveCount(5); // string, int, long, MyGenericObject<string, long>, MyGenericObject<string, int>
    }

    [Fact]
    public void Enum()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateFromType<int>()), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnumConditional", MagicOnionTypeInfo.CreateFromType<int>()), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateFromType<int>())), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnumConditional", MagicOnionTypeInfo.CreateFromType<int>())), new string[] { "CONST_2" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateEnum("MyNamespace", "YetAnotherEnum", MagicOnionTypeInfo.CreateFromType<int>())), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(3);
        serializationInfoCollection.Enums.Should().HaveCount(3);
        serializationInfoCollection.Enums[0].Namespace.Should().Be("MyNamespace");
        serializationInfoCollection.Enums[0].Name.Should().Be("MyEnum");
        serializationInfoCollection.Enums[0].GetFormatterNameWithConstructorArgs().Should().Be("MyEnumFormatter()");
        serializationInfoCollection.Enums[0].HasIfDirectiveConditions.Should().BeFalse();
        serializationInfoCollection.Enums[1].Namespace.Should().Be("MyNamespace");
        serializationInfoCollection.Enums[1].Name.Should().Be("MyEnumConditional");
        serializationInfoCollection.Enums[1].GetFormatterNameWithConstructorArgs().Should().Be("MyEnumConditionalFormatter()");
        serializationInfoCollection.Enums[1].IfDirectiveConditions.Should().BeEquivalentTo("CONST_1", "CONST_2");
        serializationInfoCollection.Enums[2].Namespace.Should().Be("MyNamespace");
        serializationInfoCollection.Enums[2].Name.Should().Be("YetAnotherEnum");
        serializationInfoCollection.Enums[2].GetFormatterNameWithConstructorArgs().Should().Be("YetAnotherEnumFormatter()");
        serializationInfoCollection.Enums[2].HasIfDirectiveConditions.Should().BeFalse();
        serializationInfoCollection.TypeHints.Should().HaveCount(6); // MyEnum, MyEnumConditional, YetAnotherEnum, MyGenericObject<MyEnum>, MyGenericObject<MyEnumConditional>, MyGenericObject<YetAnotherEnum>
    }
        
    [Fact]
    public void KnownTypes_SkipBuiltInGenericTypes()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ArraySegment<byte>>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ArraySegment<byte>?>(), new string[] {}),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().BeEmpty();
        serializationInfoCollection.TypeHints.Should().HaveCount(3); // byte, ArraySegment<byte>, Nullable<ArraySegment<byte>>
    }

    [Fact]
    public void KnownTypes_Array_SkipBuiltInTypes()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<byte[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<short[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<int[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<long[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<sbyte[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ushort[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<uint[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ulong[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<string[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<decimal[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<float[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<double[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Guid[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<char[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<DateTime[]>(), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MessagePack", "Nil")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Vector2")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Vector3")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Vector4")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Quaternion")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Color")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Bounds")), new string[] {}),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("UnityEngine", "Rect")), new string[] {}),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().BeEmpty();
        serializationInfoCollection.TypeHints.Should().HaveCount(46);
    }  
    
    [Fact]
    public void KnownTypes_Array_NonBuiltIn()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject"), arrayRank: 2), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject"), arrayRank: 3), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("MyNamespace", "MyObject"), arrayRank: 4), new string[] { "CONST_1" }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(4);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ArrayFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TwoDimensionalArrayFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ThreeDimensionalArrayFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.FourDimensionalArrayFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.TypeHints.Should().HaveCount(5); // MyObject, MyObject[], MyObject[,], MyObject[,,], MyObject[,,,]
    }

    [Fact]
    public void KnownTypes_Generics()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "List",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "IList",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "IReadOnlyList",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "Dictionary",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "IDictionary",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "IReadOnlyDictionary",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "ICollection",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "IReadOnlyCollection",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "IEnumerable",MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Collections.Generic", "KeyValuePair",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Linq", "ILookup",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System.Linq", "IGrouping",MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.Create("MyNamespace", "MyObject")), new string[] { "CONST_1" }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(12);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ListFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceListFormatter2<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceReadOnlyListFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.DictionaryFormatter<global::System.String, global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[4].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceDictionaryFormatter<global::System.String, global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[5].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceReadOnlyDictionaryFormatter<global::System.String, global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[6].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceCollectionFormatter2<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[7].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceReadOnlyCollectionFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[8].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceEnumerableFormatter<global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[9].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.KeyValuePairFormatter<global::System.String, global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[10].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceLookupFormatter<global::System.String, global::MyNamespace.MyObject>()");
        serializationInfoCollection.Generics[11].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.InterfaceGroupingFormatter<global::System.String, global::MyNamespace.MyObject>()");
        serializationInfoCollection.TypeHints.Should().HaveCount(14);
    }
    
    [Fact]
    public void KnownTypes_Nullable()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<short?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<int?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<long?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<byte?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ushort?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<uint?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ulong?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<sbyte?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<bool?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<char?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Guid?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<DateTime?>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<TimeSpan?>(), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Enums.Should().BeEmpty();
        serializationInfoCollection.Generics.Should().BeEmpty();
        serializationInfoCollection.TypeHints.Should().HaveCount(26);
    }

    [Fact]
    public void KnownTypes_ValueTuple()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool, byte>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool, byte, short>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string, long, float, bool, byte, short, Guid>>(), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(8);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String>()");
        serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64>()");
        serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single>()");
        serializationInfoCollection.Generics[4].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean>()");
        serializationInfoCollection.Generics[5].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte>()");
        serializationInfoCollection.Generics[6].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16>()");
        serializationInfoCollection.Generics[7].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.ValueTupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16, global::System.Guid>()");
        serializationInfoCollection.TypeHints.Should().HaveCount(8 + 8);
    }

    [Fact]
    public void KnownTypes_Tuple()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool, byte>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool, byte, short>>(), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.CreateFromType<Tuple<int, string, long, float, bool, byte, short, Guid>>(), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(8);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String>()");
        serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64>()");
        serializationInfoCollection.Generics[3].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single>()");
        serializationInfoCollection.Generics[4].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean>()");
        serializationInfoCollection.Generics[5].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte>()");
        serializationInfoCollection.Generics[6].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16>()");
        serializationInfoCollection.Generics[7].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.TupleFormatter<global::System.Int32, global::System.String, global::System.Int64, global::System.Single, global::System.Boolean, global::System.Byte, global::System.Int16, global::System.Guid>()");
    }

    [Fact]
    public void DynamicArgumentTuple()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<int>()), new string[] { "CONST_1" }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Generics.Should().HaveCount(2);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MagicOnion.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MagicOnion.DynamicArgumentTupleFormatter<global::System.String, global::System.Int32>(default(global::System.String), default(global::System.Int32))");
        serializationInfoCollection.TypeHints.Should().HaveCount(5); // string, long, int, DynamicArgumentTuple<string, long>, DynamicArgumentTuple<string, int>
    }

    [Fact]
    public void UserDefinedMessagePackSerializerFormattersNamespace()
    {
        // Arrange
        var userDefinedMessagePackFormattersNamespace = "MyFormatters";
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(userDefinedMessagePackFormattersNamespace));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Enums.Should().BeEmpty();
        serializationInfoCollection.Generics.Should().HaveCount(3);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MagicOnion.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))");
        serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs().Should().Be("global::MyFormatters.MyNamespace.MyGenericObjectFormatter<global::System.String>()");
    }

    [Fact]
    public void UserDefinedMessagePackSerializerFormattersNamespace_NotSpecified()
    {
        // Arrange
        var collector = new SerializationInfoCollector(new MagicOnionGeneratorTestOutputLogger(testOutputHelper), new MessagePackFormatterNameMapper(string.Empty));
        var types = new[]
        {
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("System", "Nullable", MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject")), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", MagicOnionTypeInfo.CreateFromType<string>(), MagicOnionTypeInfo.CreateFromType<long>()), new string[] { }),
            new SerializationInfoCollector.TypeWithIfDirectives(MagicOnionTypeInfo.Create("MyNamespace", "MyGenericObject", MagicOnionTypeInfo.CreateFromType<string>()), new string[] { }),
        };

        // Act
        var serializationInfoCollection = collector.Collect(types);

        // Assert
        serializationInfoCollection.Should().NotBeNull();
        serializationInfoCollection.Enums.Should().BeEmpty();
        serializationInfoCollection.Generics.Should().HaveCount(3);
        serializationInfoCollection.Generics[0].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.NullableFormatter<global::MyNamespace.MyGenericObject>()");
        serializationInfoCollection.Generics[1].GetFormatterNameWithConstructorArgs().Should().Be("global::MagicOnion.DynamicArgumentTupleFormatter<global::System.String, global::System.Int64>(default(global::System.String), default(global::System.Int64))");
        serializationInfoCollection.Generics[2].GetFormatterNameWithConstructorArgs().Should().Be("global::MessagePack.Formatters.MyNamespace.MyGenericObjectFormatter<global::System.String>()");
    }
}

file static class SerializationFormatterRegisterInfoExtensions
{
    public static string GetFormatterNameWithConstructorArgs(this ISerializationFormatterRegisterInfo serializationFormatterRegisterInfo)
        => serializationFormatterRegisterInfo.FormatterName + serializationFormatterRegisterInfo.FormatterConstructorArgs;
}
