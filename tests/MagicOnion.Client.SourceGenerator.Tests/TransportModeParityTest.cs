using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;
using MagicOnion.Internal;
using Microsoft.CodeAnalysis;
using SampleServiceDefinitions.Hubs;

namespace MagicOnion.Client.SourceGenerator.Tests;

/// <summary>
/// Verifies that the source generator (Roslyn symbols) and the runtime (reflection) resolve the same transport mode
/// for every method of the shared contract fixtures in SampleServiceDefinitions, so that static clients, dynamic clients,
/// and the server agree on which methods are unreliable and on the table exchanged during negotiation.
/// </summary>
public class TransportModeParityTest
{
    [Fact]
    public void MethodAttributes()
        => AssertParity<ITransportModeHub, ITransportModeReceiver>();

    [Fact]
    public void InterfaceDefault_WithReliableOverride()
        => AssertParity<ITransportModeDefaultHub, ITransportModeDefaultReceiver>();

    [Fact]
    public void InheritedDefaults_NearestToRootWins_UnrelatedBranchIgnored()
        => AssertParity<ITransportModeInheritedHub, ITransportModeReceiver>();

    [Fact]
    public void DiamondInheritance_AgreeingDefaults()
        => AssertParity<ITransportModeDiamondHub, ITransportModeReceiver>();

    static void AssertParity<THub, TReceiver>()
        where THub : IStreamingHub<THub, TReceiver>
    {
        // Roslyn: collect the hub from the compiled SampleServiceDefinitions assembly referenced by the compilation.
        var (compilation, _) = CompilationHelper.Create("");
        Assert.True(ReferenceSymbols.TryCreate(compilation, out var referenceSymbols));
        var hubSymbol = compilation.GetTypeByMetadataName(typeof(THub).FullName!);
        Assert.NotNull(hubSymbol);

        var (collection, diagnostics) = MethodCollector.Collect(ImmutableArray.Create(hubSymbol), referenceSymbols, TestContext.Current.CancellationToken);
        Assert.True(diagnostics.Count == 0, "Unexpected diagnostics: " + string.Join(Environment.NewLine, diagnostics));
        var hub = Assert.Single(collection.Hubs);

        // Reflection: the same contracts as seen by the server and the dynamic client.
        var hubContract = StreamingHubTransportContract.ForHub<THub>();
        var receiverContract = StreamingHubTransportContract.ForReceiver<TReceiver>();

        Assert.Equal(hubContract.Methods.Count, hub.Methods.Count);
        foreach (var method in hub.Methods)
        {
            Assert.True(hubContract.TryGetMode(method.HubId, out var runtimeMode), $"Hub method '{method.MethodName}' ({method.HubId}) was not found in the runtime contract.");
            Assert.True((int)runtimeMode == (int)method.TransportMode, $"Hub method '{method.MethodName}': Roslyn={method.TransportMode}, Reflection={runtimeMode}");
        }

        Assert.Equal(receiverContract.Methods.Count, hub.Receiver.Methods.Count);
        foreach (var method in hub.Receiver.Methods)
        {
            Assert.True(receiverContract.TryGetMode(method.HubId, out var runtimeMode), $"Receiver method '{method.MethodName}' ({method.HubId}) was not found in the runtime contract.");
            Assert.True((int)runtimeMode == (int)method.TransportMode, $"Receiver method '{method.MethodName}': Roslyn={method.TransportMode}, Reflection={runtimeMode}");
        }

        // The table exchanged during negotiation (unreliable methods only) must be identical as well.
        var roslynUnreliableHubMethods = hub.Methods
            .Where(x => x.TransportMode != TransportMode.Reliable)
            .Select(x => (MethodId: x.HubId, Mode: (int)x.TransportMode))
            .OrderBy(x => x.MethodId)
            .ToArray();
        var runtimeUnreliableHubMethods = hubContract.UnreliableMethods.Entries
            .Select(x => (x.MethodId, Mode: (int)x.Mode))
            .ToArray();
        Assert.Equal(runtimeUnreliableHubMethods, roslynUnreliableHubMethods);

        var roslynUnreliableReceiverMethods = hub.Receiver.Methods
            .Where(x => x.TransportMode != TransportMode.Reliable)
            .Select(x => (MethodId: x.HubId, Mode: (int)x.TransportMode))
            .OrderBy(x => x.MethodId)
            .ToArray();
        var runtimeUnreliableReceiverMethods = receiverContract.UnreliableMethods.Entries
            .Select(x => (x.MethodId, Mode: (int)x.Mode))
            .ToArray();
        Assert.Equal(runtimeUnreliableReceiverMethods, roslynUnreliableReceiverMethods);
    }
}
