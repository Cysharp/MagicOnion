#pragma warning disable CS1998

using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.CodeGen;
using MagicOnion.Generator.CodeGen.Extensions;
using MagicOnion.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator;

public class MagicOnionCompiler
{
    static readonly Encoding NoBomUtf8 = new UTF8Encoding(false);

    readonly IMagicOnionGeneratorLogger logger;

    public MagicOnionCompiler(IMagicOnionGeneratorLogger logger)
    {
        this.logger = logger;
    }
    
    public IReadOnlyList<(string Path, string Source)> Generate(Compilation compilation, GeneratorOptions options, CancellationToken cancellationToken)
    {
        var outputs = new List<(string Path, string Source)>();

        // Prepare args
        var namespaceDot = string.IsNullOrWhiteSpace(options.Namespace) ? string.Empty : options.Namespace + ".";

        // Generator Start...
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:DisableAutoRegister: {options.DisableAutoRegister}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Namespace: {options.Namespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:UserDefinedFormattersNamespace: {options.MessagePackFormatterNamespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:SerializerType: {options.Serializer}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Assembly version: {typeof(MagicOnionCompiler).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.OSDescription: {RuntimeInformation.OSDescription}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.FrameworkDescription: {RuntimeInformation.FrameworkDescription}");

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
                EnumFormatterGenerator: x => new EnumTemplate()
                {
                    Namespace = namespaceDot + "Formatters",
                    EnumSerializationInfos = x.ToArray()
                }.TransformText()
            ),
            _ => throw new NotImplementedException(),
        };

        var sw = Stopwatch.StartNew();

        sw.Restart();
        logger.Information("Collect services and methods Start");
        var collector = new MethodCollector(logger, cancellationToken);
        var serviceCollection = collector.Collect(compilation);
        logger.Information("Collect services and methods Complete:" + sw.Elapsed.ToString());

        cancellationToken.ThrowIfCancellationRequested();

        sw.Restart();
        logger.Information("Collect serialization information Start");
        var serializationInfoCollector = new SerializationInfoCollector(logger, serialization.Mapper);
        var serializationInfoCollection = serializationInfoCollector.Collect(serviceCollection);
        logger.Information("Collect serialization information Complete:" + sw.Elapsed.ToString());

        cancellationToken.ThrowIfCancellationRequested();

        logger.Information("Code Generation Start");
        sw.Restart();
        
        var registerTemplate = new RegisterTemplate
        {
            Namespace = options.Namespace,
            Services = serviceCollection.Services,
            Hubs = serviceCollection.Hubs,
            DisableAutoRegisterOnInitialize = options.DisableAutoRegister,
        };

        var formatterCodeGenContext = new SerializationFormatterCodeGenContext(serialization.Namespace, namespaceDot + "Formatters", serialization.InitializerName, serializationInfoCollection.RequireRegistrationFormatters, serializationInfoCollection.TypeHints);
        var resolverTexts = serialization.Generator.Build(formatterCodeGenContext);

        cancellationToken.ThrowIfCancellationRequested();


        outputs.Add((GeneratePathFromNamespaceAndTypeName(registerTemplate.Namespace, "MagicOnionInitializer"), WithAutoGenerated(registerTemplate.TransformText())));
        outputs.Add((GeneratePathFromNamespaceAndTypeName(formatterCodeGenContext.Namespace, formatterCodeGenContext.InitializerName), WithAutoGenerated(resolverTexts)));

        foreach (var enumSerializationInfo in serializationInfoCollection.Enums)
        {
            outputs.Add((GeneratePathFromNamespaceAndTypeName(namespaceDot + "Formatters", enumSerializationInfo.FormatterName), WithAutoGenerated(serialization.EnumFormatterGenerator(new []{ enumSerializationInfo }))));
        }

        foreach (var service in serviceCollection.Services)
        {
            var x = StaticMagicOnionClientGenerator.Build(new[] { service });
            outputs.Add((GeneratePathFromNamespaceAndTypeName(service.ServiceType.Namespace, service.GetClientName()), WithAutoGenerated(x)));
        }

        foreach (var hub in serviceCollection.Hubs)
        {
            var x = StaticStreamingHubClientGenerator.Build(new [] { hub });
            outputs.Add((GeneratePathFromNamespaceAndTypeName(hub.ServiceType.Namespace, hub.GetClientName()), WithAutoGenerated(x)));
        }

        if (serviceCollection.Services.Count == 0 && serviceCollection.Hubs.Count == 0)
        {
            logger.Information("Generated result is empty, unexpected result?");
        }

        logger.Information("Code Generation Complete:" + sw.Elapsed.ToString());

        return outputs.OrderBy(x => x.Path).ToArray();
    }

    static string GeneratePathFromNamespaceAndTypeName(string ns, string className)
    {
        return $"{ns}_{className}".Replace(".", "_").Replace("global::", string.Empty) + ".g.cs";
    }

    static string WithAutoGenerated(string s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine(s);
        return sb.ToString();
    }

    static string NormalizeNewLines(string content)
    {
        // The T4 generated code may be text with mixed line ending types. (CR + CRLF)
        // We need to normalize the line ending type in each Operating Systems. (e.g. Windows=CRLF, Linux/macOS=LF)
        return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
    }

    void Output(string path, string text)
    {
        path = path.Replace("global::", "");

        logger.Information($"Write to {path}");

        var fi = new FileInfo(path);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }

        System.IO.File.WriteAllText(path, NormalizeNewLines(text), NoBomUtf8);
    }
}
