using System.Diagnostics;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

/// <summary>
/// Provides logic for gathering information to determine required formatters (for enums, collections and user-defined generic types).
/// </summary>
public class SerializationInfoCollector
{
    readonly ISerializationFormatterNameMapper serializationFormatterNameMapper;

    public SerializationInfoCollector(ISerializationFormatterNameMapper serializationFormatterNameMapper)
    {
        this.serializationFormatterNameMapper = serializationFormatterNameMapper;
    }

    public MagicOnionSerializationInfoCollection Collect(MagicOnionServiceCollection serviceCollection)
        => Collect(EnumerateTypes(serviceCollection));

    static IEnumerable<MagicOnionTypeInfo> EnumerateTypes(MagicOnionServiceCollection serviceCollection)
    {
        return Enumerable.Concat(
            serviceCollection.Services.SelectMany(service =>
            {
                return service.Methods.SelectMany(method =>
                {
                    return Enumerable.Concat(
                        EnumerateTypes(method.ResponseType),
                        EnumerateTypes(method.RequestType)
                    );
                });
            }),
            serviceCollection.Hubs.SelectMany(hub =>
            {
                return Enumerable.Concat(
                    hub.Receiver.Methods.SelectMany(method =>
                    {
                        return Enumerable.Concat(
                            EnumerateTypes(method.ResponseType),
                            EnumerateTypes(method.RequestType)
                        );
                    }),
                    hub.Methods.SelectMany(method =>
                    {
                        return Enumerable.Concat(
                            EnumerateTypes(method.ResponseType),
                            EnumerateTypes(method.RequestType)
                        );
                    })
                );
            })
        );
    }
    static IEnumerable<MagicOnionTypeInfo> EnumerateTypes(MagicOnionTypeInfo type)
    {
        yield return type;

        if (type.HasGenericArguments)
        {
            foreach (var genericTypeArg in type.GenericArguments)
            {
                foreach (var t in EnumerateTypes(genericTypeArg))
                {
                    yield return t;
                }
            }
        }
    }

    public MagicOnionSerializationInfoCollection Collect(IEnumerable<MagicOnionTypeInfo> types)
    {
        var mapper = serializationFormatterNameMapper;
        var context = new SerializationInfoCollectorContext();
        var flattened = types.SelectMany(x => x.EnumerateDependentTypes(includesSelf: true));
        var proceeded = new HashSet<MagicOnionTypeInfo>();

        foreach (var type in flattened)
        {
            if (proceeded.Contains(type)) continue;
            proceeded.Add(type);

            if (mapper.WellKnownTypes.BuiltInTypes.Contains(type.FullName))
            {
                continue;
            }

            if (type.IsEnum)
            {
                Debug.Assert(type.UnderlyingType is not null);
                context.Enums.Add(new EnumSerializationInfo(
                    type.Namespace,
                    type.Name,
                    type.FullName,
                    type.UnderlyingType!.Name
                ));
            }
            else if (type.IsArray)
            {
                Debug.Assert(type.ElementType is not null);
                if (mapper.WellKnownTypes.BuiltInArrayElementTypes.Contains(type.ElementType!.FullName))
                {
                    continue;
                }


                var (formatterName, formatterConstructorArgs) = mapper.MapArray(type);
                context.Generics.Add(new GenericSerializationInfo(type.FullName, formatterName, formatterConstructorArgs));
                mapper.MapArray(type);
            }
            else if (type.HasGenericArguments)
            {
                if (type.FullNameOpenType == "global::System.Nullable<>" && mapper.WellKnownTypes.BuiltInNullableTypes.Contains(type.GenericArguments[0].FullName))
                {
                    continue;
                }

                if (mapper.TryMapGeneric(type, out var formatterName, out var formatterConstructorArgs))
                {
                    context.Generics.Add(new GenericSerializationInfo(type.FullName, formatterName, formatterConstructorArgs));
                }
            }
        }

        return new MagicOnionSerializationInfoCollection(
            context.Enums,
            context.Generics,
            proceeded.Select(x => new SerializationTypeHintInfo(x.FullName)).ToList()
        );
    }

    class SerializationInfoCollectorContext
    {
        public List<EnumSerializationInfo> Enums { get; } = new List<EnumSerializationInfo>();
        public List<GenericSerializationInfo> Generics { get; } = new List<GenericSerializationInfo>();
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
