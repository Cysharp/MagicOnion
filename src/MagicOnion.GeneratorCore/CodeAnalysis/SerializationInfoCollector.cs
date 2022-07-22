using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MagicOnion.Generator.CodeAnalysis
{
    /// <summary>
    /// Provides logic for gathering information to determine required formatters (for enums, collections and user-defined generic types).
    /// </summary>
    public class SerializationInfoCollector
    {
        public MagicOnionSerializationInfoCollection Collect(MagicOnionServiceCollection serviceCollection, string userDefinedMessagePackFormattersNamespace = null)
            => Collect(EnumerateTypes(serviceCollection), userDefinedMessagePackFormattersNamespace);

        static IEnumerable<TypeWithIfDirectives> EnumerateTypes(MagicOnionServiceCollection serviceCollection)
        {
            return Enumerable.Concat(
                serviceCollection.Services.SelectMany(service =>
                {
                    return service.Methods.SelectMany(method =>
                    {
                        var ifDirectives = new[] { service.IfDirectiveCondition, method.IfDirectiveCondition }.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                        return Enumerable.Concat(
                            EnumerateTypes(method.ResponseType, ifDirectives),
                            EnumerateTypes(method.RequestType, ifDirectives)
                        );
                    });
                }),
                serviceCollection.Hubs.SelectMany(hub =>
                {
                    return Enumerable.Concat(
                        hub.Receiver.Methods.SelectMany(method =>
                        {
                            var ifDirectives = new[] { hub.IfDirectiveCondition, method.IfDirectiveCondition }.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                            return Enumerable.Concat(
                                EnumerateTypes(method.ResponseType, ifDirectives),
                                EnumerateTypes(method.RequestType, ifDirectives)
                            );
                        }),
                        hub.Methods.SelectMany(method =>
                        {
                            var ifDirectives = new[] { hub.IfDirectiveCondition, method.IfDirectiveCondition }.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                            return Enumerable.Concat(
                                EnumerateTypes(method.ResponseType, ifDirectives),
                                EnumerateTypes(method.RequestType, ifDirectives)
                            );
                        })
                    );
                })
            );
        }
        static IEnumerable<TypeWithIfDirectives> EnumerateTypes(MagicOnionTypeInfo type, IReadOnlyList<string> ifDirectives)
        {
            yield return new TypeWithIfDirectives(type, ifDirectives);

            if (type.HasGenericArguments)
            {
                foreach (var genericTypeArg in type.GenericArguments)
                {
                    foreach (var t in EnumerateTypes(genericTypeArg, ifDirectives))
                    {
                        yield return t;
                    }
                }
            }
        }

        public MagicOnionSerializationInfoCollection Collect(IEnumerable<TypeWithIfDirectives> types, string userDefinedMessagePackFormattersNamespace = null)
        {
            var context = new SerializationInfoCollectorContext();
            var flattened = types.SelectMany(x => x.Type.EnumerateDependentTypes(includesSelf: true).Select(y => new TypeWithIfDirectives(y, x.IfDirectives)));

            userDefinedMessagePackFormattersNamespace = userDefinedMessagePackFormattersNamespace ?? "MessagePack.Formatters";
            if (!userDefinedMessagePackFormattersNamespace.StartsWith("global::")) userDefinedMessagePackFormattersNamespace = "global::" + userDefinedMessagePackFormattersNamespace;

            foreach (var typeWithDirectives in flattened)
            {
                var type = typeWithDirectives.Type;
                if (WellKnownSerializationTypes.BuiltInTypes.Contains(type.FullName))
                {
                    continue;
                }

                if (type.IsEnum)
                {
                    context.Enums.Add(new EnumSerializationInfo(
                        type.Namespace,
                        type.Name,
                        type.FullName,
                        type.UnderlyingType.Name,
                        typeWithDirectives.IfDirectives
                    ));
                }
                else if (type.IsArray)
                {
                    if (WellKnownSerializationTypes.BuiltInArrayElementTypes.Contains(type.ElementType.FullName))
                    {
                        continue;
                    }

                    switch (type.ArrayRank)
                    {
                        case 1:
                            context.Generics.Add(new GenericSerializationInfo(type.FullName, $"global::MessagePack.Formatters.ArrayFormatter<{type.ElementType.FullName}>()", typeWithDirectives.IfDirectives));
                            break;
                        case 2:
                            context.Generics.Add(new GenericSerializationInfo(type.FullName, $"global::MessagePack.Formatters.TwoDimensionalArrayFormatter<{type.ElementType.FullName}>()", typeWithDirectives.IfDirectives));
                            break;
                        case 3:
                            context.Generics.Add(new GenericSerializationInfo(type.FullName, $"global::MessagePack.Formatters.ThreeDimensionalArrayFormatter<{type.ElementType.FullName}>()", typeWithDirectives.IfDirectives));
                            break;
                        case 4:
                            context.Generics.Add(new GenericSerializationInfo(type.FullName, $"global::MessagePack.Formatters.FourDimensionalArrayFormatter<{type.ElementType.FullName}>()", typeWithDirectives.IfDirectives));
                            break;
                        default:
                            throw new IndexOutOfRangeException($"An array of rank must be less than 5. ({type.FullName})");
                    }
                }
                else if (type.HasGenericArguments)
                {
                    if (type.FullNameOpenType == "global::System.Nullable<>" && WellKnownSerializationTypes.BuiltInNullableTypes.Contains(type.GenericArguments[0].FullName))
                    {
                        continue;
                    }

                    var genericTypeArgs = string.Join(", ", type.GenericArguments.Select(x => x.FullName));

                    string formatterName;
                    if (type.Namespace == "MagicOnion" && type.Name == "DynamicArgumentTuple")
                    {
                        // MagicOnion.DynamicArgumentTuple
                        var ctorArguments = string.Join(", ", type.GenericArguments.Select(x => $"default({x.FullName})"));
                        formatterName = $"global::MagicOnion.DynamicArgumentTupleFormatter<{genericTypeArgs}>({ctorArguments})";
                    }
                    else if (WellKnownSerializationTypes.GenericFormattersMap.TryGetValue(type.FullNameOpenType, out var mappedFormatterName))
                    {
                        // Well-known generic types (Nullable<T>, IList<T>, List<T>, Dictionary<TKey, TValue> ...)
                        formatterName = $"{mappedFormatterName}<{genericTypeArgs}>()";
                    }
                    else
                    {
                        // User-defined generic types
                        formatterName = $"{userDefinedMessagePackFormattersNamespace}{(string.IsNullOrWhiteSpace(userDefinedMessagePackFormattersNamespace) ? "" : ".")}{type.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace | MagicOnionTypeInfo.DisplayNameFormat.WithoutGenericArguments)}Formatter<{genericTypeArgs}>()";
                    }

                    context.Generics.Add(new GenericSerializationInfo(type.FullName, formatterName, typeWithDirectives.IfDirectives));
                }
            }

            return new MagicOnionSerializationInfoCollection(MergeResolverRegisterInfo(context.Enums), MergeResolverRegisterInfo(context.Generics));
        }

        static GenericSerializationInfo[] MergeResolverRegisterInfo(IEnumerable<GenericSerializationInfo> serializationInfoSet)
            => MergeResolverRegisterInfo(serializationInfoSet, (serializationInfo, serializationInfoCandidate) =>
                new GenericSerializationInfo(
                    serializationInfo.FullName,
                    serializationInfo.FormatterName,
                    serializationInfo.IfDirectiveConditions.Concat(serializationInfoCandidate.IfDirectiveConditions).ToArray()
                )
            );

        static EnumSerializationInfo[] MergeResolverRegisterInfo(IEnumerable<EnumSerializationInfo> serializationInfoSet)
            => MergeResolverRegisterInfo(serializationInfoSet, (serializationInfo, serializationInfoCandidate) =>
                new EnumSerializationInfo(
                    serializationInfo.Namespace,
                    serializationInfo.Name,
                    serializationInfo.FullName,
                    serializationInfo.UnderlyingType,
                    serializationInfo.IfDirectiveConditions.Concat(serializationInfoCandidate.IfDirectiveConditions).ToArray()
                )
            );

        static T[] MergeResolverRegisterInfo<T>(IEnumerable<T> serializationInfoSet, Func<T, T, T> mergeFunc)
            where T : IMessagePackFormatterResolverRegisterInfo
        {
            // The priority of the generation depends on the `#if` directive
            // If a serialization info has no `#if` conditions, we always use it. If there is more than one with the condition, it is merged.
            var candidates = new Dictionary<string, T>();
            foreach (var serializationInfo in serializationInfoSet)
            {
                if (serializationInfo.HasIfDirectiveConditions && candidates.TryGetValue(serializationInfo.FullName, out var serializationInfoCandidate))
                {
                    if (!serializationInfoCandidate.HasIfDirectiveConditions)
                    {
                        // If the candidate serialization info has no `#if` conditions, we keep to use it.
                        continue;
                    }

                    // Merge `IfDirectiveConditions`
                    candidates[serializationInfo.FullName] = mergeFunc(serializationInfo, serializationInfoCandidate);
                }
                else
                {
                    // The serialization info has no `#if` conditions, or is found first.
                    candidates[serializationInfo.FullName] = serializationInfo;
                }
            }

            return candidates.Values.ToArray();
        }


        class SerializationInfoCollectorContext
        {
            public List<EnumSerializationInfo> Enums { get; } = new List<EnumSerializationInfo>();
            public List<GenericSerializationInfo> Generics { get; } = new List<GenericSerializationInfo>();
        }

        [DebuggerDisplay("{Type,nq}")]
        public class TypeWithIfDirectives
        {
            public MagicOnionTypeInfo Type { get; }
            public IReadOnlyList<string> IfDirectives { get; }

            public TypeWithIfDirectives(MagicOnionTypeInfo type, IReadOnlyList<string> ifDirectives)
            {
                Type = type;
                IfDirectives = ifDirectives;
            }
        }
    }

    public class MagicOnionSerializationInfoCollection
    {
        public IReadOnlyList<EnumSerializationInfo> Enums { get; }
        public IReadOnlyList<GenericSerializationInfo> Generics { get; }
        public IReadOnlyList<IMessagePackFormatterResolverRegisterInfo> RequireRegistrationFormatters { get; }

        public MagicOnionSerializationInfoCollection(IReadOnlyList<EnumSerializationInfo> enums, IReadOnlyList<GenericSerializationInfo> generics)
        {
            Enums = enums;
            Generics = generics;
            RequireRegistrationFormatters = generics.OrderBy(x => x.FullName).Cast<IMessagePackFormatterResolverRegisterInfo>().Concat(enums.OrderBy(x => x.FullName)).ToArray();
        }
    }

    public static class WellKnownSerializationTypes
    {
        public static readonly IReadOnlyDictionary<string, string> GenericFormattersMap = new Dictionary<string, string>
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
            {"global::System.Linq.ILookup<,>", "global::MessagePack.Formatters.InterfaceLookupFormatter" },
            {"global::System.Linq.IGrouping<,>", "global::MessagePack.Formatters.InterfaceGroupingFormatter" },
        };

        public static readonly HashSet<string> BuiltInTypes = new HashSet<string>(new string[]
        {
            "global::System.ArraySegment<global::System.Byte>",
        });

        public static readonly HashSet<string> BuiltInNullableTypes = new HashSet<string>(new string[]
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

        public static readonly HashSet<string> BuiltInArrayElementTypes = new HashSet<string>(new string[]
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
