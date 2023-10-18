using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.CodeGen;
using MagicOnion.Client.SourceGenerator.CodeGen.Extensions;

namespace MagicOnion.Client.SourceGenerator;

public static class MagicOnionClientGenerator
{
    public static IReadOnlyList<(string Path, string Source)> Generate(GenerationContext context, MagicOnionServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        var outputs = new List<(string Path, string Source)>();

        var options = GeneratorOptions.Default;

        // <Namespace>.
        var namespaceDot = string.IsNullOrWhiteSpace(options.Namespace) ? string.Empty : options.Namespace + ".";
        // <Namespace>.Formatters
        var formattersNamespace = namespaceDot + "Formatters";

        // Configure serialization
        (ISerializationFormatterNameMapper Mapper, string Namespace, ISerializerFormatterGenerator Generator, Func<IEnumerable<EnumSerializationInfo>, string> EnumFormatterGenerator)
            serialization = options.Serializer switch
        {
            GeneratorOptions.SerializerType.MemoryPack => (
                Mapper: new MemoryPackFormatterNameMapper(),
                Namespace: options.Namespace,
                Generator: new MemoryPackFormatterRegistrationGenerator(),
                EnumFormatterGenerator: _ => string.Empty
            ),
            GeneratorOptions.SerializerType.MessagePack => (
                Mapper: new MessagePackFormatterNameMapper(options.MessagePackFormatterNamespace),
                Namespace: namespaceDot + "Resolvers",
                Generator: new MessagePackFormatterResolverGenerator(),
                EnumFormatterGenerator: x => MessagePackEnumFormatterGenerator.Build(context, x)
            ),
            _ => throw new NotImplementedException(),
        };

        cancellationToken.ThrowIfCancellationRequested();

        var serializationInfoCollector = new SerializationInfoCollector(serialization.Mapper);
        var serializationInfoCollection = serializationInfoCollector.Collect(serviceCollection);

        cancellationToken.ThrowIfCancellationRequested();

        var formatterCodeGenContext = new SerializationFormatterCodeGenContext(formattersNamespace, serializationInfoCollection.RequireRegistrationFormatters, serializationInfoCollection.TypeHints);
        var resolverTexts = serialization.Generator.Build(context, formatterCodeGenContext);

        cancellationToken.ThrowIfCancellationRequested();

        outputs.Add((GeneratePathFromNamespaceAndTypeName(context.Namespace ?? string.Empty, context.InitializerPartialTypeName), MagicOnionInitializerGenerator.Build(context.Namespace, context.InitializerPartialTypeName, options, serviceCollection)));
        outputs.Add((GeneratePathFromNamespaceAndTypeName(context.Namespace ?? string.Empty, context.InitializerPartialTypeName + ".Resolver"), resolverTexts));

        foreach (var enumSerializationInfo in serializationInfoCollection.Enums)
        {
            outputs.Add((GeneratePathFromNamespaceAndTypeName(context.Namespace ?? string.Empty, context.InitializerPartialTypeName + ".Formatters." + enumSerializationInfo.FormatterName), serialization.EnumFormatterGenerator(new []{ enumSerializationInfo })));
        }

        foreach (var service in serviceCollection.Services)
        {
            var x = StaticMagicOnionClientGenerator.Build(new[] { service });
            outputs.Add((GeneratePathFromNamespaceAndTypeName(service.ServiceType.Namespace, service.GetClientName()), x));
        }

        foreach (var hub in serviceCollection.Hubs)
        {
            var x = StaticStreamingHubClientGenerator.Build(new [] { hub });
            outputs.Add((GeneratePathFromNamespaceAndTypeName(hub.ServiceType.Namespace, hub.GetClientName()), x));
        }

        return outputs.OrderBy(x => x.Path).ToArray();
    }

    static string GeneratePathFromNamespaceAndTypeName(string ns, string className)
    {
        return $"{ns}_{className}".Replace(".", "_").Replace("global::", string.Empty) + ".g.cs";
    }
}
