using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

public interface ISerializationFormatterNameMapper
{
    IWellKnownSerializationTypes WellKnownTypes { get; }
    bool TryMapGeneric(MagicOnionTypeInfo type, [NotNullWhen(true)] out string? formatterName, [NotNullWhen(true)] out string? formatterConstructorArgs);
    (string FormatterName, string FormatterConstructorArgs) MapArray(MagicOnionTypeInfo type);
}


public interface IWellKnownSerializationTypes
{
    IReadOnlyDictionary<string, string> GenericFormattersMap { get; }
    HashSet<string> BuiltInTypes { get; }
    HashSet<string> BuiltInNullableTypes { get; }
    HashSet<string> BuiltInArrayElementTypes { get; }
}

public class MessagePackFormatterNameMapper : ISerializationFormatterNameMapper
{
    readonly string userDefinedFormatterNamespace;
    readonly bool allowToMapUserDefinedFormatter;

    public IWellKnownSerializationTypes WellKnownTypes => MessagePackWellKnownSerializationTypes.Instance;

    public MessagePackFormatterNameMapper(string userDefinedFormatterNamespace, bool allowToMapUserDefinedFormatter)
    {
        userDefinedFormatterNamespace = string.IsNullOrWhiteSpace(userDefinedFormatterNamespace) ? "MessagePack.Formatters" : userDefinedFormatterNamespace;
        if (!userDefinedFormatterNamespace.StartsWith("global::")) userDefinedFormatterNamespace = "global::" + userDefinedFormatterNamespace;

        this.userDefinedFormatterNamespace = userDefinedFormatterNamespace;
        this.allowToMapUserDefinedFormatter = allowToMapUserDefinedFormatter;
    }

    public bool TryMapGeneric(MagicOnionTypeInfo type, [NotNullWhen(true)] out string? formatterName, [NotNullWhen(true)] out string? formatterConstructorArgs)
    {
        formatterName = null;
        formatterConstructorArgs = null;

        var genericTypeArgs = string.Join(", ", type.GenericArguments.Select(x => x.FullName));
        if (type is { Namespace: "MagicOnion", Name: "DynamicArgumentTuple" })
        {
            // MagicOnion.DynamicArgumentTuple
            var ctorArguments = string.Join(", ", type.GenericArguments.Select(x => $"default({x.FullName})"));
            formatterName = $"global::MagicOnion.Serialization.MessagePack.DynamicArgumentTupleFormatter<{genericTypeArgs}>";
            formatterConstructorArgs = $"({ctorArguments})";
        }
        else if (MessagePackWellKnownSerializationTypes.Instance.GenericFormattersMap.TryGetValue(type.FullNameOpenType, out var mappedFormatterName))
        {
            // Well-known generic types (Nullable<T>, IList<T>, List<T>, Dictionary<TKey, TValue> ...)
            formatterName = $"{mappedFormatterName}<{genericTypeArgs}>";
            formatterConstructorArgs = "()";
        }
        else if (allowToMapUserDefinedFormatter)
        {
            // User-defined generic types
            formatterName = $"{userDefinedFormatterNamespace}{(string.IsNullOrWhiteSpace(userDefinedFormatterNamespace) ? "" : ".")}{type.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace | MagicOnionTypeInfo.DisplayNameFormat.WithoutGenericArguments)}Formatter<{genericTypeArgs}>";
            formatterConstructorArgs = "()";
        }

        return formatterName != null;
    }

    public (string FormatterName, string FormatterConstructorArgs) MapArray(MagicOnionTypeInfo type)
    {
        Debug.Assert(type.ElementType is not null);
        return type.ArrayRank switch
        {
            1 => ($"global::MessagePack.Formatters.ArrayFormatter<{type.ElementType!.FullName}>", "()"),
            2 => ($"global::MessagePack.Formatters.TwoDimensionalArrayFormatter<{type.ElementType!.FullName}>", "()"),
            3 => ($"global::MessagePack.Formatters.ThreeDimensionalArrayFormatter<{type.ElementType!.FullName}>", "()"),
            4 => ($"global::MessagePack.Formatters.FourDimensionalArrayFormatter<{type.ElementType!.FullName}>", "()"),
            _ => throw new IndexOutOfRangeException($"An array of rank must be less than 5. ({type.FullName})"),
        };
    }

