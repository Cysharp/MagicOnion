using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.CodeAnalysis;

[DebuggerDisplay("{ToDisplayName(DisplayNameFormat.Short),nq}")]
public class MagicOnionTypeInfo : IEquatable<MagicOnionTypeInfo>
{
    public static class KnownTypes
    {
        // ReSharper disable InconsistentNaming
        public static MagicOnionTypeInfo System_Void { get; } = new MagicOnionTypeInfo("System", "Void", SubType.ValueType);
        public static MagicOnionTypeInfo System_String { get; } = new MagicOnionTypeInfo("System", "String");
        public static MagicOnionTypeInfo System_Boolean { get; } = new MagicOnionTypeInfo("System", "Boolean", SubType.ValueType);
        public static MagicOnionTypeInfo MessagePack_Nil { get; } = new MagicOnionTypeInfo("MessagePack", "Nil", SubType.ValueType);
        public static MagicOnionTypeInfo System_Threading_Tasks_Task { get; } = new MagicOnionTypeInfo("System.Threading.Tasks", "Task");
        public static MagicOnionTypeInfo System_Threading_Tasks_ValueTask { get; } = new MagicOnionTypeInfo("System.Threading.Tasks", "ValueTask", SubType.ValueType);
        public static MagicOnionTypeInfo MagicOnion_UnaryResult { get; } = new MagicOnionTypeInfo("MagicOnion", "UnaryResult", SubType.ValueType);
        // ReSharper restore InconsistentNaming
    }

    readonly SubType _subType;

    public string Namespace { get; }
    public string Name { get; }

    public IReadOnlyList<MagicOnionTypeInfo> GenericArguments { get; }
    public bool HasGenericArguments => GenericArguments.Any();

    public MagicOnionTypeInfo GetGenericTypeDefinition()
    {
        if (!HasGenericArguments) throw new InvalidOperationException("The type is not constructed generic type.");
        return MagicOnionTypeInfo.Create(Namespace, Name, Array.Empty<MagicOnionTypeInfo>(), IsValueType);
    }

    public bool IsArray => _subType == SubType.Array;
    public int ArrayRank { get; }
    public MagicOnionTypeInfo ElementType { get; }

    public string FullName
        => ToDisplayName(DisplayNameFormat.FullyQualified);
    public string FullNameOpenType
        => ToDisplayName(DisplayNameFormat.FullyQualified | DisplayNameFormat.OpenGenerics);

    public bool IsValueType => _subType == SubType.ValueType || _subType == SubType.Enum;

    public bool IsEnum => _subType == SubType.Enum;
    public MagicOnionTypeInfo UnderlyingType { get; }

    [Flags]
    public enum DisplayNameFormat
    {
        Short = 0,
        Global = 1,
        Namespace = 1 << 1,
        WithoutGenericArguments = 1 << 2,
        OpenGenerics = 1 << 3,
        FullyQualified = Namespace | Global,
    }

    private enum SubType
    {
        None,
        ValueType,
        Enum,
        Array,
    }

    private MagicOnionTypeInfo(string @namespace, string name, SubType subType = SubType.None, int arrayRank = 0, MagicOnionTypeInfo[] genericArguments = null, MagicOnionTypeInfo elementType = null, MagicOnionTypeInfo underlyingType = null)
    {
        _subType = subType;
        Namespace = @namespace;
        Name = name;
        GenericArguments = genericArguments ?? Array.Empty<MagicOnionTypeInfo>();
        ArrayRank = arrayRank;
        ElementType = elementType;
        UnderlyingType = underlyingType;
    }

    public string ToDisplayName(DisplayNameFormat format = DisplayNameFormat.Short)
        => IsArray
            ? $"{ElementType.ToDisplayName(format)}[{(ArrayRank > 1 ? new string(',', ArrayRank - 1) : "")}]"
            : $"{(format.HasFlag(DisplayNameFormat.Global) ? "global::" : "")}{(format.HasFlag(DisplayNameFormat.Namespace) && !string.IsNullOrWhiteSpace(Namespace) ? Namespace + "." : "")}{Name}{((!format.HasFlag(DisplayNameFormat.WithoutGenericArguments) || format.HasFlag(DisplayNameFormat.OpenGenerics)) && GenericArguments.Any() ? "<" + (format.HasFlag(DisplayNameFormat.OpenGenerics) ? new string(',', GenericArguments.Count - 1) : string.Join(", ", GenericArguments.Select(x => x.ToDisplayName(format)))) + ">" : "")}";

    public IEnumerable<MagicOnionTypeInfo> EnumerateDependentTypes(bool includesSelf = false)
    {
        if (includesSelf)
        {
            yield return this;
        }

        if (IsArray)
        {
            yield return ElementType;
            foreach (var t in ElementType.EnumerateDependentTypes())
            {
                yield return t;
            }
        }
        if (IsEnum)
        {
            //yield return UnderlyingType;
            //foreach (var t in UnderlyingType.EnumerateDependentTypes())
            //{
            //    yield return t;
            //}
        }
        foreach (var genericArgument in GenericArguments)
        {
            yield return genericArgument;
            foreach (var t in genericArgument.EnumerateDependentTypes())
            {
                yield return t;
            }
        }
    }

