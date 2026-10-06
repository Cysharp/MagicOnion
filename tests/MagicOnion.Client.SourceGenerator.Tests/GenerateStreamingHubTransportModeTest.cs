using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateStreamingHubTransportModeTest
{
    const string UnreliableHubSource = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                [Unreliable]
                void OnMove(int x, int y);
                void OnJoined(string name);
                Task<int> OnQueryAsync(int value);
            }

            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                [Unreliable]
                Task MoveAsync(int x, int y);
                [UnreliableUnordered]
                ValueTask EmoteAsync(int id);
                [Transport(StreamingHubTransportMode.UnreliableSequenced)]
                void Ping();
                Task<int> QueryAsync(int value);
                Task JoinAsync(string name);
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

    [Fact]
    public async Task Unreliable_Methods()
    {
        await MagicOnionSourceGeneratorVerifier.RunAsync(UnreliableHubSource);
    }

    [Fact]
    public async Task Unreliable_Methods_DiagnosticHandler()
    {
        var source = UnreliableHubSource.Replace("[MagicOnionClientGeneration(typeof(IMyHub))]", "[MagicOnionClientGeneration(typeof(IMyHub), EnableStreamingHubDiagnosticHandler = true)]");
        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task InterfaceDefault_WithReliableOverride()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            [Unreliable]
            public interface IMyHubReceiver
            {
                void OnMove(int x);
                [Reliable]
                void OnJoined(string name);
            }

            [Unreliable]
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task MoveAsync(int x);
                [Reliable]
                Task<int> JoinAsync(string name);
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task InheritedDefault_NearestToRootWins()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void OnMove(int x);
            }

            [Reliable]
            public interface IMyHubBase
            {
                Task BaseAsync();
            }

            [Unreliable]
            public interface IMyHubMiddle : IMyHubBase
            {
                Task MiddleAsync();
            }

            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>, IMyHubMiddle
            {
                Task<int> RootAsync();
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Invalid_UnreliableMethod_ReturnsValue()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyHubReceiver
            {
            }

            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                [Unreliable]
                Task<int> {|#0:BadAsync|}();
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.StreamingHubUnreliableMethodMustNotReturnValue.Id, DiagnosticSeverity.Error).WithLocation(0),
            }
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }

    [Fact]
    public async Task Invalid_UnreliableReceiverMethod_ClientResult()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                [Unreliable]
                Task<int> {|#0:OnQueryAsync|}(int value);
            }

            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task MoveAsync();
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.StreamingHubUnreliableReceiverMethodMustReturnVoid.Id, DiagnosticSeverity.Error).WithLocation(0),
            }
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }

    [Fact]
    public async Task Invalid_MultipleTransportAttributes()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyHubReceiver
            {
            }

            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                [Unreliable, Reliable]
                Task {|#0:MoveAsync|}();
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.StreamingHubTransportAttributeConflict.Id, DiagnosticSeverity.Error).WithLocation(0),
            }
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }

    [Fact]
    public async Task Invalid_AmbiguousInheritedDefaults()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        using MagicOnion.Client;

        namespace TempProject
        {
            public interface IMyHubReceiver
            {
            }

            public interface IMyHubBase
            {
                Task {|#0:BaseAsync|}();
            }

            [Unreliable]
            public interface IMyHubLeft : IMyHubBase
            {
            }

            [Reliable]
            public interface IMyHubRight : IMyHubBase
            {
            }

            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>, IMyHubLeft, IMyHubRight
            {
            }

            [MagicOnionClientGeneration(typeof(IMyHub))]
            partial class MagicOnionInitializer {}
        }
        """;

        var verifierOptions = VerifierOptions.Default with
        {
            TestBehaviorsOverride = TestBehaviors.SkipGeneratedSourcesCheck,
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(MagicOnionDiagnosticDescriptors.StreamingHubTransportModeAmbiguous.Id, DiagnosticSeverity.Error).WithLocation(0),
            }
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(source, verifierOptions: verifierOptions);
    }
}
