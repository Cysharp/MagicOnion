using System.Diagnostics.CodeAnalysis;
using MagicOnion.Internal;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Features.Internal;

internal interface IStreamingHubFeature
{
    MagicOnionManagedGroupProvider GroupProvider { get; }
    IStreamingHubHeartbeatManager HeartbeatManager { get; }
    UniqueHashDictionary<StreamingHubHandler> Handlers { get; }

    /// <summary>Gets the registered additional-transport providers.</summary>
    IReadOnlyList<IAdditionalTransportServerProvider> AdditionalTransportProviders { get; }

    /// <summary>Gets the negotiation options of additional transports.</summary>
    AdditionalTransportServerOptions AdditionalTransportOptions { get; }

    /// <summary>Gets the hub methods whose transport mode is unreliable.</summary>
    StreamingHubTransportModeTable UnreliableHubMethods { get; }

    bool TryGetMethod(int methodId, [NotNullWhen(true)] out StreamingHubHandler? handler);
}
