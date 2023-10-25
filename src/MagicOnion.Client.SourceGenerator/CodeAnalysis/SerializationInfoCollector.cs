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

    static IReadOnlyList<MagicOnionTypeInfo> EnumerateTypes(MagicOnionServiceCollection serviceCollection)
    {
        var types = new HashSet<MagicOnionTypeInfo>();

        foreach (var service in serviceCollection.Services)
        {
            foreach (var method in service.Methods)
            {
                EnumerateTypesCore(types, method.ResponseType);
                EnumerateTypesCore(types, method.RequestType);
            }
        }
        foreach (var hub in serviceCollection.Hubs)
        {
            foreach (var method in hub.Methods)
            {
                EnumerateTypesCore(types, method.ResponseType);
                EnumerateTypesCore(types, method.RequestType);
            }
            foreach (var method in hub.Receiver.Methods)
            {
                EnumerateTypesCore(types, method.ResponseType);
                EnumerateTypesCore(types, method.RequestType);
            }
        }

        return types.ToArray();

        static void EnumerateTypesCore(HashSet<MagicOnionTypeInfo> types, MagicOnionTypeInfo rootType)
        {
            types.Add(rootType);
            if (rootType.HasGenericArguments)
            {
                foreach (var genericTypeArg in rootType.GenericArguments)
                {
                    EnumerateTypesCore(types, genericTypeArg);
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