    class MessagePackWellKnownSerializationTypes : IWellKnownSerializationTypes
    {
        MessagePackWellKnownSerializationTypes() {}
        public static IWellKnownSerializationTypes Instance { get; } = new MessagePackWellKnownSerializationTypes();

        public IReadOnlyDictionary<string, string> GenericFormattersMap { get; } = new Dictionary<string, string>
        {
            {"global::System.Nullable<>", "global::MessagePack.Formatters.NullableFormatter" },
            {"global::System.Collections.Generic.List<>", "global::MessagePack.Formatters.ListFormatter" },
            {"global::System.Collections.Generic.IList<>", "global::MessagePack.Formatters.InterfaceListFormatter2" },
            {"global::System.Collections.Generic.IReadOnlyList<>", "global::MessagePack.Formatters.InterfaceReadOnlyListFormatter" },
            {"global::System.Collections.Generic.Dictionary<,>", "global::MessagePack.Formatters.DictionaryFormatter"},
            {"global::System.Collections.Generic.IDictionary<,>", "global::MessagePack.Formatters.InterfaceDictionaryFormatter"},
            {"global::System.Collections.Generic.IReadOnlyDictionary<,>", "global::MessagePack.Formatters.InterfaceReadOnlyDictionaryFormatter"},
            {"global::System.Collections.Generic.ICollection<>", "global::MessagePack.Formatters.InterfaceCollectionFormatter2" },
            {"global::System.Collections.Generic.IReadOnlyCollection<>", "global::MessagePack.Formatters.InterfaceReadOnlyCollectionFormatter" },
            {"global::System.Collections.Generic.IEnumerable<>", "global::MessagePack.Formatters.InterfaceEnumerableFormatter" },
            {"global::System.Collections.Generic.KeyValuePair<,>", "global::MessagePack.Formatters.KeyValuePairFormatter" },
            {"global::System.Linq.ILookup<,>", "global::MessagePack.Formatters.InterfaceLookupFormatter" },
            {"global::System.Linq.IGrouping<,>", "global::MessagePack.Formatters.InterfaceGroupingFormatter" },
            {"global::System.Tuple<>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,,,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,,,,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,,,,,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,,,,,,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.Tuple<,,,,,,,>", "global::MessagePack.Formatters.TupleFormatter" },
            {"global::System.ValueTuple<>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,,,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,,,,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,,,,,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,,,,,,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
            {"global::System.ValueTuple<,,,,,,,>", "global::MessagePack.Formatters.ValueTupleFormatter" },
        };

        public HashSet<string> BuiltInTypes { get; } = new HashSet<string>(new string[]
        {
            "global::System.ArraySegment<global::System.Byte>",
        });

        public HashSet<string> BuiltInNullableTypes { get; } = new HashSet<string>(new string[]
        {
            // Nullable
            // https://github.com/neuecc/MessagePack-CSharp/blob/master/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Resolvers/BuiltinResolver.cs#L73
            "global::System.ArraySegment<global::System.Byte>",
            "global::System.Int16",
            "global::System.Int32",
            "global::System.Int64",
            "global::System.UInt16",
            "global::System.UInt32",
            "global::System.UInt64",
            "global::System.Single",
            "global::System.Double",
            "global::System.Boolean",
            "global::System.Byte",
            "global::System.SByte",
            "global::System.DateTime",
            "global::System.Char",

            "global::System.Decimal",
            "global::System.TimeSpan",
            "global::System.DateTimeOffset",
            "global::System.Guid",
        });

        public HashSet<string> BuiltInArrayElementTypes { get; } = new HashSet<string>(new string[]
        {
            "global::System.Int16",
            "global::System.Int32",
            "global::System.Int64",
            "global::System.UInt16",
            "global::System.UInt32",
            "global::System.UInt64",
            "global::System.Single",
            "global::System.Double",
            "global::System.Boolean",
            "global::System.Byte",
            "global::System.SByte",
            "global::System.DateTime",
            "global::System.Char",

            "global::System.Decimal",
            "global::System.String",
            "global::System.Guid",
            "global::System.TimeSpan",

            "global::MessagePack.Nil",

            // Unity extensions
            "global::UnityEngine.Vector2",
            "global::UnityEngine.Vector3",
            "global::UnityEngine.Vector4",
            "global::UnityEngine.Quaternion",
            "global::UnityEngine.Color",
            "global::UnityEngine.Bounds",
            "global::UnityEngine.Rect",

            "global::System.Reactive.Unit",
        });
    }
}


public class MemoryPackFormatterNameMapper : ISerializationFormatterNameMapper
{
    public IWellKnownSerializationTypes WellKnownTypes => MessagePackWellKnownSerializationTypes.Instance;

