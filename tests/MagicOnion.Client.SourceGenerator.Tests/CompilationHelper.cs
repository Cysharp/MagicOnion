using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SampleServiceDefinitions.Services;

namespace MagicOnion.Client.SourceGenerator.Tests;

public static class CompilationHelper
{
    public static (Compilation Compilation, SemanticModel SemanticModel) Create(string code)
    {
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, CSharpParseOptions.Default);
        var assemblyName = Guid.NewGuid().ToString();
        var refAsmDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Private.CoreLib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Runtime.Extensions.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Linq.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Console.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Memory.dll")),
            MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "netstandard.dll")),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Grpc.Core.AsyncUnaryCall<>).Assembly.Location), // Grpc.Core.Api
            MetadataReference.CreateFromFile(typeof(Grpc.Net.Client.GrpcChannel).Assembly.Location), // Grpc.Net.Client
            MetadataReference.CreateFromFile(typeof(MessagePack.MessagePackSerializer).Assembly.Location), // MessagePack
            MetadataReference.CreateFromFile(typeof(MessagePack.MessagePackObjectAttribute).Assembly.Location), // MessagePack.Annotations
            MetadataReference.CreateFromFile(typeof(MagicOnion.IService<>).Assembly.Location), // MagicOnion.Abstractions
            MetadataReference.CreateFromFile(typeof(MagicOnion.Client.MagicOnionClient).Assembly.Location), // MagicOnion.Client
            MetadataReference.CreateFromFile(typeof(MagicOnion.Serialization.MagicOnionSerializerProvider).Assembly.Location), // MagicOnion.Shared
            MetadataReference.CreateFromFile(typeof(MagicOnion.Serialization.MessagePackMagicOnionSerializerProvider).Assembly.Location), // MagicOnion.Serialization.MessagePack
            MetadataReference.CreateFromFile(typeof(IGreeterService).Assembly.Location), // SampleServiceDefinitions
        };
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create(assemblyName, new [] { syntaxTree }, references, compilationOptions);
        //if (compilation.GetDiagnostics().Any(x => x.Severity == DiagnosticSeverity.Error))
        //{
        //    throw new InvalidOperationException("Failed to compile the source code. \n" + string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(x => x.ToString())));
        //}
        return (compilation, compilation.GetSemanticModel(syntaxTree));
    }
}
