using System.Diagnostics;
using MagicOnion.Generator.Internal;
using static MagicOnion.Generator.CodeAnalysis.SerializationInfoCollector;

namespace MagicOnion.Generator.CodeAnalysis;

/// <summary>
/// Provides logic for gathering information to determine required formatters (for enums, collections and user-defined generic types).
/// </summary>
public class SerializationInfoCollector
{
    readonly IMagicOnionGeneratorLogger logger;
    readonly ISerializationFormatterNameMapper serializationFormatterNameMapper;

    public SerializationInfoCollector(IMagicOnionGeneratorLogger logger, ISerializationFormatterNameMapper serializationFormatterNameMapper)
    {
        this.logger = logger;
        this.serializationFormatterNameMapper = serializationFormatterNameMapper;
    }

    public MagicOnionSerializationInfoCollection Collect(MagicOnionServiceCollection serviceCollection)
        => Collect(EnumerateTypes(serviceCollection));

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

    public MagicOnionSerializationInfoCollection Collect(IEnumerable<TypeWithIfDirectives> types)
    {
        var mapper = serializationFormatterNameMapper;
        var context = new SerializationInfoCollectorContext();
        var flattened = types.SelectMany(x => x.Type.EnumerateDependentTypes(includesSelf: true).Select(y => new TypeWithIfDirectives(y, x.IfDirectives)));
        var proceeded = new HashSet<TypeWithIfDirectives>();

        foreach (var typeWithDirectives in flattened)
        {
            if (proceeded.Contains(typeWithDirectives)) continue;
            proceeded.Add(typeWithDirectives);

            var type = typeWithDirectives.Type;
            if (mapper.WellKnownTypes.BuiltInTypes.Contains(type.FullName))
            {
                logger.Trace($"[{nameof(SerializationInfoCollector)}] Found type '{type.FullName}'. Skip this because the type is supported by serializer built-in.");
                continue;
            }

            if (type.IsEnum)
            {
                Debug.Assert(type.UnderlyingType is not null);
                logger.Trace($"[{nameof(SerializationInfoCollector)}] Found Enum type '{type.FullName}'");
                context.Enums.Add(new EnumSerializationInfo(
                    type.Namespace,
                    type.Name,
                    type.FullName,
                    type.UnderlyingType!.Name,
                    typeWithDirectives.IfDirectives
                ));
            }
            else if (type.IsArray)
            {
                Debug.Assert(type.ElementType is not null);
                if (mapper.WellKnownTypes.BuiltInArrayElementTypes.Contains(type.ElementType!.FullName))
                {
                    logger.Trace($"[{nameof(SerializationInfoCollector)}] Array type '{type.FullName}'. Skip this because an array element type is supported by serializer built-in.");
                    continue;
                }

                logger.Trace($"[{nameof(SerializationInfoCollector)}] Array type '{type.FullName}'");

                var (formatterName, formatterConstructorArgs) = mapper.MapArray(type);
                context.Generics.Add(new GenericSerializationInfo(type.FullName, formatterName, formatterConstructorArgs, typeWithDirectives.IfDirectives));
                mapper.MapArray(type);
            }
            else if (type.HasGenericArguments)
            {
                if (type.FullNameOpenType == "global::System.Nullable<>" && mapper.WellKnownTypes.BuiltInNullableTypes.Contains(type.GenericArguments[0].FullName))
                {
                    logger.Trace($"[{nameof(SerializationInfoCollector)}] Generic type '{type.FullName}'. Skip this because it is nullable.");
                    continue;
                }

                if (mapper.TryMapGeneric(type, out var formatterName, out var formatterConstructorArgs))
                {
                    logger.Trace($"[{nameof(SerializationInfoCollector)}] Generic type '{type.FullName}' (IfDirectives={string.Join(", ", typeWithDirectives.IfDirectives)})");
                    context.Generics.Add(new GenericSerializationInfo(type.FullName, formatterName, formatterConstructorArgs, typeWithDirectives.IfDirectives));
                }
            }
        }

        return new MagicOnionSerializationInfoCollection(
            MergeResolverRegisterInfo(context.Enums),
            MergeResolverRegisterInfo(context.Generics),
            MergeSerializationTypeHintInfo(proceeded.Select(x => new SerializationTypeHintInfo(x.Type.FullName, x.IfDirectives)))
        );
    }

    static GenericSerializationInfo[] MergeResolverRegisterInfo(IEnumerable<GenericSerializationInfo> serializationInfoSet)
        => MergeResolverRegisterInfo(serializationInfoSet, (serializationInfo, serializationInfoCandidate) =>
            new GenericSerializationInfo(
                serializationInfo.FullName,
                serializationInfo.FormatterName,
                serializationInfo.FormatterConstructorArgs,
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

    static SerializationTypeHintInfo[] MergeSerializationTypeHintInfo(IEnumerable<SerializationTypeHintInfo> serializationInfoSet)
        => MergeResolverRegisterInfo(serializationInfoSet, (serializationInfo, serializationInfoCandidate) =>
            new SerializationTypeHintInfo(
                serializationInfo.FullName,
                serializationInfo.IfDirectiveConditions.Concat(serializationInfoCandidate.IfDirectiveConditions).ToArray()
            )
        );

    /// <summary>
    /// Merge `#if` directives by type of serialization target.
    /// </summary>
    /// <remarks>
    /// * Foo --> Foo<br />
    /// * Bar, Bar (#if CONST_1) --> Bar<br />
    /// * Baz (#if CONST_1), Baz (#if CONST_2) --> Baz (#if CONST_1 || CONST_2)<br />
    /// </remarks>
    static T[] MergeResolverRegisterInfo<T>(IEnumerable<T> serializationInfoSet, Func<T, T, T> mergeFunc)
        where T : ISerializationFormatterRegisterInfo
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
    public class TypeWithIfDirectives : IEquatable<TypeWithIfDirectives>
    {
        public MagicOnionTypeInfo Type { get; }
        public IReadOnlyList<string> IfDirectives { get; }

        public TypeWithIfDirectives(MagicOnionTypeInfo type, IReadOnlyList<string> ifDirectives)
        {
            Type = type;
            IfDirectives = ifDirectives;
        }

        public bool Equals(TypeWithIfDirectives other)
        {
            if (other is null) return false;
            return (other.Type == this.Type && Enumerable.SequenceEqual(other.IfDirectives, this.IfDirectives));
        }

        public override int GetHashCode()
        {
            return (Type, string.Join(";", IfDirectives)).GetHashCode();
        }
    }
}

public class MagicOnionSerializationInfoCollection
{
    public IReadOnlyList<EnumSerializationInfo> Enums { get; }
    public IReadOnlyList<GenericSerializationInfo> Generics { get; }
    public IReadOnlyList<ISerializationFormatterRegisterInfo> RequireRegistrationFormatters { get; }
    public IReadOnlyList<SerializationTypeHintInfo> TypeHints { get; }

    public MagicOnionSerializationInfoCollection(IReadOnlyList<EnumSerializationInfo> enums, IReadOnlyList<GenericSerializationInfo> generics, IReadOnlyList<SerializationTypeHintInfo> typeHints)
    {
        Enums = enums;
        Generics = generics;
        RequireRegistrationFormatters = generics.OrderBy(x => x.FullName).Cast<ISerializationFormatterRegisterInfo>().Concat(enums.OrderBy(x => x.FullName)).ToArray();
        TypeHints = typeHints.OrderBy(x => x.FullName).ToArray();
    }
}