    public bool TryMapGeneric(MagicOnionTypeInfo type, [NotNullWhen(true)] out string? formatterName, [NotNullWhen(true)] out string? formatterConstructorArgs)
    {
        formatterName = null;
        formatterConstructorArgs = null;

        var genericTypeArgs = string.Join(", ", type.GenericArguments.Select(x => x.FullName));
        if (type is { Namespace: "MagicOnion", Name: "DynamicArgumentTuple" })
        {
            // MagicOnion.DynamicArgumentTuple
            var ctorArguments = string.Join(", ", type.GenericArguments.Select(x => $"default({x.FullName})"));
            formatterName = $"global::MagicOnion.Serialization.MemoryPack.DynamicArgumentTupleFormatter<{genericTypeArgs}>";
            formatterConstructorArgs = "()";
        }
        else if (MessagePackWellKnownSerializationTypes.Instance.GenericFormattersMap.TryGetValue(type.FullNameOpenType, out var mappedFormatterName))
        {
            // Well-known generic types (Nullable<T>, IList<T>, List<T>, Dictionary<TKey, TValue> ...)
            formatterName = $"{mappedFormatterName}<{genericTypeArgs}>";
            formatterConstructorArgs = "()";
        }

        return formatterName != null;
    }

    public (string FormatterName, string FormatterConstructorArgs) MapArray(MagicOnionTypeInfo type)
    {
        Debug.Assert(type.ElementType is not null);
        return type.ArrayRank switch
        {
            1 => ($"global::MemoryPack.Formatters.ArrayFormatter<{type.ElementType!.FullName}>", "()"),
            2 => ($"global::MemoryPack.Formatters.TwoDimensionalArrayFormatter<{type.ElementType!.FullName}>", "()"),
            3 => ($"global::MemoryPack.Formatters.ThreeDimensionalArrayFormatter<{type.ElementType!.FullName}>", "()"),
            4 => ($"global::MemoryPack.Formatters.FourDimensionalArrayFormatter<{type.ElementType!.FullName}>", "()"),
            _ => throw new IndexOutOfRangeException($"An array of rank must be less than 5. ({type.FullName})"),
        };
    }

    class MessagePackWellKnownSerializationTypes : IWellKnownSerializationTypes
    {
        MessagePackWellKnownSerializationTypes() {}
        public static IWellKnownSerializationTypes Instance { get; } = new MessagePackWellKnownSerializationTypes();

