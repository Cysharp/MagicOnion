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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult A();
            }
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<int> A();
            }
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<long> A();
            }
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<string> A();
            }
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                Task<UnaryResult<int>> A();
            }
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error)}
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
        using System.Threading.Tasks;

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                Task<ClientStreamingResult<string, string>> ClientStreamingAsync();
                Task<ServerStreamingResult<string>> ServerStreamingAsync();
                Task<DuplexStreamingResult<string, string>> DuplexStreamingAsync();
            }
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                int A();
            }
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error)}
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                UnaryResult<ServerStreamingResult<int>> A();
            }
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[] {new DiagnosticResult(MagicOnionDiagnosticDescriptors.UnaryUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error)}
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

        namespace TempProject
        {
            public interface IMyService : IService<IMyService>
            {
                ClientStreamingResult<string, string> ClientStreamingAsync();
                ServerStreamingResult<string> ServerStreamingAsync();
                DuplexStreamingResult<string, string> DuplexStreamingAsync();
            }
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error),
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error),
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType.Id, DiagnosticSeverity.Error),
            }
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }
}
