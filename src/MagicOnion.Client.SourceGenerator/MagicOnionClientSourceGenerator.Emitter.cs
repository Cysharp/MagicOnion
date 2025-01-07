using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.CodeGen;
using MagicOnion.Client.SourceGenerator.CodeGen.MemoryPack;
using MagicOnion.Client.SourceGenerator.CodeGen.MessagePack;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator;

public partial class MagicOnionClientSourceGenerator
{
    public const string MagicOnionClientGenerationAttributeShortName = "MagicOnionClientGeneration";
    public const string MagicOnionClientGenerationAttributeName = $"{MagicOnionClientGenerationAttributeShortName}Attribute";
    public const string MagicOnionClientGenerationAttributeFullName = $"MagicOnion.Client.{MagicOnionClientGenerationAttributeName}";

    static class Emitter
    {
        public static void Emit(GenerationContext context, ImmutableArray<INamedTypeSymbol> interfaceSymbols, ReferenceSymbols referenceSymbols)
        {
            var (serviceCollection, diagnostics) = MethodCollector.Collect(interfaceSymbols, referenceSymbols, context.SourceProductionContext.CancellationToken);
            var generated = EmitCore(context, serviceCollection, context.SourceProductionContext.CancellationToken);

            foreach (var diagnostic in diagnostics)
            {
                context.SourceProductionContext.ReportDiagnostic(diagnostic);
            }

            foreach (var (path, source) in generated)
            {
                context.SourceProductionContext.AddSource(context.Options.GenerateFileHintNamePrefix + path, source);
            }
        }

        static IReadOnlyList<(string Path, string Source)> EmitCore(GenerationContext context, MagicOnionServiceCollection serviceCollection, CancellationToken cancellationToken)
        {
            var outputs = new List<(string Path, string Source)>();

            // Configure serialization
            (ISerializationFormatterNameMapper Mapper, ISerializerFormatterGenerator Generator, Func<IEnumerable<EnumSerializationInfo>, string> EnumFormatterGenerator)
                serialization = context.Options.Serializer switch
            {
                SerializerType.MemoryPack => (
                    Mapper: new MemoryPackFormatterNameMapper(),
                    Generator: new MemoryPackFormatterRegistrationGenerator(),
                    EnumFormatterGenerator: _ => string.Empty
                ),
                SerializerType.MessagePack => (
                    Mapper: new MessagePackFormatterNameMapper(context.Options.MessagePackFormatterNamespace),
                    Generator: new MessagePackFormatterResolverGenerator(emitGenericFormatterInstantiationAndTypeHints: false),
                    EnumFormatterGenerator: x => MessagePackEnumFormatterGenerator.Build(context, x)
                ),
                _ => throw new NotImplementedException(),
            };

            cancellationToken.ThrowIfCancellationRequested();

            var serializationInfoCollector = new SerializationInfoCollector(serialization.Mapper);
            var serializationInfoCollection = serializationInfoCollector.Collect(serviceCollection);

            cancellationToken.ThrowIfCancellationRequested();

            var formatterCodeGenContext = new SerializationFormatterCodeGenContext(context.Namespace ?? string.Empty, serializationInfoCollection.RequireRegistrationFormatters, serializationInfoCollection.TypeHints);
            var (serializerHintNameSuffix, serializationSource) = serialization.Generator.Build(context, formatterCodeGenContext);

            cancellationToken.ThrowIfCancellationRequested();

            outputs.Add((GeneratePathFromNamespaceAndTypeName(context.Namespace ?? string.Empty, context.InitializerPartialTypeName), MagicOnionInitializerGenerator.Build(context, serviceCollection)));
            outputs.Add((GeneratePathFromNamespaceAndTypeName(context.Namespace ?? string.Empty, context.InitializerPartialTypeName + serializerHintNameSuffix), serializationSource));

            if (serializationInfoCollection.Enums.Any())
            {
                outputs.Add((GeneratePathFromNamespaceAndTypeName(context.Namespace ?? string.Empty, context.InitializerPartialTypeName + ".EnumFormatters"), serialization.EnumFormatterGenerator(serializationInfoCollection.Enums)));
            }

            foreach (var service in serviceCollection.Services)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var x = StaticMagicOnionClientGenerator.Build(context, service);
                outputs.Add((GeneratePathFromNamespaceAndTypeName(service.ServiceType.Namespace, service.GetClientName()), x));
            }

            foreach (var hub in serviceCollection.Hubs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var x = StaticStreamingHubClientGenerator.Build(context, hub);
                outputs.Add((GeneratePathFromNamespaceAndTypeName(hub.ServiceType.Namespace, hub.GetClientName()), x));
            }

            return outputs.OrderBy(x => x.Path).ToArray();
        }

        static string GeneratePathFromNamespaceAndTypeName(string ns, string className)
        {
            return $"{ns}_{className}".Replace(".", "_").Replace("global::", string.Empty) + ".g.cs";
        }
    }
}
