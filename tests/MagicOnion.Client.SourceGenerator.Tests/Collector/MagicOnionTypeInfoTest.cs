using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.Tests.Collector;

public class MagicOnionTypeInfoTest
{
    [Fact]
    public void Simple()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.Create("System", "String");

        // Assert
        var fullName = typeInfo.FullName;
        Assert.Equal("global::System.String", fullName);
        Assert.False(typeInfo.IsArray);
        Assert.False(typeInfo.IsEnum);
        Assert.Null(typeInfo.ElementType);
        Assert.Null(typeInfo.UnderlyingType);
    }

    [Fact]
    public void CreateArray()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateArray("System", "String");

        // Assert
        var fullName = typeInfo.FullName;
        Assert.Equal("global::System.String[]", fullName);
        Assert.True(typeInfo.IsArray);
        Assert.Equal(MagicOnionTypeInfo.Create("System", "String"), typeInfo.ElementType);
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
        Assert.Equal("global::System.Tuple<global::System.Int32[], global::System.String>[]", fullName);
        Assert.True(typeInfo.IsArray);
    }
    
    [Fact]
    public void CreateArray_JaggedArray()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("System", "String[]"));

        // Assert
        var fullName = typeInfo.FullName;
        Assert.Equal("global::System.String[][]", fullName);
        Assert.True(typeInfo.IsArray);
        Assert.Equal(MagicOnionTypeInfo.Create("System", "String[]"), typeInfo.ElementType); // NOTE: Currently, MOTypeInfo doesn't handle an element type for jagged array.
    }
       
    [Fact]
    public void CreateArray_Rank()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.Create("System", "String"), 3);

        // Assert
        var fullName = typeInfo.FullName;
        Assert.Equal("global::System.String[,,]", fullName);
        Assert.Equal(3, typeInfo.ArrayRank);
        Assert.True(typeInfo.IsArray);
    }

    [Fact]
    public void CreateFromType()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<string>();

        // Assert
        Assert.Equal("global::System.String", typeInfo.FullName);
        Assert.False(typeInfo.IsEnum);
        Assert.False(typeInfo.IsArray);
        Assert.False(typeInfo.IsValueType);
    }

    [Fact]
    public void CreateFromType_ValueType()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<byte>();

        // Assert
        Assert.Equal("global::System.Byte", typeInfo.FullName);
        Assert.False(typeInfo.IsEnum);
        Assert.False(typeInfo.IsArray);
        Assert.True(typeInfo.IsValueType);
    }

    [Fact]
    public void CreateFromType_Generics()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<int, string>>();

        // Assert
        Assert.Equal("global::System.Tuple<global::System.Int32, global::System.String>", typeInfo.FullName);
        Assert.Equal(2, typeInfo.GenericArguments.Count());
        Assert.False(typeInfo.IsEnum);
        Assert.False(typeInfo.IsArray);
    }
    
    [Fact]
    public void CreateFromType_Array()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<int[]>();

        // Assert
        Assert.Equal("global::System.Int32[]", typeInfo.FullName);
        Assert.Empty(typeInfo.GenericArguments);
        Assert.False(typeInfo.IsEnum);
        Assert.True(typeInfo.IsArray);
        Assert.Equal(MagicOnionTypeInfo.CreateValueType("System", "Int32"), typeInfo.ElementType);
        Assert.Equal(1, typeInfo.ArrayRank);
    }

    [Fact]
    public void CreateFromType_Array_Rank()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<int[,,]>();

        // Assert
        Assert.Equal("global::System.Int32[,,]", typeInfo.FullName);
        Assert.Empty(typeInfo.GenericArguments);
        Assert.False(typeInfo.IsEnum);
        Assert.True(typeInfo.IsArray);
        Assert.Equal(MagicOnionTypeInfo.CreateValueType("System", "Int32"), typeInfo.ElementType);
        Assert.Equal(3, typeInfo.ArrayRank);
    }

    [Fact]
    public void CreateFromType_Enum()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<DayOfWeek>();

        // Assert
        Assert.Equal("global::System.DayOfWeek", typeInfo.FullName);
        Assert.Empty(typeInfo.GenericArguments);
        Assert.True(typeInfo.IsEnum);
        Assert.True(typeInfo.IsValueType);
        Assert.Equal(MagicOnionTypeInfo.CreateValueType("System", "Int32"), typeInfo.UnderlyingType);
        Assert.False(typeInfo.IsArray);
        Assert.Null(typeInfo.ElementType);
    }
    
    [Fact]
    public void CreateFromType_Enum_Array()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateFromType<DayOfWeek[]>();

        // Assert
        Assert.Equal("global::System.DayOfWeek[]", typeInfo.FullName);
        Assert.Empty(typeInfo.GenericArguments);
        Assert.False(typeInfo.IsEnum);
        Assert.Null(typeInfo.UnderlyingType);
        Assert.True(typeInfo.IsArray);
        Assert.NotNull(typeInfo.ElementType);
        Assert.Equal(MagicOnionTypeInfo.CreateEnum("System", "DayOfWeek", MagicOnionTypeInfo.CreateValueType("System", "Int32")), typeInfo.ElementType);
        Assert.True(typeInfo.ElementType.IsEnum);
        Assert.False(typeInfo.ElementType.IsArray);
        Assert.True(typeInfo.ElementType.IsValueType);
        Assert.Equal(MagicOnionTypeInfo.CreateValueType("System", "Int32"), typeInfo.ElementType.UnderlyingType);
    }

    [Fact]
    public void CreateValueType()
    {
        // Arrange & Act
        var typeInfo = MagicOnionTypeInfo.CreateValueType("MyNamespace", "MyStruct", new [] { MagicOnionTypeInfo.CreateFromType<byte>() });

        // Assert
        Assert.True(typeInfo.IsValueType);
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
        Assert.Equal("global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>", fullName);
        Assert.Equal([MagicOnionTypeInfo.Create("System", "String"), MagicOnionTypeInfo.CreateValueType("System", "Int32")], genericArguments);
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
        Assert.Equal("global::System.Collections.Generic.Dictionary<global::System.String, global::System.Tuple<global::System.Double, global::System.Byte>>", fullName);
        Assert.Equal([MagicOnionTypeInfo.CreateValueType("System", "Double"), MagicOnionTypeInfo.CreateValueType("System", "Byte")], genericArgumentsNested);
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
        Assert.Empty(typeInfo.Namespace);
        Assert.Equal("MyClass", typeInfo.Name);
        Assert.Equal("global::MyClass", typeInfo.FullName);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "MyClass", SymbolFilter.Type, TestContext.Current.CancellationToken).OfType<INamedTypeSymbol>().ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0]);

        // Assert
        Assert.Equal("MyNamespace", typeInfo.Namespace);
        Assert.Equal("MyClass", typeInfo.Name);
        Assert.Equal("global::MyNamespace.MyClass", typeInfo.FullName);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.Create("System", "Tuple",
            MagicOnionTypeInfo.CreateValueType("System", "Nullable", 
                MagicOnionTypeInfo.CreateValueType("System", "Boolean")),
            MagicOnionTypeInfo.CreateValueType("System", "Nullable",
                MagicOnionTypeInfo.CreateValueType("System", "Int64"))), typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateValueType( "System", "Int32", Array.Empty<MagicOnionTypeInfo>()), typeInfo);
        Assert.True(typeInfo.IsValueType);
        Assert.False(typeInfo.IsEnum);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateEnum( "MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateValueType("System", "Byte")), typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.CreateEnum( "MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateValueType("System", "Byte"))), typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.Create("System", "Tuple", 
            MagicOnionTypeInfo.CreateValueType("System", "Int32"),
            MagicOnionTypeInfo.CreateEnum( "MyNamespace", "MyEnum", MagicOnionTypeInfo.CreateValueType("System", "Byte"))),
            typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateArray(MagicOnionTypeInfo.CreateValueType("System", "Int32")), typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateArray("System", "Int32[]"), typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateArray("System", "Tuple", 
            MagicOnionTypeInfo.CreateValueType("System", "Int32"),
            MagicOnionTypeInfo.Create("System", "String")),
            typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal(MagicOnionTypeInfo.CreateArray("System", "String", Array.Empty<MagicOnionTypeInfo>(), 3), typeInfo);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal("System.Collections.Generic", typeInfo.Namespace);
        Assert.Equal("List", typeInfo.Name);
        Assert.Equal(1, typeInfo.GenericArguments.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("System", "String"), typeInfo.GenericArguments[0]);
        Assert.Equal("global::System.Collections.Generic.List<global::System.String>", typeInfo.FullName);
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
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal("System.Collections.Generic", typeInfo.Namespace);
        Assert.Equal("List", typeInfo.Name);
        Assert.Equal(1, typeInfo.GenericArguments.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("System", "Tuple", MagicOnionTypeInfo.CreateValueType("System", "Int32"), MagicOnionTypeInfo.Create("System", "String")), typeInfo.GenericArguments[0]);
        Assert.Equal("global::System.Collections.Generic.List<global::System.Tuple<global::System.Int32, global::System.String>>", typeInfo.FullName);
    }

    [Fact]
    public void FromSymbol_NamedValueTuple()
    {
        // Arrange
        var (compilation, semModel) = CompilationHelper.Create(@"
            namespace MyNamespace
            {
                using System;
                using System.Collections.Generic;
                public class MyClass
                {
                    public (string Name, int Age) FieldA;
                }
            }
        ");
        var symbols = compilation.GetSymbolsWithName(x => x == "FieldA", SymbolFilter.Member, TestContext.Current.CancellationToken)
            .OfType<IFieldSymbol>()
            .ToArray();

        // Act
        var typeInfo = MagicOnionTypeInfo.CreateFromSymbol(symbols[0].Type);

        // Assert
        Assert.Equal("System", typeInfo.Namespace);
        Assert.Equal("ValueTuple", typeInfo.Name);
        Assert.Equal(2, typeInfo.GenericArguments.Count());
        Assert.Equal(MagicOnionTypeInfo.Create("System", "String"), typeInfo.GenericArguments[0]);
        Assert.Equal(MagicOnionTypeInfo.CreateValueType("System", "Int32"), typeInfo.GenericArguments[1]);
        Assert.Equal("global::System.ValueTuple<global::System.String, global::System.Int32>", typeInfo.FullName);
    }

    [Fact]
    public void ToDisplay_Short()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<MessagePack.Nil>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Short);
        // Assert
        Assert.Equal("Nil", formatted);
    }
    
    [Fact]
    public void ToDisplay_Short_Nested()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Short);
        // Assert
        Assert.Equal("Tuple<String, Int32>", formatted);
    }

    [Fact]
    public void ToDisplay_FullyQualified()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<MessagePack.Nil>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.FullyQualified);
        // Assert
        Assert.Equal("global::MessagePack.Nil", formatted);
    }

    [Fact]
    public void ToDisplay_FullyQualified_Nested()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.FullyQualified);
        // Assert
        Assert.Equal("global::System.Tuple<global::System.String, global::System.Int32>", formatted);
    }

    [Fact]
    public void ToDisplay_Namespace()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<MessagePack.Nil>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace);
        // Assert
        Assert.Equal("MessagePack.Nil", formatted);
    }

    [Fact]
    public void ToDisplay_OpenGenerics()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.OpenGenerics);
        // Assert
        Assert.Equal("Tuple<,>", formatted);
    }
    
    [Fact]
    public void ToDisplay_WithoutGenericArguments()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<string, int>>();
        // Act
        var formatted = typeInfo.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.WithoutGenericArguments);
        // Assert
        Assert.Equal("Tuple", formatted);
    }

    [Fact]
    public void EnumerateDependentTypes_Array()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<int[]>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        Assert.Equal([MagicOnionTypeInfo.CreateFromType<int>()], types);
    }
    
    [Fact]
    public void EnumerateDependentTypes_Enum_Array()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<DayOfWeek[]>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        Assert.Equal([MagicOnionTypeInfo.CreateFromType<DayOfWeek>()], types);
    }

    [Fact]
    public void EnumerateDependentTypes_Enum_Generics()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Task<DayOfWeek[]>>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        Assert.Equal([MagicOnionTypeInfo.CreateFromType<DayOfWeek[]>(), MagicOnionTypeInfo.CreateFromType<DayOfWeek>()], types);
    }

    [Fact]
    public void EnumerateDependentTypes_Generics()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<int, string>>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        Assert.Equal([MagicOnionTypeInfo.CreateFromType<int>(), MagicOnionTypeInfo.CreateFromType<string>()], types);
    }
    
    [Fact]
    public void EnumerateDependentTypes_Generics_Nested()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<Tuple<int, Dictionary<string, Task<byte[]>>>>();
        // Act
        var types = typeInfo.EnumerateDependentTypes().ToArray();
        // Assert
        Assert.Equal([
                MagicOnionTypeInfo.CreateFromType<int>(),
                MagicOnionTypeInfo.CreateFromType<Dictionary<string, Task<byte[]>>>(),
                MagicOnionTypeInfo.CreateFromType<string>(),
                MagicOnionTypeInfo.CreateFromType<Task<byte[]>>(),
                MagicOnionTypeInfo.CreateFromType<byte[]>(),
                MagicOnionTypeInfo.CreateFromType<byte>()
            ],
            types);
    }

    [Fact]
    public void GetGenericTypeDefinition()
    {
        // Arrange
        var typeInfo = MagicOnionTypeInfo.CreateFromType<ValueTuple<int, string>>();
        // Act
        var genericDefinition = typeInfo.GetGenericTypeDefinition();
        // Assert
        Assert.Equal(MagicOnionTypeInfo.Create("System", "ValueTuple", Array.Empty<MagicOnionTypeInfo>(), true), genericDefinition);
    }
}
