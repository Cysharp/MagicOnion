#define WRITE_EXPECTED

// https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/develop/tests/MessagePack.SourceGenerator.Tests/Verifiers/CSharpSourceGeneratorVerifier%601%2BTest.cs
// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using VerifyCS = MagicOnion.Client.SourceGenerator.Tests.Verifiers.MagicOnionSourceGeneratorVerifier;

namespace MagicOnion.Client.SourceGenerator.Tests.Verifiers;

internal record VerifierOptions
{
    public bool UseMemoryPack { get; init; }
    public TestBehaviors? TestBehaviorsOverride { get; init; }
    public IReadOnlyList<DiagnosticResult>? ExpectedDiagnostics { get; init; }

    public static VerifierOptions Default { get; } = new VerifierOptions
    {
        UseMemoryPack = false,
    };
}

internal class MagicOnionSourceGeneratorVerifier
{
    public static async Task RunAsync(string testSourceCode, GeneratorOptions? options = null, VerifierOptions? verifierOptions = null, [CallerFilePath]string? testFile = null, [CallerMemberName]string? testMethod = null)
    {
        if (string.IsNullOrEmpty(testSourceCode)) throw new ArgumentNullException(nameof(testSourceCode));
        if (string.IsNullOrEmpty(testFile)) throw new ArgumentNullException(nameof(testFile));
        if (string.IsNullOrEmpty(testMethod)) throw new ArgumentNullException(nameof(testMethod));

        await RunAsync(new[] { ("Source.cs", testSourceCode) }, options, verifierOptions, testFile, testMethod);
    }

    public static async Task RunAsync(IEnumerable<(string Path, string Content)> testSourceCodes, GeneratorOptions? options = null, VerifierOptions? verifierOptions = null, [CallerFilePath]string? testFile = null, [CallerMemberName]string? testMethod = null)
    {
        if (testSourceCodes is null) throw new ArgumentNullException(nameof(testSourceCodes));
        if (string.IsNullOrEmpty(testFile)) throw new ArgumentNullException(nameof(testFile));
        if (string.IsNullOrEmpty(testMethod)) throw new ArgumentNullException(nameof(testMethod));

        var test = new VerifyCS.Test(testFile, testMethod)
        {
            TestState =
            {
            },
        };

        if (options is not null)
        {
            test.TestState.AdditionalFiles.Add((GeneratorOptions.JsonFileName, options.ToJson()));
        }

        if (verifierOptions is not null)
        {
            if (verifierOptions.ExpectedDiagnostics is not null)
            {
                test.ExpectedDiagnostics.AddRange(verifierOptions.ExpectedDiagnostics);
            }

            if (verifierOptions.TestBehaviorsOverride is not null)
            {
                test.TestBehaviors = verifierOptions.TestBehaviorsOverride.Value;
            }

            if (verifierOptions.UseMemoryPack)
            {
                // MemoryPack.Core
                test.TestState.AdditionalReferences.Add(typeof(MemoryPack.IMemoryPackFormatter).Assembly);
                // MagicOnion.Serialization.MemoryPack
                test.TestState.AdditionalReferences.Add(typeof(MagicOnion.Serialization.MemoryPack.DynamicArgumentTupleFormatter).Assembly);
            }
        }

        test.TestState.Sources.AddRange(testSourceCodes.Select(x => (x.Path, SourceText.From(x.Content, Encoding.UTF8, SourceHashAlgorithm.Sha1))));
        await test.RunAsync();
    }

    internal class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, XUnitVerifier>
    {
        readonly string testFile;
        readonly string testMethod;

        public Test(string testFile, string testMethod)
        {
            this.testFile = testFile;
            this.testMethod = testMethod;

            this.ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
            this.AddAdditionalReferences();

#if WRITE_EXPECTED
            TestBehaviors |= TestBehaviors.SkipGeneratedSourcesCheck;
#endif
        }

        void AddAdditionalReferences()
        {
            // MagicOnion.Abstractions
            this.TestState.AdditionalReferences.Add(typeof(MagicOnion.UnaryResult).Assembly);
            // MagicOnion.Client
            this.TestState.AdditionalReferences.Add(typeof(MagicOnion.Client.MagicOnionClient).Assembly);
            // MagicOnion.Shared
            this.TestState.AdditionalReferences.Add(typeof(MagicOnion.MagicOnionMarshallers).Assembly);
            // MessagePack
            this.TestState.AdditionalReferences.Add(typeof(MessagePack.Formatters.IMessagePackFormatter).Assembly);
            // MessagePack.Annotations
            this.TestState.AdditionalReferences.Add(typeof(MessagePack.MessagePackObjectAttribute).Assembly);
            // Grpc.Core.Api
            this.TestState.AdditionalReferences.Add(typeof(Grpc.Core.AsyncUnaryCall<>).Assembly);
            // Grpc.Net.Client
            this.TestState.AdditionalReferences.Add(typeof(Grpc.Net.Client.GrpcChannel).Assembly);
        }

        void AddGeneratedReferenceSources()
        {
            var prefix = $"{typeof(Test).Assembly.GetName().Name}.Resources.{Path.GetFileNameWithoutExtension(testFile)}.{testMethod}.";

            foreach (var resName in typeof(Test).Assembly.GetManifestResourceNames().OrderBy(x => x))
            {
                if (!resName.StartsWith(prefix)) continue;

                using var stream = typeof(Test).Assembly.GetManifestResourceStream(resName) ?? throw new InvalidOperationException();
                using var reader = new StreamReader(stream);
                var source = reader.ReadToEnd();
                TestState.GeneratedSources.Add((typeof(MagicOnion.Client.SourceGenerator.MagicOnionClientSourceGenerator), resName.Substring(prefix.Length), SourceText.From(source, Encoding.UTF8, SourceHashAlgorithm.Sha1)));
            }
        }

        protected override Task RunImplAsync(CancellationToken cancellationToken)
        {
            AddGeneratedReferenceSources();
            return base.RunImplAsync(cancellationToken);
        }

        protected override async Task<(Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics)> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            var (compilation, diagnostics) = await base.GetProjectCompilationAsync(project, verifier, cancellationToken);
            var resourceDirectory = Path.Combine(Path.GetDirectoryName(testFile)!, "Resources", Path.GetFileNameWithoutExtension(testFile), testMethod);

            foreach (var syntaxTree in compilation.SyntaxTrees.Skip(project.DocumentIds.Count))
            {
                WriteTreeToDiskIfNecessary(syntaxTree, resourceDirectory, "");
            }

            return (compilation, diagnostics);
        }


        protected override IEnumerable<Type> GetSourceGenerators()
        {
            yield return typeof(MagicOnionClientSourceGenerator);
        }

        [Conditional("WRITE_EXPECTED")]
        static void WriteTreeToDiskIfNecessary(SyntaxTree tree, string resourceDirectory, string fileNamePrefix)
        {
            if (tree.Encoding is null)
            {
                throw new ArgumentException("Syntax tree encoding was not specified");
            }

            var name = fileNamePrefix + Path.GetFileName(tree.FilePath);
            var filePath = Path.Combine(resourceDirectory, name);
            Directory.CreateDirectory(resourceDirectory);
            File.WriteAllText(filePath, tree.GetText().ToString(), tree.Encoding);
        }
    }
}
