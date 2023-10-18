using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateTest
{
    [Fact]
    public async Task NoGenerate()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
            UnaryResult PingAsync();
            UnaryResult<bool> CanGreetAsync();
        }

        // The source code has no `MagicOnionClientGeneration` attributed class.
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task NotPartial()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
            UnaryResult PingAsync();
            UnaryResult<bool> CanGreetAsync();
        }

        [MagicOnionClientGeneration(typeof(IGreeterService))]
        class {|#0:MagicOnionInitializer|} {}
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.TypeSpecifyingClientGenerationAttributedMustBePartial.Id, DiagnosticSeverity.Error).WithLocation(0)}
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions);
    }

    // NOTE: MagicOnionClientGeneration has `AttributeUsage(AttributeTarget.Class)`
    //[Fact]
    //public async Task NotClass()
    //{
    //    var source = """
    //    using MagicOnion;
    //    using MagicOnion.Client;
    //    
    //    namespace MyApplication1;
    //
    //    public interface IGreeterService : IService<IGreeterService>
    //    {
    //        UnaryResult<string> HelloAsync(string name, int age);
    //        UnaryResult PingAsync();
    //        UnaryResult<bool> CanGreetAsync();
    //    }
    //
    //    [MagicOnionClientGeneration(typeof(IGreeterService))]
    //    struct {|#0:MagicOnionInitializer|} {}
    //    """;
    //
    //    var verifierOptions = VerifierOptions.Default with
    //    {
    //        TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
    //        ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.TypeSpecifyingClientGenerationAttributedMustBePartial.Id, DiagnosticSeverity.Error).WithLocation(0)}
    //    };
    //    await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions);
    //}

    [Fact]
    public async Task Generate()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
            UnaryResult PingAsync();
            UnaryResult<bool> CanGreetAsync();
        }
        
        [MagicOnionClientGeneration(typeof(IGreeterService))]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Generate_Namespace()
    {
        var source = """
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace MyApplication1.Net.Remoting;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
            UnaryResult PingAsync();
            UnaryResult<bool> CanGreetAsync();
        }
        
        [MagicOnionClientGeneration(typeof(IGreeterService))]
        partial class MagicOnionInitializer {}
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task ImplicitUsings_PropertyGroup_Enable()
    {
        var sources = new []
        {
            ("Usings.cs",
                """
                // ImplicitUsings: Microsoft.NET.Sdk
                global using global::System;
                global using global::System.Collections.Generic;
                global using global::System.IO;
                global using global::System.Linq;
                global using global::System.Net.Http;
                global using global::System.Threading;
                global using global::System.Threading.Tasks;
                """),
            ("IMyService.cs",
                """
                using MagicOnion;
                using MagicOnion.Client;
                using MessagePack;
                
                namespace MyNamespace;
                
                public interface IMyService : IService<IMyService>
                {
                    UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
                }
                
                
                [MagicOnionClientGeneration(typeof(IMyService))]
                partial class MagicOnionInitializer {}
                """
            ),
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(sources);
    }

    //[Fact]
    //public async Task ImplicitUsings_PropertyGroup_Disable()
    //{
    //    var sources = new []
    //    {
    //        ("IMyService.cs",
    //            """
    //            using MagicOnion;
    //            using MessagePack;
    //            
    //            namespace MyNamespace;
    //            
    //            public interface IMyService : IService<IMyService>
    //            {
    //                UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
    //            }
    //            """
    //        ),
    //    };
    //    await MagicOnionSourceGeneratorVerifier.RunAsync(sources);
    //}
}
