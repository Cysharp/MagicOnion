using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateServiceTest
{
    [Fact]
    public async Task Return_UnaryResultNonGeneric()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult A();
            }

            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_UnaryResultOfT()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<int> A();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_UnaryResultOfValueType()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<long> A();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_UnaryResultOfRefType()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<string> A();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Invalid_Return_TaskOfUnaryResultOfT()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        
        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                Task<UnaryResult<int>> {|#0:A|}();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error).WithLocation(0)}
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }

    [Fact]
    public async Task Return_StreamingResult()
    {
        var source = """
        using System;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;
        using System.Threading.Tasks;

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                Task<ClientStreamingResult<string, string>> ClientStreamingAsync();
                Task<ServerStreamingResult<string>> ServerStreamingAsync();
                Task<DuplexStreamingResult<string, string>> DuplexStreamingAsync();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Invalid_Return_NonGenerics()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                int {|#0:A|}();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error).WithLocation(0)}
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }

    [Fact]
    public async Task Invalid_Return_NonSupportedUnaryResultOfT()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<ServerStreamingResult<int>> {|#0:A|}();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.UnaryUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error).WithLocation(0)}
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }

    [Fact]
    public async Task Invalid_Return_RawStreaming_NonTask()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                ClientStreamingResult<string, string> {|#0:ClientStreamingAsync|}();
                ServerStreamingResult<string> {|#1:ServerStreamingAsync|}();
                DuplexStreamingResult<string, string> {|#2:DuplexStreamingAsync|}();
            }
        
            [MagicOnionClientGeneration(typeof(IMyService))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error).WithLocation(0),
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error).WithLocation(1),
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error).WithLocation(2),
            }
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }
}
