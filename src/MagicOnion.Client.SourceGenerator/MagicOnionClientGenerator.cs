using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.CodeGen;
using MagicOnion.Client.SourceGenerator.CodeGen.Extensions;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

public static class MagicOnionClientGenerator
{
    public static IReadOnlyList<(string Path, string Source)> Generate(MagicOnionServiceCollection serviceCollection, GeneratorOptions options, CancellationToken cancellationToken)
    {
        var outputs = new List<(string Path, string Source)>();

        // <Namespace>.
        var namespaceDot = string.IsNullOrWhiteSpace(options.Namespace) ? string.Empty : options.Namespace + ".";
        // <Namespace>.Formatters
        var formattersNamespace = namespaceDot + "Formatters";

        // Configure serialization
        (ISerializationFormatterNameMapper Mapper, string Namespace, string InitializerName, ISerializerFormatterGenerator Generator, Func<IEnumerable<EnumSerializationInfo>, string> EnumFormatterGenerator)
            serialization = options.Serializer switch
        {
            GeneratorOptions.SerializerType.MemoryPack => (
                Mapper: new MemoryPackFormatterNameMapper(),
                Namespace: options.Namespace,
                InitializerName: "MagicOnionMemoryPackFormatterProvider",
                Generator: new MemoryPackFormatterRegistrationGenerator(),
                EnumFormatterGenerator: _ => string.Empty
            ),
            GeneratorOptions.SerializerType.MessagePack => (
                Mapper: new MessagePackFormatterNameMapper(options.MessagePackFormatterNamespace),
                Namespace: namespaceDot + "Resolvers",
                InitializerName: "MagicOnionResolver",
                Generator: new MessagePackFormatterResolverGenerator(),
                EnumFormatterGenerator: x => MessagePackEnumFormatterGenerator.Build(formattersNamespace, x)
            ),
            _ => throw new NotImplementedException(),
        };

        cancellationToken.ThrowIfCancellationRequested();

        var serializationInfoCollector = new SerializationInfoCollector(serialization.Mapper);
        var serializationInfoCollection = serializationInfoCollector.Collect(serviceCollection);

        cancellationToken.ThrowIfCancellationRequested();

        var formatterCodeGenContext = new SerializationFormatterCodeGenContext(serialization.Namespace, formattersNamespace, serialization.InitializerName, serializationInfoCollection.RequireRegistrationFormatters, serializationInfoCollection.TypeHints);
        var resolverTexts = serialization.Generator.Build(formatterCodeGenContext);

        cancellationToken.ThrowIfCancellationRequested();

        outputs.Add((GeneratePathFromNamespaceAndTypeName(options.Namespace, "MagicOnionInitializer"), MagicOnionInitializerGenerator.Build(options, serviceCollection)));
        outputs.Add((GeneratePathFromNamespaceAndTypeName(formatterCodeGenContext.Namespace, formatterCodeGenContext.InitializerName), resolverTexts));

        foreach (var enumSerializationInfo in serializationInfoCollection.Enums)
        {
            outputs.Add((GeneratePathFromNamespaceAndTypeName(formattersNamespace, enumSerializationInfo.FormatterName), serialization.EnumFormatterGenerator(new []{ enumSerializationInfo })));
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
