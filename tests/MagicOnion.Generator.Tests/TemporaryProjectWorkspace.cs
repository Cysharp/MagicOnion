using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MagicOnion.Generator.Tests
{
    public class TemporaryProjectWorkspace : IDisposable
    {
        private readonly string _tempDirPath;
        private readonly string _csprojFileName = "TempProject.csproj";
        private readonly string _abstractionsProjectDir;

        public string CsProjectPath { get; }
        public string ProjectDirectory { get; }
        public string OutputDirectory { get; }

        public static TemporaryProjectWorkspace Create()
        {
            return new TemporaryProjectWorkspace();
        }

        private TemporaryProjectWorkspace()
        {
            _tempDirPath = Path.Combine(Path.GetTempPath(), $"MagicOnion.Generator.Tests-{Guid.NewGuid()}");

            ProjectDirectory = Path.Combine(_tempDirPath, "Project");
            OutputDirectory = Path.Combine(_tempDirPath, "Output");

            Directory.CreateDirectory(ProjectDirectory);
            Directory.CreateDirectory(OutputDirectory);

            var solutionRootDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../../.."));
            _abstractionsProjectDir = Path.Combine(solutionRootDir, "src/MagicOnion.Abstractions/MagicOnion.Abstractions.csproj");

            CsProjectPath = Path.Combine(ProjectDirectory, _csprojFileName);
            AddFileToProject(_csprojFileName, @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""" + _abstractionsProjectDir + @""" />
  </ItemGroup>
</Project>
");
        }

        public void AddFileToProject(string fileName, string contents)
        {
            File.WriteAllText(Path.Combine(ProjectDirectory, fileName), contents.Trim());
        }

        public void AddFileToOutput(string fileName, string contents)
        {
            File.WriteAllText(Path.Combine(OutputDirectory, fileName), contents.Trim());
        }

        public INamedTypeSymbol[] GetNamedTypeSymbolsFromGenerated()
        {
            var compilation = GetCompilationFromGenerated();

            return compilation.SyntaxTrees
                .Select(x => compilation.GetSemanticModel(x))
                .SelectMany(semanticModel =>
                {
                    return semanticModel.SyntaxTree.GetRoot()
                        .DescendantNodes()
                        .Select(x => semanticModel.GetDeclaredSymbol(x))
                        .OfType<INamedTypeSymbol>();
                })
                .ToArray();
        }

        public Compilation GetCompilationFromGenerated()
        {
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString())
                .AddSyntaxTrees(
                    Directory.EnumerateFiles(ProjectDirectory, "*.cs", SearchOption.AllDirectories)
                        .Concat(Directory.EnumerateFiles(OutputDirectory, "*.cs", SearchOption.AllDirectories))
                        .Select(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x), CSharpParseOptions.Default, x))
                )
                .AddReferences();

            return compilation;
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirPath, true);
        }
    }
}