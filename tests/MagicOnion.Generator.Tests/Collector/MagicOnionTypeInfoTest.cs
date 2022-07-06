using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MagicOnion.Generator.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.Tests.Collector;

public class MagicOnionTypeInfoTest
{
    [Fact]
    public void Simple()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.Create("System", "String");

        // Assert
        var fullName = typeInfo.FullName;
        fullName.Should().Be("global::System.String");
        typeInfo.IsArray.Should().BeFalse();
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.ElementType.Should().BeNull();
        typeInfo.UnderlyingType.Should().BeNull();
    }

    [Fact]
    public void CreateArray()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateArray("System", "String");

        // Assert
        var fullName = typeInfo.FullName;
        fullName.Should().Be("global::System.String[]");
        typeInfo.IsArray.Should().BeTrue();
        typeInfo.ElementType.Should().Be(MagicOnionTypeInfo.Create("System", "String"));
    }
    
    [Fact]
    public void CreateArray_Generics()
    {
        // Arrange & Act
        // Tuple<int[], string>[]
        var typeInfo = MagicOnionTypeInfo.CreateArray("System", "Tuple",
            MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.CreateValueType("System", "Int32")), MagicOnionTypeInfo.Create("System", "String"));

        // Assert
        var fullName = typeInfo.FullName;
        fullName.Should().Be("global::System.Tuple<global::System.Int32[], global::System.String>[]");
        typeInfo.IsArray.Should().BeTrue();
    }
    
    [Fact]
    public void CreateArray_JaggedArray()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("System", "String[]"));

        // Assert
        var fullName = typeInfo.FullName;
        fullName.Should().Be("global::System.String[][]");
        typeInfo.IsArray.Should().BeTrue();
        typeInfo.ElementType.Should().Be(MagicOnionTypeInfo.Create("System", "String[]")); // NOTE: Currently, MOTypeInfo doesn't handle an element type for jagged array.
    }
       
    [Fact]
    public void CreateArray_Rank()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("System", "String"), 3);

        // Assert
        var fullName = typeInfo.FullName;
        fullName.Should().Be("global::System.String[,,]");
        typeInfo.ArrayRank.Should().Be(3);
        typeInfo.IsArray.Should().BeTrue();
    }

    [Fact]
    public void CreateFromType()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<string>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.String");
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.IsArray.Should().BeFalse();
        typeInfo.IsValueType.Should().BeFalse();
    }

    [Fact]
    public void CreateFromType_ValueType()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<byte>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.Byte");
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.IsArray.Should().BeFalse();
        typeInfo.IsValueType.Should().BeTrue();
    }

    [Fact]
    public void CreateFromType_Generics()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<int, string>>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.Tuple<global::System.Int32, global::System.String>");
        typeInfo.GenericArguments.Should().HaveCount(2);
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.IsArray.Should().BeFalse();
    }
    
    [Fact]
    public void CreateFromType_Array()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<int[]>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.Int32[]");
        typeInfo.GenericArguments.Should().BeEmpty();
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.IsArray.Should().BeTrue();
        typeInfo.ElementType.Should().Be(MagicOnionTypeInfo.CreateValueType("System", "Int32"));
        typeInfo.ArrayRank.Should().Be(1);
    }

    [Fact]
    public void CreateFromType_Array_Rank()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<int[,,]>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.Int32[,,]");
        typeInfo.GenericArguments.Should().BeEmpty();
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.IsArray.Should().BeTrue();
        typeInfo.ElementType.Should().Be(MagicOnionTypeInfo.CreateValueType("System", "Int32"));
        typeInfo.ArrayRank.Should().Be(3);
    }

    [Fact]
    public void CreateFromType_Enum()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<DayOfWeek>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.DayOfWeek");
        typeInfo.GenericArguments.Should().BeEmpty();
        typeInfo.IsEnum.Should().BeTrue();
        typeInfo.IsValueType.Should().BeTrue();
        typeInfo.UnderlyingType.Should().Be(MagicOnionTypeInfo.CreateValueType("System", "Int32"));
        typeInfo.IsArray.Should().BeFalse();
        typeInfo.ElementType.Should().BeNull();
    }
    
    [Fact]
    public void CreateFromType_Enum_Array()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<DayOfWeek[]>();

        // Assert
        typeInfo.FullName.Should().Be("global::System.DayOfWeek[]");
        typeInfo.GenericArguments.Should().BeEmpty();
        typeInfo.IsEnum.Should().BeFalse();
        typeInfo.UnderlyingType.Should().BeNull();
        typeInfo.IsArray.Should().BeTrue();
        typeInfo.ElementType.Should().Be(MagicOnionTypeInfo.CreateEnum("System", "DayOfWeek", MagicOnionTypeInfo.CreateValueType("System", "Int32")));
        typeInfo.ElementType.IsEnum.Should().BeTrue();
        typeInfo.ElementType.IsArray.Should().BeFalse();
        typeInfo.ElementType.IsValueType.Should().BeTrue();
        typeInfo.ElementType.UnderlyingType.Should().Be(MagicOnionTypeInfo.CreateValueType("System", "Int32"));
    }

    [Fact]
    public void CreateValueType()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateValueType("MyNamespace", "MyStruct", new [] { MagicOnionTypeInfo.CreateFromType<byte>() });

        // Assert
        typeInfo.IsValueType.Should().BeTrue();
    }

    [Fact]
    public void CreateEnum()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateEnum("MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateFromType<byte>());
    }

    [Fact]
    public void WithGenericArgument()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.Create("System.Collections.Generic", "Dictionary",
            MagicOnionTypeInfo.Create("System", "String"),
                MagicOnionTypeInfo.CreateValueType("System", "Int32"));

        // Assert
        var fullName = typeInfo.FullName;
        var genericArguments = typeInfo.GenericArguments;
        fullName.Should().Be("global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>");
        genericArguments.Should().BeEquivalentTo(MagicOnionTypeInfo.Create("System", "String"), MagicOnionTypeInfo.CreateValueType("System", "Int32"));
    }

    [Fact]
    public void WithGenericArgumentNested()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.Create("System.Collections.Generic", "Dictionary",
            MagicOnionTypeInfo.Create("System", "String"),
            MagicOnionTypeInfo.Create("System", "Tuple",
                MagicOnionTypeInfo.CreateValueType("System", "Double"),
                MagicOnionTypeInfo.CreateValueType("System", "Byte")));

        // Assert
        var fullName = typeInfo.FullName;
        var genericArgumentsNested = typeInfo.GenericArguments[1].GenericArguments;
        fullName.Should().Be("global::System.Collections.Generic.Dictionary<global::System.String, global::System.Tuple<global::System.Double, global::System.Byte>>");
        genericArgumentsNested.Should().BeEquivalentTo(MagicOnionTypeInfo.CreateValueType("System", "Double"), MagicOnionTypeInfo.CreateValueType("System", "Byte"));
    }

    [Fact]
    public void FromSymbol_Global()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            public class MyClass { }
        ");
        var symbols = compilation.GlobalNamespace.GetTypeMembers("MyClass");

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0]);

        // Assert
        typeInfo.Namespace.Should().BeEmpty();
        typeInfo.Name.Should().Be("MyClass");
        typeInfo.FullName.Should().Be("global::MyClass");
    }

    [Fact]
    public void FromSymbol_Namespaced()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass { }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "MyClass", SymbolFilter.Type).OfType<INamedTypeSymbol>().ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0]);

        // Assert
        typeInfo.Namespace.Should().Be("MyNamespace");
        typeInfo.Name.Should().Be("MyClass");
        typeInfo.FullName.Should().Be("global::MyNamespace.MyClass");
    }

    [Fact]
    public void FromSymbol_Nullable()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public System.Tuple<bool?, long?> FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.Create("System", "Tuple",
            MagicOnionTypeInfo.CreateValueType("System", "Nullable", 
                MagicOnionTypeInfo.CreateValueType("System", "Boolean")),
            MagicOnionTypeInfo.CreateValueType("System", "Nullable",
                MagicOnionTypeInfo.CreateValueType("System", "Int64"))));
    }
       
    [Fact]
    public void FromSymbol_ValueType()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public int FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateValueType( "System", "Int32", Array.Empty<MagicOnionTypeInfo>()));
        typeInfo.IsValueType.Should().BeTrue();
        typeInfo.IsEnum.Should().BeFalse();
    }

    [Fact]
    public void FromSymbol_Enum()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public enum MyEnum : byte
                {
                    A, B, C
                }
                public class MyClass
                {
                    public MyEnum FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateEnum( "MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateValueType("System", "Byte")));
    }
        
    [Fact]
    public void FromSymbol_Enum_Array()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public enum MyEnum : byte
                {
                    A, B, C
                }
                public class MyClass
                {
                    public MyEnum[] FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.CreateEnum( "MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateValueType("System", "Byte"))));
    }
           
    [Fact]
    public void FromSymbol_Enum_Generics()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public enum MyEnum : byte
                {
                    A, B, C
                }
                public class MyClass
                {
                    public System.Tuple<int, MyEnum> FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.Create("System", "Tuple", 
            MagicOnionTypeInfo.CreateValueType("System", "Int32"),
            MagicOnionTypeInfo.CreateEnum( "MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateValueType("System", "Byte"))));
    }
    
    [Fact]
    public void FromSymbol_Array()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public int[] FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.CreateValueType("System", "Int32")));
    }
        
    [Fact]
    public void FromSymbol_JaggedArray()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public int[][] FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateArray("System", "Int32[]"));
    }

    [Fact]
    public void FromSymbol_ArrayGenerics()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public System.Tuple<int, string>[] FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateArray("System", "Tuple", 
            MagicOnionTypeInfo.CreateValueType("System", "Int32"),
            MagicOnionTypeInfo.Create("System", "String")));
    }
    
    [Fact]
    public void FromSymbol_ArrayRank()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public string[,,] FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Should().Be(MagicOnionTypeInfo.CreateArray("System", "String", Array.Empty<MagicOnionTypeInfo>(), 3));
    }

    [Fact]
    public void FromSymbol_Generics()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                public class MyClass
                {
                    public System.Collections.Generic.List<string> FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Namespace.Should().Be("System.Collections.Generic");
        typeInfo.Name.Should().Be("List");
        typeInfo.GenericArguments.Should().HaveCount(1);
        typeInfo.GenericArguments[0].Should().Be(MagicOnionTypeInfo.Create("System", "String"));
        typeInfo.FullName.Should().Be("global::System.Collections.Generic.List<global::System.String>");
    }

    [Fact]
    public void FromSymbol_Generics_Nested()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                using System;
                using System.Collections.Generic;
                public class MyClass
                {
                    public List<Tuple<int, string>> FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        typeInfo.Namespace.Should().Be("System.Collections.Generic");
        typeInfo.Name.Should().Be("List");
        typeInfo.GenericArguments.Should().HaveCount(1);
        typeInfo.GenericArguments[0].Should().Be(MagicOnionTypeInfo.Create("System", "Tuple", MagicOnionTypeInfo.CreateValueType("System", "Int32"), MagicOnionTypeInfo.Create("System", "String")));
        typeInfo.FullName.Should().Be("global::System.Collections.Generic.List<global::System.Tuple<global::System.Int32, global::System.String>>");
    }

    [Fact]
    public void ToDisplay_Short()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<MessagePack.Nil>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Short);
        // Assert
        formatted.Should().Be("Nil");
    }
    
    [Fact]
    public void ToDisplay_Short_Nested()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Short);
        // Assert
        formatted.Should().Be("Tuple<String, Int32>");
    }

    [Fact]
    public void ToDisplay_FullyQualified()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<MessagePack.Nil>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.FullyQualified);
        // Assert
        formatted.Should().Be("global::MessagePack.Nil");
    }

    [Fact]
    public void ToDisplay_FullyQualified_Nested()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.FullyQualified);
        // Assert
        formatted.Should().Be("global::System.Tuple<global::System.String, global::System.Int32>");
    }

    [Fact]
    public void ToDisplay_Namespace()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<MessagePack.Nil>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace);
        // Assert
        formatted.Should().Be("MessagePack.Nil");
    }

    [Fact]
    public void ToDisplay_OpenGenerics()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.OpenGenerics);
        // Assert
        formatted.Should().Be("Tuple<,>");
    }
    
    [Fact]
    public void ToDisplay_WithoutGenericArguments()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.WithoutGenericArguments);
        // Assert
        formatted.Should().Be("Tuple");
    }

    [Fact]
    public void EnumerateDependentTypes_Array()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<int[]>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        types.Should().BeEquivalentTo(MagicOnionTypeInfo.CreateFromType<int>());
    }
    
    [Fact]
    public void EnumerateDependentTypes_Enum_Array()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<DayOfWeek[]>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        types.Should().BeEquivalentTo(MagicOnionTypeInfo.CreateFromType<DayOfWeek>());
    }    

    [Fact]
    public void EnumerateDependentTypes_Enum_Generics()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Task<DayOfWeek[]>>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        types.Should().BeEquivalentTo(MagicOnionTypeInfo.CreateFromType<DayOfWeek[]>(), MagicOnionTypeInfo.CreateFromType<DayOfWeek>());
    }

    [Fact]
    public void EnumerateDependentTypes_Generics()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<int, string>>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        types.Should().BeEquivalentTo(MagicOnionTypeInfo.CreateFromType<int>(), MagicOnionTypeInfo.CreateFromType<string>());
    }
    
    [Fact]
    public void EnumerateDependentTypes_Generics_Nested()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<int, Dictionary<string, Task<byte[]>>>>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        types.Should().BeEquivalentTo(
            MagicOnionTypeInfo.CreateFromType<int>(),
            MagicOnionTypeInfo.CreateFromType<Dictionary<string, Task<byte[]>>>(),
            MagicOnionTypeInfo.CreateFromType<string>(),
            MagicOnionTypeInfo.CreateFromType<Task<byte[]>>(),
            MagicOnionTypeInfo.CreateFromType<byte[]>(),
            MagicOnionTypeInfo.CreateFromType<byte>());
    }
}