using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicOnion.Generator.Tests
{
    /// <summary>
    /// Provides a temporary work area for unit testing.
    /// </summary>
    public class TemporaryProjectWorkarea : IDisposable
    {
        readonly TemporaryProjectWorkareaOptions options;
        readonly string tempDirPath;
        readonly string csprojFileName = "TempProject.csproj";

        public string CsProjectPath { get; }

        public string ProjectDirectory { get; }

        public string OutputDirectory { get; }

        /// <summary>
        /// Gets the identifier of the workarea.
        /// </summary>
        public Guid WorkareaId { get; }
        
        public static TemporaryProjectWorkarea Create(bool cleanOnDisposing = true)
        {
            return new TemporaryProjectWorkarea(TemporaryProjectWorkareaOptions.Default with { CleanOnDisposing = cleanOnDisposing });
        }

        public static TemporaryProjectWorkarea Create(TemporaryProjectWorkareaOptions options)
        {
            return new TemporaryProjectWorkarea(options ?? throw new ArgumentNullException(nameof(options)));
        }

        private TemporaryProjectWorkarea(TemporaryProjectWorkareaOptions options)
        {
            WorkareaId = Guid.NewGuid();

            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.tempDirPath = Path.Combine(Path.GetTempPath(), $"MagicOnion.Generator.Tests-{Guid.NewGuid()}");

            ProjectDirectory = Path.Combine(tempDirPath, "Project");
            OutputDirectory = Path.Combine(tempDirPath, "Output");

            Directory.CreateDirectory(ProjectDirectory);
            Directory.CreateDirectory(OutputDirectory);

            var solutionRootDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "../../../../.."));
            var abstractionsProjectDir = Path.Combine(solutionRootDir, "src/MagicOnion.Abstractions/MagicOnion.Abstractions.csproj");

            CsProjectPath = Path.Combine(ProjectDirectory, csprojFileName);
            var csprojContents = $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>{options.TargetFramework}</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""{abstractionsProjectDir}"" />
    {string.Join("", (options.AdditionalProjectReferences ?? Array.Empty<string>()).Select(x => $@"<ProjectReference Include=""{x}"" />"))}
    {string.Join("", (options.AdditionalPackageReferences ?? Array.Empty<string>()).Select(x => $@"<PackageReference Include=""{x}"" />"))}
  </ItemGroup>
  {options.AdditionalCsProjectContent}
</Project>
";
            AddFileToProject(csprojFileName, csprojContents);
        }

        public void AddFileToProject(string fileName, string contents)
        {
            File.WriteAllText(Path.Combine(ProjectDirectory, fileName), contents.Trim());
        }

        public OutputCompilation GetOutputCompilation()
        {
            var refAsmDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString())
                .AddSyntaxTrees(
                    Directory.EnumerateFiles(ProjectDirectory, "*.cs", SearchOption.AllDirectories)
                        .Concat(Directory.EnumerateFiles(OutputDirectory, "*.cs", SearchOption.AllDirectories))
                        .Select(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x), CSharpParseOptions.Default, x)))
                .AddReferences(
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
                    MetadataReference.CreateFromFile(typeof(MagicOnion.Client.MagicOnionClient).Assembly.Location), // MagicOnion.Client
                    MetadataReference.CreateFromFile(typeof(MagicOnion.MagicOnionMarshallers).Assembly.Location), // MagicOnion.Shared
                    MetadataReference.CreateFromFile(typeof(MagicOnion.IService<>).Assembly.Location), // MagicOnion.Abstractions
                    MetadataReference.CreateFromFile(typeof(MessagePack.IFormatterResolver).Assembly.Location), // MessagePack
                    MetadataReference.CreateFromFile(typeof(MessagePack.MessagePackObjectAttribute).Assembly.Location) // MessagePack.Annotations
                )
                .AddReferences((options.AdditionalReferences ?? Array.Empty<string>()).Select(x => MetadataReference.CreateFromFile(x)))
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return new OutputCompilation(this, compilation);
        }

        public void Dispose()
        {
            if (options.CleanOnDisposing)
            {
                Directory.Delete(tempDirPath, true);
            }
        }
    }

    public record TemporaryProjectWorkareaOptions(
        bool CleanOnDisposing = true,
        string TargetFramework = "netstandard2.0",
        string AdditionalCsProjectContent = "",
        IEnumerable<string>? AdditionalReferences = default,
        IEnumerable<string>? AdditionalPackageReferences = default,
        IEnumerable<string>? AdditionalProjectReferences = default
    )
    {
        public static TemporaryProjectWorkareaOptions Default { get; } = new TemporaryProjectWorkareaOptions();

    }

    public class OutputCompilation
    {
        readonly TemporaryProjectWorkarea workarea;

        public Compilation Compilation { get; }

        public OutputCompilation(TemporaryProjectWorkarea workarea, Compilation compilation)
        {
            this.workarea = workarea;
            this.Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        }

        public INamedTypeSymbol[] GetNamedTypeSymbolsFromGenerated()
        {
            return Compilation.SyntaxTrees
                .Select(x => Compilation.GetSemanticModel(x))
                .SelectMany(semanticModel =>
                {
                    return semanticModel.SyntaxTree.GetRoot()
                        .DescendantNodes()
                        .Select(x => semanticModel.GetDeclaredSymbol(x))
                        .OfType<INamedTypeSymbol>();
                })
                .ToArray();
        }

        public IReadOnlyList<string> GetResolverKnownFormatterTypes()
        {
            return Compilation.SyntaxTrees
                .SelectMany(x => x.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(x => x.Identifier.ToString().EndsWith("ResolverGetFormatterHelper"))
                    .SelectMany(x => x.DescendantNodes())
                    .OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Identifier.ToString() == "GetFormatter")
                    .SelectMany(x => x.DescendantNodes())
                    .OfType<SwitchSectionSyntax>()
                    .SelectMany(x => x.DescendantNodes())
                    .OfType<ObjectCreationExpressionSyntax>()
                    .SelectMany(x => x.ChildNodes())
                    .Where(x => x is QualifiedNameSyntax || x is IdentifierNameSyntax || x is GenericNameSyntax || x is PredefinedTypeSyntax)
                    .Select(x => x.ToString()))
                .ToArray();
        }

        public IReadOnlyList<Diagnostic> GetCompilationErrors()
        {
            return Compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
        }

        /// <summary>
        /// Load the generated assembly and execute the code in that context.
        /// </summary>
        public void ExecuteWithGeneratedAssembly(Action<AssemblyLoadContext, Assembly> action)
        {
            var memoryStream = new MemoryStream();
            Compilation.Emit(memoryStream);
            memoryStream.Position = 0;

            var assemblyLoadContext = new AssemblyLoadContext($"TempProject-{workarea.WorkareaId}", isCollectible: true);
            try
            {
                assemblyLoadContext.LoadFromStream(memoryStream);
                var assembly = assemblyLoadContext.Assemblies.First();
                action(assemblyLoadContext, assembly);
            }
            finally
            {
                assemblyLoadContext.Unload();
            }
        }
    }
}