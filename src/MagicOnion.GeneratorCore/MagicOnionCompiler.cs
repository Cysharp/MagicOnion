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
        // Prepare args
        var namespaceDot = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : @namespace + ".";
        var conditionalSymbols = conditionalSymbol?.Split(',') ?? Array.Empty<string>();

        // Generator Start...
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Input: {input}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Output: {output}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:DisableAutoRegister: {disableAutoRegister}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Namespace: {@namespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:ConditionalSymbol: {conditionalSymbol}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:UserDefinedFormattersNamespace: {userDefinedFormattersNamespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:SerializerType: {serializerType}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Assembly version: {typeof(MagicOnionCompiler).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.OSDescription: {RuntimeInformation.OSDescription}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.FrameworkDescription: {RuntimeInformation.FrameworkDescription}");

        // Configure serialization
        var serialization = serializerType switch
        {
            SerializerType.MemoryPack => (
                Mapper: (ISerializationFormatterNameMapper)new MemoryPackFormatterNameMapper(),
                Namespace: @namespace,
                InitializerName: "MagicOnionMemoryPackFormatterProvider",
                Generator: (ISerializerFormatterGenerator)new MemoryPackFormatterRegistrationGenerator(),
                EnumFormatterGenerator: (Func<IEnumerable<EnumSerializationInfo>, string>)(enumSerializationInfo => string.Empty)
            ),
            SerializerType.MessagePack => (
                Mapper: (ISerializationFormatterNameMapper)new MessagePackFormatterNameMapper(userDefinedFormattersNamespace),
                Namespace: namespaceDot + "Resolvers",
                InitializerName: "MagicOnionResolver",
                Generator: (ISerializerFormatterGenerator)new MessagePackFormatterResolverGenerator(),
                EnumFormatterGenerator: (Func<IEnumerable<EnumSerializationInfo>, string>)(enumSerializationInfo => new EnumTemplate()
                {
                    Namespace = namespaceDot + "Formatters",
                    EnumSerializationInfos = enumSerializationInfo.ToArray()
                }.TransformText())
            ),
            _ => throw new NotImplementedException(),
        };

        var sw = Stopwatch.StartNew();
        logger.Information("Project Compilation Start:" + input);
        var compilation = await PseudoCompilation.CreateFromProjectAsync(new[] { input }, conditionalSymbols, logger, cancellationToken);
        logger.Information("Project Compilation Complete:" + sw.Elapsed.ToString());

        sw.Restart();
        logger.Information("Collect services and methods Start");
        var collector = new MethodCollector(logger);
        var serviceCollection = collector.Collect(compilation);
        logger.Information("Collect services and methods Complete:" + sw.Elapsed.ToString());
            
        sw.Restart();
        logger.Information("Collect serialization information Start");
        var serializationInfoCollector = new SerializationInfoCollector(logger, serialization.Mapper);
        var serializationInfoCollection = serializationInfoCollector.Collect(serviceCollection);
        logger.Information("Collect serialization information Complete:" + sw.Elapsed.ToString());

        logger.Information("Output Generation Start");
        sw.Restart();
        
        var registerTemplate = new RegisterTemplate
        {
            Namespace = @namespace,
            Services = serviceCollection.Services,
            Hubs = serviceCollection.Hubs,
            DisableAutoRegisterOnInitialize = disableAutoRegister,
        };

        var formatterCodeGenContext = new SerializationFormatterCodeGenContext(serialization.Namespace, namespaceDot + "Formatters", serialization.InitializerName, serializationInfoCollection.RequireRegistrationFormatters);
        var resolverTexts = serialization.Generator.Build(formatterCodeGenContext);

        if (Path.GetExtension(output) == ".cs")
        {
            var enums = serializationInfoCollection.Enums
                .GroupBy(x => x.Namespace)
                .OrderBy(x => x.Key)
                .ToArray();

            var clientTexts = StaticMagicOnionClientGenerator.Build(serviceCollection.Services);
            var hubTexts = StaticStreamingHubClientGenerator.Build(serviceCollection.Hubs);

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine(registerTemplate.TransformText());
            sb.AppendLine(resolverTexts);
            foreach (var item in enums)
            {
                sb.AppendLine(serialization.EnumFormatterGenerator(item));
            }

            sb.AppendLine(clientTexts);
            sb.AppendLine(hubTexts);

            Output(output, sb.ToString());
        }
        else
        {
            Output(NormalizePath(output, registerTemplate.Namespace, "MagicOnionInitializer"), WithAutoGenerated(registerTemplate.TransformText()));
            Output(NormalizePath(output, formatterCodeGenContext.Namespace, formatterCodeGenContext.InitializerName), WithAutoGenerated(resolverTexts));

            foreach (var enumSerializationInfo in serializationInfoCollection.Enums)
            {
                Output(NormalizePath(output, namespaceDot + "Formatters", enumSerializationInfo.Name + "Formatter"), WithAutoGenerated(serialization.EnumFormatterGenerator(new []{ enumSerializationInfo })));
            }

            foreach (var service in serviceCollection.Services)
            {
                var x = StaticMagicOnionClientGenerator.Build(new[] { service });
                Output(NormalizePath(output, service.ServiceType.Namespace, service.GetClientName()), WithAutoGenerated(x));
            }

            foreach (var hub in serviceCollection.Hubs)
            {
                var x = StaticStreamingHubClientGenerator.Build(new [] { hub });
                Output(NormalizePath(output, hub.ServiceType.Namespace, hub.GetClientName()), WithAutoGenerated(x));
            }
        }

        if (serviceCollection.Services.Count == 0 && serviceCollection.Hubs.Count == 0)
        {
            logger.Information("Generated result is empty, unexpected result?");
        }

        logger.Information("Output Generation Complete:" + sw.Elapsed.ToString());
    }

    static string NormalizePath(string dir, string ns, string className)
    {
        return Path.Combine(dir, $"{ns}_{className}".Replace(".", "_").Replace("global::", string.Empty) + ".cs");
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