        public IReadOnlyDictionary<string, string> GenericFormattersMap { get; } = new Dictionary<string, string>
        {
            {"global::System.Nullable<>", "global::MemoryPack.Formatters.NullableFormatter" },

            // https://github.com/Cysharp/MemoryPack/blob/2eb8e42861d105f6d8a8ba7638eb119972d5c630/src/MemoryPack.Generator/ReferenceSymbols.cs#L105
            // ArrayFormatters
            { "global::System.ArraySegment<>", "global::MemoryPack.Formatters.ArraySegmentFormatter" },
            { "global::System.Memory<>", "global::MemoryPack.Formatters.MemoryFormatter" },
            { "global::System.ReadOnlyMemory<>", "global::MemoryPack.Formatters.ReadOnlyMemoryFormatter" },
            { "global::System.Buffers.ReadOnlySequence<>", "global::MemoryPack.Formatters.ReadOnlySequenceFormatter" },

            // CollectionFormatters
            { "global::System.Collections.Generic.List<>", "global::MemoryPack.Formatters.ListFormatter" },
            { "global::System.Collections.Generic.Stack<>", "global::MemoryPack.Formatters.StackFormatter" },
            { "global::System.Collections.Generic.Queue<>", "global::MemoryPack.Formatters.QueueFormatter" },
            { "global::System.Collections.Generic.LinkedList<>", "global::MemoryPack.Formatters.LinkedListFormatter" },
            { "global::System.Collections.Generic.HashSet<>", "global::MemoryPack.Formatters.HashSetFormatter" },
            { "global::System.Collections.Generic.SortedSet<>", "global::MemoryPack.Formatters.SortedSetFormatter" },
            { "global::System.Collections.Generic.PriorityQueue<,>", "global::MemoryPack.Formatters.PriorityQueueFormatter" },
            { "global::System.Collections.ObjectModel.ObservableCollection<>", "global::MemoryPack.Formatters.ObservableCollectionFormatter" },
            { "global::System.Collections.ObjectModel.Collection<>", "global::MemoryPack.Formatters.CollectionFormatter" },
            { "global::System.Collections.Concurrent.ConcurrentQueue<>", "global::MemoryPack.Formatters.ConcurrentQueueFormatter" },
            { "global::System.Collections.Concurrent.ConcurrentStack<>", "global::MemoryPack.Formatters.ConcurrentStackFormatter" },
            { "global::System.Collections.Concurrent.ConcurrentBag<>", "global::MemoryPack.Formatters.ConcurrentBagFormatter" },
            { "global::System.Collections.Generic.Dictionary<,>", "global::MemoryPack.Formatters.DictionaryFormatter" },
            { "global::System.Collections.Generic.SortedDictionary<,>", "global::MemoryPack.Formatters.SortedDictionaryFormatter" },
            { "global::System.Collections.Generic.SortedList<,>", "global::MemoryPack.Formatters.SortedListFormatter" },
            { "global::System.Collections.Concurrent.ConcurrentDictionary<,>", "global::MemoryPack.Formatters.ConcurrentDictionaryFormatter" },
            { "global::System.Collections.ObjectModel.ReadOnlyCollection<>", "global::MemoryPack.Formatters.ReadOnlyCollectionFormatter" },
            { "global::System.Collections.ObjectModel.ReadOnlyObservableCollection<>", "global::MemoryPack.Formatters.ReadOnlyObservableCollectionFormatter" },
            { "global::System.Collections.Concurrent.BlockingCollection<>", "global::MemoryPack.Formatters.BlockingCollectionFormatter" },

            // ImmutableCollectionFormatters
            { "global::System.Collections.Immutable.ImmutableArray<>", "global::MemoryPack.Formatters.ImmutableArrayFormatter" },
            { "global::System.Collections.Immutable.ImmutableList<>", "global::MemoryPack.Formatters.ImmutableListFormatter" },
            { "global::System.Collections.Immutable.ImmutableQueue<>", "global::MemoryPack.Formatters.ImmutableQueueFormatter" },
            { "global::System.Collections.Immutable.ImmutableStack<>", "global::MemoryPack.Formatters.ImmutableStackFormatter" },
            { "global::System.Collections.Immutable.ImmutableDictionary<,>", "global::MemoryPack.Formatters.ImmutableDictionaryFormatter" },
            { "global::System.Collections.Immutable.ImmutableSortedDictionary<,>", "global::MemoryPack.Formatters.ImmutableSortedDictionaryFormatter" },
            { "global::System.Collections.Immutable.ImmutableSortedSet<>", "global::MemoryPack.Formatters.ImmutableSortedSetFormatter" },
            { "global::System.Collections.Immutable.ImmutableHashSet<>", "global::MemoryPack.Formatters.ImmutableHashSetFormatter" },
            { "global::System.Collections.Immutable.IImmutableList<>", "global::MemoryPack.Formatters.InterfaceImmutableListFormatter" },
            { "global::System.Collections.Immutable.IImmutableQueue<>", "global::MemoryPack.Formatters.InterfaceImmutableQueueFormatter" },
            { "global::System.Collections.Immutable.IImmutableStack<>", "global::MemoryPack.Formatters.InterfaceImmutableStackFormatter" },
            { "global::System.Collections.Immutable.IImmutableDictionary<,>", "global::MemoryPack.Formatters.InterfaceImmutableDictionaryFormatter" },
            { "global::System.Collections.Immutable.IImmutableSet<>", "global::MemoryPack.Formatters.InterfaceImmutableSetFormatter" },

            // InterfaceCollectionFormatters
            { "global::System.Collections.Generic.IEnumerable<>", "global::MemoryPack.Formatters.InterfaceEnumerableFormatter" },
            { "global::System.Collections.Generic.ICollection<>", "global::MemoryPack.Formatters.InterfaceCollectionFormatter" },
            { "global::System.Collections.Generic.IReadOnlyCollection<>", "global::MemoryPack.Formatters.InterfaceReadOnlyCollectionFormatter" },
            { "global::System.Collections.Generic.IList<>", "global::MemoryPack.Formatters.InterfaceListFormatter" },
            { "global::System.Collections.Generic.IReadOnlyList<>", "global::MemoryPack.Formatters.InterfaceReadOnlyListFormatter" },
            { "global::System.Collections.Generic.IDictionary<,>", "global::MemoryPack.Formatters.InterfaceDictionaryFormatter" },
            { "global::System.Collections.Generic.IReadOnlyDictionary<,>", "global::MemoryPack.Formatters.InterfaceReadOnlyDictionaryFormatter" },
            { "global::System.Linq.ILookup<,>", "global::MemoryPack.Formatters.InterfaceLookupFormatter" },
            { "global::System.Linq.IGrouping<,>", "global::MemoryPack.Formatters.InterfaceGroupingFormatter" },
            { "global::System.Collections.Generic.ISet<>", "global::MemoryPack.Formatters.InterfaceSetFormatter" },
            { "global::System.Collections.Generic.IReadOnlySet<>", "global::MemoryPack.Formatters.InterfaceReadOnlySetFormatter" },

            { "global::System.Collections.Generic.KeyValuePair<,>", "global::MemoryPack.Formatters.KeyValuePairFormatter" },
            { "global::System.Lazy<>", "global::MemoryPack.Formatters.LazyFormatter" },
            
            // TupleFormatters
            { "global::System.Tuple<>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,,,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,,,,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,,,,,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,,,,,,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.Tuple<,,,,,,,>", "global::MemoryPack.Formatters.TupleFormatter" },
            { "global::System.ValueTuple<>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,,,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
            { "global::System.ValueTuple<,,,,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter" },
        };

        public HashSet<string> BuiltInTypes { get; } = new HashSet<string>(new string[]
        {
            // Nullable
            // https://github.com/Cysharp/MemoryPack/blob/4120788fdf8f4fa6b258686f9b274a11be852899/src/MemoryPack.Core/MemoryPackFormatterProvider.WellknownTypes.tt#L9
            "global::System.SByte",
            "global::System.Byte",
            "global::System.Int16",
            "global::System.UInt16",
            "global::System.Int32",
            "global::System.UInt32",
            "global::System.Int64",
            "global::System.UInt64",
            "global::System.Char",
            "global::System.Single",
            "global::System.Double",
            "global::System.Decimal",
            "global::System.Boolean",
            "global::System.IntPtr",
            "global::System.UIntPtr",

            "global::System.DateTime",
            "global::System.DateTimeOffset",
            "global::System.TimeSpan",
            "global::System.Guid",

            // System.Numerics
            "global::System.Numerics.Complex",
            "global::System.Numerics.Plane",
            "global::System.Numerics.Quaternion",
            "global::System.Numerics.Matrix3x2",
            "global::System.Numerics.Matrix4x4",
            "global::System.Numerics.Vector2",
            "global::System.Numerics.Vector3",
            "global::System.Numerics.Vector4",

            // .NET 7
            "global::System.Text.Rune",
            "global::System.DateOnly",
            "global::System.TimeOnly",
            "global::System.Half",
            "global::System.Int128",
            "global::System.UInt128",

            // .NET common
            "global::System.String",
            "global::System.Version",
            "global::System.Uri",

            // .NET common
            "global::System.TimeZoneInfo",
            "global::System.BigInteger",
            "global::System.Collections.BitArray",
            "global::System.Text.StringBuilder",
            "global::System.Type",

            // Unity extensions
            // https://github.com/Cysharp/MemoryPack/blob/4120788fdf8f4fa6b258686f9b274a11be852899/src/MemoryPack.Unity/Assets/Plugins/MemoryPack/Runtime/MemoryPack.Unity/ProviderInitializer.cs
            "global::UnityEngine.Vector2",
            "global::UnityEngine.Vector3",
            "global::UnityEngine.Vector4",
            "global::UnityEngine.Vector2Int",
            "global::UnityEngine.Vector3Int",
            "global::UnityEngine.Bounds",
            "global::UnityEngine.BoundsInt",
            "global::UnityEngine.Range",
            "global::UnityEngine.RangeInt",
            "global::UnityEngine.Rect",
            "global::UnityEngine.RectInt",
        });

        public HashSet<string> BuiltInNullableTypes => BuiltInTypes;

        public HashSet<string> BuiltInArrayElementTypes => BuiltInTypes;
    }
}