    public static MagicOnionTypeInfo Create(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
        => Create(@namespace, name, genericArguments, isValueType: false);

    public static MagicOnionTypeInfo CreateValueType(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
        => Create(@namespace, name, genericArguments, isValueType: true);

    public static MagicOnionTypeInfo Create(string @namespace, string name, MagicOnionTypeInfo[] genericArguments, bool isValueType)
    {
        if (@namespace == "MessagePack" && name == "Nil") return KnownTypes.MessagePack_Nil;
        if (@namespace == "System" && name == "String") return KnownTypes.System_String;
        if (@namespace == "System" && name == "Boolean") return KnownTypes.System_Boolean;
        if (@namespace == "System" && name == "Void") return KnownTypes.System_Void;
        if (@namespace == "System.Threading.Tasks" && name == "Task" && genericArguments.Length == 0) return KnownTypes.System_Threading_Tasks_Task;
        if (@namespace == "System.Threading.Tasks" && name == "ValueTask" && genericArguments.Length == 0) return KnownTypes.System_Threading_Tasks_ValueTask;
        if (@namespace == "MagicOnion" && name == "UnaryResult" && genericArguments.Length == 0) return KnownTypes.MagicOnion_UnaryResult;

        return new MagicOnionTypeInfo(@namespace, name, isValueType ? SubType.ValueType : SubType.None, arrayRank:0, genericArguments);
    }

    public static MagicOnionTypeInfo CreateArray(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
        => CreateArray(@namespace, name, genericArguments, 1);

    public static MagicOnionTypeInfo CreateArray(string @namespace, string name, MagicOnionTypeInfo[] genericArguments, int arrayRank)
        => CreateArray(Create(@namespace, name, genericArguments), arrayRank);

    public static MagicOnionTypeInfo CreateArray(MagicOnionTypeInfo elementType, int arrayRank = 1)
    {
        if (arrayRank < 1) throw new ArgumentOutOfRangeException(nameof(arrayRank));
        return new MagicOnionTypeInfo(elementType.Namespace, elementType.Name, SubType.Array, arrayRank: arrayRank, elementType: elementType);
    }

    public static MagicOnionTypeInfo CreateEnum(string @namespace, string name, MagicOnionTypeInfo underlyingType)
        => new MagicOnionTypeInfo(@namespace, name, SubType.Enum, arrayRank:0, genericArguments: Array.Empty<MagicOnionTypeInfo>(), underlyingType: underlyingType);

    public static MagicOnionTypeInfo CreateFromType<T>()
        => CreateFromType(typeof(T));
    public static MagicOnionTypeInfo CreateFromType(Type type)
    {
        if (type.IsArray)
        {
            return CreateArray(CreateFromType(type.GetElementType()), type.GetArrayRank());
        }
        else if (type.IsGenericType)
        {
            if (type.IsGenericTypeDefinition) throw new InvalidOperationException("The type must be constructed generic type.");
            return Create(type.Namespace, type.Name.Substring(0, type.Name.IndexOf('`')), type.GetGenericArguments().Select(x => CreateFromType(x)).ToArray(), type.IsValueType);
        }
        else if (type.IsEnum)
        {
            return CreateEnum(type.Namespace, type.Name, CreateFromType(type.GetEnumUnderlyingType()));
        }

        return Create(type.Namespace, type.Name, Array.Empty<MagicOnionTypeInfo>(), type.IsValueType);
    }

    public static MagicOnionTypeInfo CreateFromSymbol(ITypeSymbol symbol)
    {
        var isArray = false;
        var arrayRank = 0;

        var finalSymbol = symbol;
        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            finalSymbol = arrayTypeSymbol.ElementType;
            arrayRank = arrayTypeSymbol.Rank;
            isArray = true;
        }

        if (finalSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            var @namespace = finalSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : finalSymbol.ContainingNamespace.ToDisplayString();
            var name = finalSymbol.Name;
            var typeArguments = namedTypeSymbol.TypeArguments.Select(MagicOnionTypeInfo.CreateFromSymbol).ToArray();

            MagicOnionTypeInfo type;
            if (finalSymbol.TypeKind == TypeKind.Enum)
            {
                type =  CreateEnum(@namespace, name, CreateFromSymbol(namedTypeSymbol.EnumUnderlyingType));
            }
            else
            {
                type = Create(@namespace, name, typeArguments, finalSymbol.IsValueType);
            }

            return isArray ? CreateArray(type, arrayRank) : type;
        }
        else if (isArray && finalSymbol is IArrayTypeSymbol)
        {
            // T[][]
            // NOTE: MagicOnionTypeInfo has limited support for a jagged array.
            var jaggedCount = 0;
            while (finalSymbol is IArrayTypeSymbol jaggedArrayTypeSymbol)
            {
                finalSymbol = jaggedArrayTypeSymbol.ElementType;
                jaggedCount++;
            }

            var @namespace = finalSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : finalSymbol.ContainingNamespace.ToDisplayString();
            var name = finalSymbol.Name + string.Concat(Enumerable.Repeat("[]", jaggedCount));
            return CreateArray(@namespace, name, Array.Empty<MagicOnionTypeInfo>(), arrayRank);
        }
        else
        {
            throw new InvalidOperationException("The specified type symbol is unnamed type symbol. Generator cannot handle it.");
        }
    }

    public bool Equals(MagicOnionTypeInfo other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return FullName == other.FullName && /* Namespace + Name + GenericArguments + ArrayRank + ElementType */
               _subType == other._subType &&
               ElementType == other.ElementType &&
               UnderlyingType == other.UnderlyingType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MagicOnionTypeInfo)obj);
    }

    public static bool operator ==(MagicOnionTypeInfo a, MagicOnionTypeInfo b) => a?.Equals(b) ?? (b is null);
    public static bool operator !=(MagicOnionTypeInfo a, MagicOnionTypeInfo b) => !(a == b);

    public override int GetHashCode() => (FullName /* Namespace + Name + GenericArguments + ArrayRank + ElementType */, _subType, ElementType, UnderlyingType).GetHashCode();
}
