#pragma warning disable CS1998

using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.CodeGen;
using MagicOnion.Generator.Utils;
using MagicOnion.Generator.CodeGen.Extensions;
using MagicOnion.Generator.Internal;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator;

public enum SerializerType
{
    MessagePack,
    MemoryPack,
}

public class MagicOnionCompiler
{
    static readonly Encoding NoBomUtf8 = new UTF8Encoding(false);

    readonly IMagicOnionGeneratorLogger logger;
    readonly CancellationToken cancellationToken;

    public MagicOnionCompiler(IMagicOnionGeneratorLogger logger, CancellationToken cancellationToken)
    {
        this.logger = logger;
        this.cancellationToken = cancellationToken;
    }

    public async Task GenerateFileAsync(
        string input,
        string output,
        bool disableAutoRegister,
        string @namespace,
        string conditionalSymbol,
        string userDefinedFormattersNamespace,
        SerializerType serializerType)
    {
        var conditionalSymbols = conditionalSymbol?.Split(',') ?? Array.Empty<string>();

        // Generator Start...
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Input: {input}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:ConditionalSymbol: {conditionalSymbol}");

        var sw = Stopwatch.StartNew();
        logger.Information("Project Compilation Start:" + input);
        var compilation = await PseudoCompilation.CreateFromProjectAsync(new[] { input }, conditionalSymbols, logger, cancellationToken);
        logger.Information("Project Compilation Complete:" + sw.Elapsed.ToString());

        var outputs = await GenerateAsync(compilation, Path.GetFileName(output), disableAutoRegister, @namespace, userDefinedFormattersNamespace, serializerType);

        sw.Restart();
        logger.Information("Writing generated codes");
        if (Path.GetExtension(output) == ".cs")
        {
            Output(output, outputs[0].Source);
        }
        else
        {
            foreach (var o in outputs)
            {
                Output(o.Path, o.Source);
            }
        }
        logger.Information("Writing generated codes Complete:" + sw.Elapsed.ToString());
    }

    
    public async Task<IReadOnlyList<(string Path, string Source)>> GenerateAsync(
        Compilation compilation,
        string generatedFileNameBase,
        bool disableAutoRegister,
        string @namespace,
        string userDefinedFormattersNamespace,
        SerializerType serializerType
    )
    {
        var outputs = new List<(string Path, string Source)>();

        // Prepare args
        var namespaceDot = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : @namespace + ".";

        // Generator Start...
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:DisableAutoRegister: {disableAutoRegister}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Namespace: {@namespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:UserDefinedFormattersNamespace: {userDefinedFormattersNamespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:SerializerType: {serializerType}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Assembly version: {typeof(MagicOnionCompiler).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.OSDescription: {RuntimeInformation.OSDescription}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.FrameworkDescription: {RuntimeInformation.FrameworkDescription}");

        // Configure serialization
        (ISerializationFormatterNameMapper Mapper, string Namespace, string InitializerName, ISerializerFormatterGenerator Generator, Func<IEnumerable<EnumSerializationInfo>, string> EnumFormatterGenerator)
            serialization = serializerType switch
        {
            SerializerType.MemoryPack => (
                Mapper: new MemoryPackFormatterNameMapper(),
                Namespace: @namespace,
                InitializerName: "MagicOnionMemoryPackFormatterProvider",
                Generator: new MemoryPackFormatterRegistrationGenerator(),
                EnumFormatterGenerator: _ => string.Empty
            ),
            SerializerType.MessagePack => (
                Mapper: new MessagePackFormatterNameMapper(userDefinedFormattersNamespace),
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
            Namespace = @namespace,
            Services = serviceCollection.Services,
            Hubs = serviceCollection.Hubs,
            DisableAutoRegisterOnInitialize = disableAutoRegister,
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
        return $"{ns}_{className}".Replace(".", "_").Replace("global::", string.Empty) + ".cs";
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
